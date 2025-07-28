import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ArrowLeft2Icon } from 'dc-icon';
import { Drawer, PopUp } from 'dui';
import Icon from '@ant-design/icons';
import styles from './style.module.less';

const AddSeg = (props) => {
  const { visible, onOpen, onCancel, onSelectChange, treeData, tableData, onTreeSelect, segmentData } = props;

  const [selectId, setId] = useState();
  const [show, setShow] = useState(false);
  const [selLeaf, setSelLeaf] = useState({});

  const selectType = (type) => {
    onTreeSelect(type.id, type);
    setSelLeaf(type);
    onCancel();
    setShow(true);
  };

  const segReturn = () => {
    setShow(false);
    onOpen();
  };

  const segClick = (item) => {
    setShow(false);
    onSelectChange({
      id: item.id,
      pid: selLeaf.id,
      ppid: selectId,
      name: `${selLeaf.name} - ${item.name}`,
      startFrequency: item.startFreq,
      stopFrequency: item.stopFreq,
      stepFrequency: item.bandwidth,
    });
  };

  return (
    <>
      <Drawer visible={visible} width="440px" title="频段选择" onCancel={onCancel} bodyStyle={{ padding: '0' }}>
        {treeData instanceof Array &&
          treeData?.map((item) => (
            <div key={item.id}>
              <div
                className={classnames(styles.menu1, { [styles.active]: item.id === selectId })}
                onClick={() => setId(item.id)}
              >
                <div className={classnames({ [styles.activeColor]: segmentData.ppid === item.id })}>{item.name}</div>
                {selectId !== item.id && <Icon component={arrowSvg} />}
              </div>
              {item.id === selectId &&
                item.segmentType &&
                item.segmentType instanceof Array &&
                item.segmentType.map((type) => (
                  <div key={type.id} className={styles.menu2} onClick={() => selectType(type)}>
                    <div className={classnames({ [styles.activeColor]: segmentData.pid === type.id })}>{type.name}</div>
                    <Icon component={arrowRightSVG} />
                  </div>
                ))}
            </div>
          ))}
      </Drawer>

      <PopUp visible={show} onCancel={() => setShow(false)}>
        <div className={styles.segpop}>
          <div className={styles.ct}>
            <div className={styles.hd}>
              <div className={styles.abicon} onClick={segReturn}>
                <ArrowLeft2Icon color="#3ce5d3" />
              </div>
              <span>{selLeaf.name}</span>
            </div>
            <div className={styles.bd}>
              {tableData.map((item) => (
                <div
                  key={item.id}
                  onClick={() => segClick(item)}
                  className={classnames(styles.segitem, { [styles.activeColor]: item.id === segmentData.id })}
                >
                  <span className={styles.ntext}>{item.startFreq}</span>
                  {` MHz - `}
                  <span className={styles.ntext}>{item.stopFreq}</span>
                  {` MHz @ `}
                  <span className={styles.ntext}>{item.bandwidth}</span>
                  {` kHz`}
                </div>
              ))}
            </div>
          </div>
        </div>
      </PopUp>
    </>
  );
};

AddSeg.defaultProps = {
  visible: false,
  onOpen: () => {},
  onCancel: () => {},
  onSelectChange: () => {},
  treeData: [],
  tableData: [],
  onTreeSelect: () => {},
  segmentData: {},
};

AddSeg.propTypes = {
  visible: PropTypes.bool,
  onOpen: PropTypes.func,
  onCancel: PropTypes.func,
  onSelectChange: PropTypes.func,
  treeData: PropTypes.array,
  tableData: PropTypes.array,
  segmentData: PropTypes.object,
  onTreeSelect: PropTypes.func, // 选择树的时候触发
};

const arrowSvg = () => (
  <svg width="10" height="8" viewBox="0 0 10 8" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M4.17801 6.81355C4.57569 7.38756 5.42431 7.38756 5.82199 6.81355L9.10877 2.06949C9.56825 1.40629 9.0936 0.5 8.28678 0.5L1.71322 0.5C0.906401 0.5 0.431746 1.40629 0.891226 2.06949L4.17801 6.81355Z"
      fill="var(--theme-font-30)"
    />
  </svg>
);

const arrowRightSVG = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity="0.3">
      <path
        d="M11 16L15.2427 11.7573L11 7.51468"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </g>
  </svg>
);

export default AddSeg;
