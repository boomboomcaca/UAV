import React from 'react';
import PropTypes from 'prop-types';
import CaptureList from './CaptureList.jsx';
import useCaptureList from './hooks/useCaptureList';

const CaptureList2 = (props) => {
  const { className, appConfig, request, functionName } = props;

  const { captures, onLoadMore, onDelete, onDownload, deleting, downloading } = useCaptureList(request, functionName);

  return (
    <CaptureList
      className={className}
      baseUrl={appConfig.apiBaseUrl}
      dataSource={captures}
      onLoadMore={onLoadMore}
      onDelete={onDelete}
      onDownload={onDownload}
      deleting={deleting}
      downloading={downloading}
    />
  );
};

CaptureList2.defaultProps = {
  className: null,
  appConfig: null,
  request: null,
  functionName: 'scan',
};

CaptureList2.propTypes = {
  className: PropTypes.any,
  appConfig: PropTypes.any,
  request: PropTypes.any,
  functionName: PropTypes.string,
};

export default CaptureList2;
