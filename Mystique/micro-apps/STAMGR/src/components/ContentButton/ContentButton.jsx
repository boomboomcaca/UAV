import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Button } from 'dui';
import styles from './index.module.less';

const ContentButton = (props) => {
  const { icon, text, disabled, onClick } = props;
  return (
    <Button onClick={onClick} disabled={disabled}>
      <div className={classnames(styles.root, disabled ? styles.disabled : null)}>
        {icon}
        {text}
      </div>
    </Button>
  );
};

ContentButton.defaultProps = {
  icon: null,
  text: '',
  disabled: false,
  onClick: () => {},
};

ContentButton.propTypes = {
  icon: PropTypes.any,
  text: PropTypes.string,
  disabled: PropTypes.any,
  onClick: PropTypes.func,
};

export default ContentButton;
