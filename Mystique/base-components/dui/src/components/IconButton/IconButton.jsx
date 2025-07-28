import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Loading from '../Loading';
import styles from './index2.module.less';

const IconButton = (props) => {
  const { tag, loading, children, text, className, style, checked, disabled, visible, onClick } = props;

  return (
    <div
      className={classnames(
        text !== '' && text !== undefined && text !== null ? styles.ibbase2 : styles.ibbase1,
        disabled || loading ? styles.ibbaseDisabled : null,
        visible ? null : styles.ibcollapse,
        checked ? styles.ibcontentChecked : null,
        className,
      )}
      title={text}
      style={style}
      onClick={() => {
        if (!disabled && !loading) onClick(checked, tag);
      }}
    >
      <div
        className={classnames(styles.ibindicator, checked ? styles.ibindicator2 : styles.ibindicator1)}
        style={checked === undefined ? { display: 'none' } : disabled ? { opacity: 0.2 } : null}
      />
      <div className={classnames(styles.ibplate)} style={disabled ? { opacity: 0.2 } : null}>
        <div
          className={classnames(
            styles.ibcontent2,
            text === '' || text === undefined || text === null ? styles.ibbb : null,
          )}
        >
          {children}
          {text !== '' && text !== undefined && text !== null ? <div className={styles.ibtext2}>{text}</div> : null}
        </div>
      </div>
      {loading ? <Loading loadingSize={text !== '' && text !== undefined && text !== null ? '24px' : '75%'} /> : null}
    </div>
  );
};

IconButton.defaultProps = {
  tag: '',
  loading: false,
  children: null,
  text: '',
  style: null,
  className: null,
  checked: undefined,
  disabled: false,
  visible: true,
  onClick: () => {},
};

IconButton.propTypes = {
  tag: PropTypes.string,
  loading: PropTypes.bool,
  children: PropTypes.any,
  text: PropTypes.string,
  className: PropTypes.any,
  style: PropTypes.any,
  checked: PropTypes.bool,
  disabled: PropTypes.bool,
  visible: PropTypes.bool,
  onClick: PropTypes.func,
};

export default IconButton;
