import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import CaptureList from '..';
import request from '../../../api/request';
import useCaptureList from './useCaptureList';
import styles from './index.module.less';

const CaptureListDemo = (props) => {
  const { className } = props;

  const { captures, onLoadMore, onDelete, onDownload } = useCaptureList(request);

  return (
    <div className={classnames(styles.root, className)}>
      <CaptureList
        baseUrl="http://192.168.102.16:12001"
        dataSource={captures}
        onLoadMore={onLoadMore}
        onDelete={onDelete}
        onDownload={onDownload}
      />
    </div>
  );
};

CaptureListDemo.defaultProps = {
  className: null,
};

CaptureListDemo.propTypes = {
  className: PropTypes.any,
};

export default CaptureListDemo;
