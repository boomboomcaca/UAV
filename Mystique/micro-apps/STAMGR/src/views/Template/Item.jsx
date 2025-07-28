import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './item.module.less';

const Item = (props) => {
  const { className, item, closable, onClick } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.info}>
        <div>{item.name}</div>
        <div>{item.version}</div>
      </div>
      {closable ? (
        <div
          className={styles.del}
          onClick={() => {
            onClick(item);
          }}
        />
      ) : null}
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  closable: true,
  onClick: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  closable: PropTypes.bool,
  onClick: PropTypes.func,
};

export default Item;
