import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './loading.module.less';

const Loading = (props) => {
  const { className, loadingSize, loadingMsg, loading, vertical, type } = props;
  return loading ? (
    <div className={classnames(styles.root, className)} style={vertical ? { flexDirection: 'column' } : null}>
      <div
        className={classnames(
          type === 'double' ? null : styles.loader,
          type === 'single' ? styles.loading1 : null,
          type === 'colorful' ? styles.loading2 : null,
          type === 'gradient' ? styles.loading3 : null,
          type === 'double' ? styles.loading4 : null,
        )}
        style={{ width: loadingSize, height: loadingSize }}
      />
      {loadingMsg === '' ? null : <div className={styles.loadmsg}>{loadingMsg}</div>}
    </div>
  ) : null;
};

Loading.defaultProps = {
  className: null,
  loadingSize: 24,
  loadingMsg: '',
  loading: true,
  vertical: false,
  // colorful,single,gradient,double
  type: 'gradient',
};

Loading.propTypes = {
  className: PropTypes.any,
  loadingSize: PropTypes.any,
  loadingMsg: PropTypes.string,
  loading: PropTypes.bool,
  vertical: PropTypes.bool,
  type: PropTypes.any,
};

export default Loading;
