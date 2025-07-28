import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import icons from './icons';
import styles from './empty.module.less';

const Empty = (props) => {
  const { className, message, emptype, svg, hideMsg } = props;
  return (
    <div className={classnames(styles.root, className)}>
      {svg || icons[emptype]}
      {!hideMsg && <div className={styles.msg}>{message}</div>}
    </div>
  );
};

Empty.Normal = 'EmptyNormal';
Empty.Feature = 'EmptyFeature';
Empty.Device = 'EmptyDevice';
Empty.Box = 'EmptyBox';
Empty.RunningTask = 'EmptyRunningTask';
Empty.UAV = 'EmptyUAV';
Empty.Station = 'EmptyStation';
Empty.Template = 'EmptyTemplate';

Empty.defaultProps = {
  className: null,
  message: '暂无数据',
  hideMsg: false,
  emptype: Empty.Normal,
  svg: null,
};

Empty.propTypes = {
  className: PropTypes.any,
  message: PropTypes.string,
  emptype: PropTypes.any,
  hideMsg: PropTypes.bool,
  // @svgr/webpack import {ReactComponent as XSVG} from './x.svg';
  svg: PropTypes.any,
};

export default Empty;
