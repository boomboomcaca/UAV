import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './item.module.less';

const Item = (props) => {
  const { className, item, checked, error, onDelete, onClick } = props;

  return (
    <div
      className={classnames(styles.root, className)}
      onClick={() => {
        onClick(item);
      }}
    >
      <div className={classnames(styles.content, checked ? styles.checked : null, error ? styles.error : null)}>
        <div>{item?.name}</div>
        <div>{item?.version && `V ${item?.version}`}</div>
      </div>
      <div
        className={styles.del}
        onClick={() => {
          onDelete(item);
        }}
      />
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  checked: false,
  error: undefined,
  onDelete: () => {},
  onClick: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  checked: PropTypes.bool,
  error: PropTypes.any,
  onDelete: PropTypes.func,
  onClick: PropTypes.func,
};

export default Item;
