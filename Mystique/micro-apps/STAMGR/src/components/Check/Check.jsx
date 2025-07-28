import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const svg = (
  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="8" viewBox="0 0 10 8" fill="none">
    <path
      d="M9 1L3.74795 7L1 3.86779"
      stroke="#353D5B"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const Check = (props) => {
  const { checked, onCheck } = props;
  return (
    <div className={classnames(styles.root, checked && styles.chk)} onClick={onCheck}>
      {checked ? svg : null}
    </div>
  );
};

Check.defaultProps = {
  checked: false,
  onCheck: () => {},
};

Check.propTypes = {
  checked: PropTypes.bool,
  onCheck: PropTypes.func,
};

export default Check;
