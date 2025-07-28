import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import CaptureList2 from '../CaptureList2.jsx';
import request from '../../../api/request';
import styles from './index.module.less';

const CaptureList2Demo = (props) => {
  const { className } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <CaptureList2
        appConfig={{
          apiBaseUrl: 'http://192.168.102.16:12001',
        }}
        request={request}
        functionName="ffm"
      />
    </div>
  );
};

CaptureList2Demo.defaultProps = {
  className: null,
};

CaptureList2Demo.propTypes = {
  className: PropTypes.any,
};

export default CaptureList2Demo;
