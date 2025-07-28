import React, { useContext, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Context from './context';
import { UPDATE } from './reduser';
import styles from './field.module.less';

const Field = (props) => {
  const { isGroupItem, children, rules, label, name, map, style, controlled, onValidated, onChange } = props;

  const context = useContext(Context);
  const {
    state: { data, validate, labelStyle },
    dispatch,
  } = context;

  const [error, setError] = useState(null);

  const toValidate = (value) => {
    let bo = true;
    let msg = '';
    if (rules) {
      for (let i = 0; i < rules.length; i += 1) {
        const { required, all, keys, func, message } = rules[i];
        if (required) {
          const isArr = Array.isArray(value);
          if (isArr) {
            const hasVal = (vs) => {
              let ret = all === true;
              for (let j = 0; j < vs.length; j += 1) {
                if (all) {
                  if (keys.includes(vs[0]) || vs[0] === null || vs[0] === undefined) {
                    if (vs[j] === undefined || vs[j] === null) {
                      ret = false;
                      break;
                    }
                  }
                } else if (vs[j]) {
                  ret = true;
                  break;
                }
              }
              return ret;
            };
            bo = value.length > 0 && hasVal(value);
          } else {
            bo = !(value === undefined || value === null || value === '');
          }
        } else {
          bo = func?.(value);
        }
        if (!bo) {
          let msgIdx = 0;
          if (Array.isArray(value) && keys && keys.includes(value[0])) {
            msgIdx = 1;
          }
          msg = Array.isArray(message) ? message[msgIdx] : message;
          break;
        }
      }
    }
    setError(!bo ? msg : null);
    return bo;
  };

  useEffect(() => {
    // toString.call(name) === '[object Array]';
    const isArr = Array.isArray(name);
    const getValues = (d, keys) => {
      return keys.map((k) => {
        return d[k];
      });
    };
    if (validate) {
      const bo = toValidate(isArr ? getValues(data, name) : data?.[name]);
      onValidated(name, bo);
    }
  }, [validate]);

  const getControlled = () => {
    if (controlled === true) {
      const getValues = (d, keys) => {
        const ret = [];
        for (let i = 0; i < keys.length; i += 1) {
          const key = keys[i];
          if (d[key]) {
            ret.push({ [map]: d[key] });
          }
        }
        return ret;
      };
      return {
        value: data?.[name],
        values: Array.isArray(name) && data ? getValues(data, name) : null,
        onChange: (e) => {
          const newValue = e.target ? e.target.value : e;
          dispatch({ type: UPDATE, payload: { [name]: newValue } });
          const bo = toValidate(newValue);
          onChange(name, newValue);
          // if (bo) {
          // }
        },
        onSelectValue: (vals) => {
          if (Array.isArray(name)) {
            name.forEach((v, i) => {
              dispatch({ type: UPDATE, payload: { [v]: vals?.[i]?.[map] || null } });
            });
            const newVal = name.map((_v, i) => {
              return vals?.[i]?.[map] || null;
            });
            const bo = toValidate(newVal);
            if (bo) {
              onChange(name, newVal);
            }
          } else {
            const newVal = vals.map((v) => {
              return v[map];
            });
            const bo = toValidate(newVal);
            if (bo) {
              onChange(name, newVal);
            }
          }
        },
      };
    }
    // TODO lackof:数据不变型的情况
    const { control, transform } = controlled();
    return {
      ...control,
      onSelectValue: (vals) => {
        control.onSelectValue(vals);
        const tran = transform(vals);
        dispatch({ type: UPDATE, payload: tran });
        const vs = name.map((n) => {
          return tran[n];
        });
        const bo = toValidate(vs);
        if (bo) {
          onChange(name, vs);
        }
      },
    };
  };

  return (
    <div className={classnames(styles.root, isGroupItem ? styles.itemroot : null)}>
      <div className={classnames(styles.label, isGroupItem ? styles.itemlabel : null)} style={labelStyle}>
        {label}
      </div>
      <div className={styles.child}>
        {children ? (
          React.cloneElement(children, getControlled())
        ) : (
          <div className={styles.readonly} style={style}>
            {data?.[name]}
          </div>
        )}
        <div className={classnames(styles.error, error !== null && styles.show)}>{error}</div>
      </div>
    </div>
  );
};

Field.defaultProps = {
  isGroupItem: false,
  children: null,
  rules: null,
  label: '',
  name: '',
  map: '',
  style: null,
  controlled: true,
  onValidated: () => {},
  onChange: () => {},
};

Field.propTypes = {
  isGroupItem: PropTypes.bool,
  children: PropTypes.any,
  rules: PropTypes.string,
  label: PropTypes.string,
  name: PropTypes.string,
  map: PropTypes.string,
  style: PropTypes.any,
  controlled: PropTypes.any,
  onValidated: PropTypes.func,
  onChange: PropTypes.func,
};

Field.tag = 'field';

export default Field;
