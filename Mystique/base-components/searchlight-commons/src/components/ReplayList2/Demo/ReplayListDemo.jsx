import React from 'react';
import ReplayList from '../ReplayList.jsx';
import request from '../../../api/request';
import useReplayList from './useReplayList';
import useNotifilter from './useNotifilter';

const ReplayListDemo = () => {
  const onSelectChange = (a, b, c) => {
    window.console.log(a, b, c);
  };

  const { syncData, setSyncData } = useNotifilter(
    'ws://192.168.102.16:12001/notify',
    '0144d3c6-9103-2d97-8e8e-7ae4afbd5973',
  );

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
  } = useReplayList(
    ReplayList.Segment,
    'ws://192.168.102.16:12001/control',
    '0144d3c6-9103-2d97-8e8e-7ae4afbd5973',
    request,
    setSyncData,
    onSelectChange,
  );

  // scan 0144d3c6-9103-2d97-8e8e-7ae4afbd5973
  // ffm b6fd7a70-b2ea-4927-8cb9-0bb3927e7a62

  return (
    <ReplayList
      type={ReplayList.Segment}
      segments={standardSegments}
      data={replayList}
      syncData={syncData}
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

ReplayListDemo.defaultProps = {};

ReplayListDemo.propTypes = {};

export default ReplayListDemo;
