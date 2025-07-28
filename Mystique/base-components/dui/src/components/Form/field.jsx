/* eslint-disable no-await-in-loop */
/* eslint-disable no-async-promise-executor */
import React from 'react';
import RawAsyncValidator from 'async-validator';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import FieldContext from './context';
import styles from './styles.module.less';

RawAsyncValidator.warning = () => {};

class Field extends React.Component {
  static contextType = FieldContext;

  cancelRegisterFunc;

  validatePromise = null;

  errors = [];

  notFirstValid = false;

  componentDidMount() {
    const { registerField } = this.context;
    this.cancelRegisterFunc = registerField(this);
  }

  componentWillUnmount() {
    if (this.cancelRegisterFunc) {
      this.cancelRegisterFunc();
    }
  }

  getRules = () => {
    const { rules = [] } = this.props;
    return rules.map((rule) => {
      if (typeof rule === 'function') {
        return rule(this.context);
      }
      return rule;
    });
  };

  validateRule = async (name, value, rule) => {
    const cloneRule = { ...rule };
    const validator = new RawAsyncValidator({
      [name]: [cloneRule],
    });
    let result = [];
    try {
      await Promise.resolve(validator.validate({ [name]: value }));
    } catch (e) {
      if (e.errors) {
        result = e.errors.map((c) => c.message);
      }
    }
    return result;
  };

  executeValidate = (namePath, value, rules) => {
    const summaryPromise = new Promise(async (resolve, reject) => {
      for (let i = 0; i < rules.length; i += 1) {
        const errors = await this.validateRule(namePath, value, rules[i]);
        if (errors.length) {
          reject(errors);
          return;
        }
      }
      resolve([]);
    });
    return summaryPromise;
  };

  validateRules = (value) => {
    this.notFirstValid = true;
    const { getFieldValue } = this.context;
    const { name } = this.props;
    const currentValue = value === undefined ? getFieldValue(name) : value;
    const rootPromise = Promise.resolve().then(() => {
      const filteredRules = this.getRules();
      const promise = this.executeValidate(name, currentValue, filteredRules);
      promise
        .catch((e) => e)
        .then((errors = []) => {
          if (this.validatePromise === rootPromise) {
            this.validatePromise = null;
            this.errors = errors;
            this.forceUpdate();
          }
        });
      return promise;
    });
    this.validatePromise = rootPromise;
    return rootPromise;
  };

  onStoreChange = (prevStore, curStore) => {
    const { shouldUpdate } = this.props;
    if (typeof shouldUpdate === 'function') {
      if (shouldUpdate(prevStore, curStore)) {
        this.forceUpdate();
      }
    } else {
      this.forceUpdate();
    }
  };

  getControlled = () => {
    const { name, children } = this.props;
    const { getFieldValue, setFieldsValue } = this.context;
    return {
      value: getFieldValue(name),
      onChange: (event) => {
        const newValue = event.target ? event.target.value : event;
        if (this.notFirstValid) {
          this.validateRules(newValue);
        }
        setFieldsValue({ [name]: newValue });
        children.props.onChange?.(newValue);
      },
    };
  };

  render() {
    const { children, label, labelWidth, labelOffset, name, style } = this.props;
    const { labelWidth: formLabelWidht } = this.context;
    const offset = labelOffset === true ? formLabelWidht : labelOffset;
    if (label === '') {
      return (
        <div className={styles.noLabelFiled} style={{ paddingLeft: offset }}>
          <div>{name === '' ? children : React.cloneElement(children, this.getControlled())}</div>
          <div className={styles.error}>{this.errors.join(',')}</div>
        </div>
      );
    }
    return (
      <div className={classnames(styles.field)} style={style}>
        <div className={styles.label} style={{ width: labelWidth || formLabelWidht }}>
          <span>{label}</span>
        </div>
        <div className={styles.controlled}>
          <div>{name === '' ? children : React.cloneElement(children, this.getControlled())}</div>
          <div className={styles.error}>{this.errors.join(',')}</div>
        </div>
      </div>
    );
  }
}

Field.defaultProps = {
  children: null,
  name: '',
  label: '',
  style: null,
  rules: [],
  shouldUpdate: null,
  labelWidth: null,
  labelOffset: null,
};

Field.propTypes = {
  children: PropTypes.any,
  name: PropTypes.string,
  style: PropTypes.string,
  label: PropTypes.string,
  rules: PropTypes.array,
  shouldUpdate: PropTypes.func,
  labelWidth: PropTypes.string,
  labelOffset: PropTypes.oneOfType([PropTypes.string, PropTypes.bool]),
};

export default Field;
