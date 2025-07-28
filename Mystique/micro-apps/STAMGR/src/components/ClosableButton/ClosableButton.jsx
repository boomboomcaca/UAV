import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const ClosableButton = (props) => {
  const { activate, content, onClose, onActive } = props;
  return (
    <div className={classnames(styles.root, activate ? styles.active : null)} onClick={onActive}>
      <span className={styles.content}>{content}</span>
      <span
        onClick={(e) => {
          e.stopPropagation();
          onClose();
        }}
        className={styles.close}
      >
        <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M1 1L11 11M11 1L1 11"
            stroke="white"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </span>
    </div>
  );
};

ClosableButton.defaultProps = {
  activate: false,
  content: null,
  onClose: () => {},
  onActive: () => {},
};

ClosableButton.propTypes = {
  activate: PropTypes.bool,
  content: PropTypes.any,
  onClose: PropTypes.func,
  onActive: PropTypes.func,
};

export default ClosableButton;
