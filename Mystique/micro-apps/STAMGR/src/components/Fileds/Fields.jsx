import React, { useReducer, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import Context from './context';
import { defaultState, reducer, UPDATE, VALIDATE, LABELSTYLE } from './reduser';
import styles from './index.module.less';

const Fields = (props) => {
  const { children, data, labelStyle, onInitialized, onChange } = props;

  const errorsRef = useRef({});
  const fieldsRef = useRef(0);
  const newDataRef = useRef(null);

  const [state, dispatch] = useReducer(reducer, defaultState);

  useEffect(() => {
    dispatch({ type: UPDATE, payload: data });
  }, [data]);

  useEffect(() => {
    dispatch({ type: LABELSTYLE, payload: labelStyle });
  }, [labelStyle]);

  useEffect(() => {
    newDataRef.current = state.data;
  }, [state]);

  const validateFields = () => {
    dispatch({ type: VALIDATE });
    const pro = new Promise((resolve, reject) => {
      const run = () => {
        if (Object.keys(errorsRef.current).length < fieldsRef.current) {
          run();
        } else {
          const { current: errors } = errorsRef;
          let hasError = false;
          const keys = Object.keys(errors);
          for (let i = 0; i < keys.length; i += 1) {
            const key = keys[i];
            if (errors[key] === false) {
              hasError = true;
              break;
            }
          }
          if (hasError) {
            reject(new Error('验证失败!'));
          } else {
            resolve(newDataRef.current);
          }
        }
      };
      setTimeout(run, 20);
    });
    return pro;
  };

  useEffect(() => {
    onInitialized(validateFields);
  }, []);

  const onValidated = (name, bo) => {
    errorsRef.current[JSON.stringify(name)] = bo;
  };

  const getChildren = () => {
    const cs = Array.isArray(children) ? children : [children];
    fieldsRef.current = 0;

    return cs.map((child) => {
      if (child && child.type.tag === 'field') {
        fieldsRef.current += 1;
        return React.cloneElement(child, {
          ...child.props,
          onValidated,
          onChange,
        });
      }
      if (child && child.type.tag === 'group') {
        fieldsRef.current += child.props.children.length;
        const newp = {
          ...child.props,
          children: child.props.children.map((c) => {
            return React.cloneElement(c, { ...c.props, onValidated, onChange, isGroupItem: true });
          }),
        };
        const clone = React.cloneElement(child, newp);
        return clone;
      }
      return child;
    });
  };

  return (
    <div className={styles.root}>
      <Context.Provider value={{ state, dispatch }}>{getChildren()}</Context.Provider>
    </div>
  );
};

Fields.defaultProps = {
  children: null,
  data: null,
  labelStyle: null,
  onInitialized: () => {},
  onChange: () => {},
};

Fields.propTypes = {
  children: PropTypes.any,
  data: PropTypes.any,
  labelStyle: PropTypes.number,
  onInitialized: PropTypes.func,
  onChange: PropTypes.func,
};

export default Fields;
