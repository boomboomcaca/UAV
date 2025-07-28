import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Icon from '@ant-design/icons';
import AddSeg from './AddSeg/index.jsx';

import styles from './single.module.less';

const SegmentsEditor = (props) => {
  const { callbackseg, treeData, tableData, onTreeSelect, segmentData, editable } = props;

  const [showSelectSeg, setshowSelectSeg] = useState(false);

  const onok = (newSeg) => {
    setshowSelectSeg(false);
    callbackseg(newSeg);
  };

  return (
    <div style={{ display: 'inline-block' }}>
      <div className={styles.singleseg}>
        {segmentData.name ? (
          <div className={styles.singleleft}>
            <div className={styles.name}>{segmentData.name}</div>
            <div className={styles.seg}>
              {segmentData.startFrequency}MHz - {segmentData.stopFrequency}
              MHz
            </div>
          </div>
        ) : (
          <div className={classnames(styles.singleleft, styles.onlyseg)}>
            {segmentData.startFrequency}MHz - {segmentData.stopFrequency}
            MHz
          </div>
        )}

        {editable && (
          <div className={styles.singleright} onClick={() => setshowSelectSeg(true)}>
            <Icon component={ChangeSvg} />
          </div>
        )}
      </div>
      <AddSeg
        visible={showSelectSeg}
        treeData={treeData}
        tableData={tableData}
        onOpen={() => setshowSelectSeg(true)}
        onCancel={() => setshowSelectSeg(false)}
        onTreeSelect={onTreeSelect}
        segmentData={segmentData}
        onSelectChange={(newSeg) => onok(newSeg)}
      />
    </div>
  );
};

SegmentsEditor.defaultProps = {
  segmentData: {},
  treeData: [
    {
      key: 'origin',
      name: '根节点',
    },
  ],
  tableData: [],
  onTreeSelect: () => {},
  callbackseg: () => {},
  editable: true,
};

SegmentsEditor.propTypes = {
  segmentData: PropTypes.object,
  treeData: PropTypes.array, // 频段池的树结构数据
  tableData: PropTypes.array, // 频段列表
  onTreeSelect: PropTypes.func,
  callbackseg: PropTypes.func,
  editable: PropTypes.bool,
};

const ChangeSvg = () => (
  <svg width="25" height="24" viewBox="0 0 25 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity="0.6">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M2.75 5C2.75 4.58579 3.08579 4.25 3.5 4.25H11C11.4142 4.25 11.75 4.58579 11.75 5C11.75 5.41421 11.4142 5.75 11 5.75H4.25V18.25H7.5C7.91421 18.25 8.25 18.5858 8.25 19C8.25 19.4142 7.91421 19.75 7.5 19.75H3.5C3.08579 19.75 2.75 19.4142 2.75 19V5ZM16.75 5C16.75 4.58579 17.0858 4.25 17.5 4.25H21.5C21.9142 4.25 22.25 4.58579 22.25 5V19C22.25 19.4142 21.9142 19.75 21.5 19.75H14C13.5858 19.75 13.25 19.4142 13.25 19C13.25 18.5858 13.5858 18.25 14 18.25H20.75V5.75H17.5C17.0858 5.75 16.75 5.41421 16.75 5ZM14.2745 16.7153C13.9623 16.6169 13.75 16.3274 13.75 16V5C13.75 4.58579 14.0858 4.25 14.5 4.25C14.9142 4.25 15.25 4.58579 15.25 5L15.25 13.6207L15.8856 12.7128C16.1231 12.3734 16.5908 12.2909 16.9301 12.5284C17.2694 12.766 17.352 13.2336 17.1144 13.573L15.1144 16.4301C14.9267 16.6983 14.5867 16.8137 14.2745 16.7153ZM11.25 8C11.25 7.67265 11.0377 7.38311 10.7255 7.2847C10.4133 7.18628 10.0733 7.30173 9.88558 7.5699L7.88558 10.427C7.64804 10.7664 7.73057 11.234 8.0699 11.4716C8.40924 11.7091 8.87689 11.6266 9.11442 11.2872L9.75 10.3793V19C9.75 19.4142 10.0858 19.75 10.5 19.75C10.9142 19.75 11.25 19.4142 11.25 19V8Z"
        fill="var(--theme-font-100)"
      />
    </g>
  </svg>
);

export default SegmentsEditor;
