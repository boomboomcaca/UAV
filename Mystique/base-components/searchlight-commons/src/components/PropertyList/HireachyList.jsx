import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import ListItem from './ListItem.jsx';
import styles from './index.module.less';

const HireachyList = (props) => {
  const { param, onValueChanged } = props;

  const [values, setValues] = useState([]);

  useEffect(() => {
    if (param) {
      setValues(param.parameters);
    }
  }, [param]);

  const OnValueChanged = (p, v, i) => {
    values[i][p.name] = v;
    setValues([...values]);
  };

  const onClick = (tag) => {
    if (tag === 'add') {
      const fakeData = {};
      param.template.forEach((temp) => {
        fakeData[temp.name] = temp.default;
      });
      setValues([...values, fakeData]);
    }
    if (tag === 'str') {
      onValueChanged(param, values);
    }
    if (tag === 'clr') {
      setValues([]);
    }
  };

  const onDeleteItem = (i) => {
    values.splice(i, 1);
    setValues([...values]);
  };

  return (
    <div className={styles.hirePan}>
      <div className={styles.hireBtn}>
        {[
          { label: '添加', tag: 'add' },
          { label: '完成', tag: 'str' },
          { label: '清空', tag: 'clr' },
        ].map((b) => {
          return (
            <div
              key={b.tag}
              onClick={() => {
                onClick(b.tag);
              }}
            >
              {b.label}
            </div>
          );
        })}
      </div>
      {values?.map((val, i) => {
        return (
          // eslint-disable-next-line react/no-array-index-key
          <div key={`${val[0]}-${i}`} className={styles.hireItm}>
            <div
              className={styles.hireCls}
              onClick={() => {
                onDeleteItem(i);
              }}
            />
            {param.template.map((t, idx) => {
              const px = { ...t, value: val[t.name] };
              return (
                <div key={px.name} className={styles.groupItem2} style={{ marginTop: idx === 0 ? 24 : 12 }}>
                  <span style={{ marginRight: 8 }}>{`${px.displayName}`}</span>
                  <ListItem
                    param={px}
                    styled={false}
                    onValueChanged={(p, v) => {
                      OnValueChanged(p, v, i);
                    }}
                  />
                </div>
              );
            })}
          </div>
        );
      })}
    </div>
  );
};

HireachyList.defaultProps = { param: null, onValueChanged: () => {} };

HireachyList.propTypes = {
  param: PropTypes.any,
  onValueChanged: PropTypes.func,
};

export default HireachyList;
