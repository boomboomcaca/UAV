import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Select from '@/components/Select';
import styles from './item.module.less';

// TODO 需要简化

const { Option } = Select;

const Item = (props) => {
  const { className, item, multiple, onClick, onSelect } = props;

  const [select, setSelect] = useState(null);

  useEffect(() => {
    setSelect(item?.data?.[0]);
  }, [JSON.stringify(item)]);

  const onSelectChange = (val) => {
    if (item && multiple) {
      const find = item.data.find((d) => {
        return d.id === val;
      });
      setSelect(find);
      onSelect(find);
    }
  };

  return (
    <div
      className={classnames(styles.root, className)}
      onClick={() => {
        onClick(multiple ? select : item);
      }}
      style={multiple ? { userSelect: 'none' } : null}
    >
      <div className={styles.content}>
        <div>{item?.name}</div>
        {multiple ? (
          <Select clickStop value={select?.id} onChange={onSelectChange} className={styles.select}>
            {item.data.map((d) => {
              return (
                <Option value={d.id} key={d.id}>
                  {d.version}
                </Option>
              );
            })}
          </Select>
        ) : (
          <div>{item?.version && `V ${item?.version}`}</div>
        )}
      </div>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  multiple: false,
  onClick: () => {},
  onSelect: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  multiple: PropTypes.bool,
  onClick: PropTypes.func,
  onSelect: PropTypes.func,
};

export default Item;
