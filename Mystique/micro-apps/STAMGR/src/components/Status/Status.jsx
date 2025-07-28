import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import status from './data';
import styles from './index.module.less';

const Status = (props) => {
  const { values, className } = props;
  return (
    <div className={classnames(styles.root, className)}>
      {status.map((s) => {
        const count = (values && values[s.key]) || 0;
        return (
          <div
            className={classnames(styles.item, count < 10 ? styles.sitem : null)}
            // style={count > 0 || s.key === 'unknown' ? null : { display: 'none' }}
            style={s.visible === false ? { display: 'none' } : null}
          >
            <img src={s.png} alt="" />
            {s.tag}
            <span>{count}</span>
          </div>
        );
      })}
    </div>
  );
};

Status.defaultProps = {
  values: null,
  className: null,
};

Status.propTypes = {
  values: PropTypes.any,
  className: PropTypes.any,
};

export default Status;
