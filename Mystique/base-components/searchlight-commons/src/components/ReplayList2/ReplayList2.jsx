import React from 'react';
import PropTypes from 'prop-types';
import ReplayList from './ReplayList.jsx';
import useReplayList from './hooks/useReplayList';
import useNotifilter from './hooks/useNotifilter';

const ReplayList2 = (props) => {
  const { className, appConfig, listType, request, onSelectChange, showPlay, footerClassName } = props;

  const { syncData, setSyncData } = useNotifilter(appConfig, showPlay);

  const {
    replayRefresh,
    standardSegments,
    replayList,
    onDeleteReplayItem,
    onPageChange,
    onPlayback,
    onPlaysync,
    onSearchChanged,
    onTimeChange,
    updateRemark,
    onDownload,
  } = useReplayList(listType, appConfig, request, setSyncData, onSelectChange, showPlay);

  return (
    <ReplayList
      className={className}
      footerClassName={footerClassName}
      type={listType}
      segments={standardSegments}
      data={replayList}
      syncData={syncData}
      showPlay={showPlay}
      onDeleteItems={onDeleteReplayItem}
      onPageChange={onPageChange}
      onPlayback={onPlayback}
      onPlaysync={onPlaysync}
      refreshKey={replayRefresh}
      onSearchChanged={onSearchChanged}
      onTimeChange={onTimeChange}
      updateRemark={updateRemark}
      onDownload={onDownload}
    />
  );
};

ReplayList2.defaultProps = {
  className: null,
  footerClassName: null,
  appConfig: null,
  request: null,
  // ffm fdf scan mscan wbdf itum scandf ...
  listType: ReplayList.Segments,
  showPlay: false,
  onSelectChange: () => {},
};

ReplayList2.propTypes = {
  className: PropTypes.any,
  footerClassName: PropTypes.any,
  appConfig: PropTypes.any,
  request: PropTypes.any,
  listType: PropTypes.string,
  showPlay: PropTypes.bool,
  onSelectChange: PropTypes.func,
};

export default ReplayList2;
