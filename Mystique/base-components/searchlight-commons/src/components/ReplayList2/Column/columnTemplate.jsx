import React, { useState } from 'react';
import langT from 'dc-intl';
import classnames from 'classnames';
import { message } from 'dui';
import ExportTag from '../ExportTag';
import Uploading from '../Uploading';
import RemarkEditor from '../RemarkEditor';
import SegmentsInfo from '../SegmentsInfo';
import MScanPoints from '../MScanPoints';
import icons from '../Icon';
import styles from './column.module.less';

export const singleFunc = ['ffm', 'fdf', 'itum', 'wbdf' /* , 'mscan' */, 'iqretri'];
export const segmentFunc = ['scandf'];
export const segmentsFunc = ['scan'];

const columnTemplate = (type, onOperate, onDownload, updateRemark, showPlay = false) => {
  const cols = [];
  const [value, setValue] = useState({
    sourceFile: '',
    val: '',
  });
  const [flag, setFlag] = useState(false);

  if (type === 'single' || singleFunc.includes(type)) {
    cols.push({
      key: 'frequency',
      name: <div className={styles.freqTitle}>{langT('commons', 'freq')}</div>,
      sort: true,
      render: (dat) => {
        const { frequency } = dat;
        return (
          <div className={styles.freqRender}>
            {frequency || '--'}
            <span className={styles.unit}>MHz</span>
          </div>
        );
      },
    });
    cols.push({
      key: 'bandwidth',
      name: (
        // dc-intl + dfBandwidth
        <div className={styles.bandTitle}>
          {langT('commons', type === 'fdf' || type === 'wbdf' ? 'dfBandwidth' : 'ifBandwidth')}
        </div>
      ),
      sort: true,
      render: (dat) => {
        const { bandwidth } = dat;
        return (
          <div className={styles.freqRender}>
            {bandwidth || '--'}
            <span className={styles.unit}>kHz</span>
          </div>
        );
      },
    });
  } else if (type === 'segments' || segmentsFunc.includes(type)) {
    cols.push({
      key: 'segments',
      name: <div className={styles.bandTitle}>{langT('commons', 'segments')}</div>,
      sort: true,
      render: (dat) => {
        const { segments } = dat;
        return <SegmentsInfo segments={segments} />;
      },
    });
  } else if (type === 'segment' || segmentFunc.includes(type)) {
    cols.push({
      key: 'segment',
      name: <div className={styles.bandTitle}>{langT('commons', 'segments')}</div>,
      sort: true,
      render: (dat) => {
        const { segment } = dat;
        return <div>{segment || '--'}</div>;
      },
    });
  } else if (type === 'mscan') {
    cols.push({
      key: 'mscanPoints',
      name: <div className={styles.bandTitle}>频点列表</div>,
      sort: false,
      render: (dat) => {
        const { mscanPoints } = dat;
        // TODO be careful about : value~parameters
        return <MScanPoints points={mscanPoints?.value} />;
      },
    });
  }

  const ret = [
    ...cols,
    {
      key: 'edgeName',
      name: <div className={styles.nameTitle}>{langT('commons', 'stationName')}</div>,
      sort: true,
    },
    {
      key: 'dataStartTime',
      name: <div className={styles.timeTitle}>{langT('commons', 'startTime')}</div>,
      sort: true,
    },
    {
      key: 'timeSpan',
      name: <div className={styles.spanTitle}>{langT('commons', 'timeSpan')}</div>,
      sort: true,
    },
  ];
  if (type !== 'iqretri') {
    ret.push({
      key: 'percentage',
      name: <div className={styles.opTitle}>{langT('commons', 'dataOperate')}</div>,
      style: { width: 240 },
      render: (dat) => {
        const { percentage, params } = dat;
        let saveDataTypeList = null;
        try {
          const ps = JSON.parse(params);
          saveDataTypeList = ps.find((p) => {
            return p.name === 'saveDataTypeList';
          });
        } catch (error) {
          window.console.log(error);
        }
        if (dat?.type === 'iq') {
          return null;
        }
        return (
          <div className={styles.opRender}>
            {percentage === 101 ? (
              <ExportTag
                savedTypes={saveDataTypeList?.value}
                onClick={(tag, condition) => {
                  onDownload(tag, dat, condition);
                }}
              />
            ) : percentage < 0 ? (
              <div
                title={langT('commons', 'waitingForUpload')}
                className={styles.upload}
                onClick={() => {
                  onOperate(dat);
                }}
              >
                {icons.upload}
                {langT('commons', 'waitingForUpload')}
              </div>
            ) : percentage !== 200 ? (
              <Uploading percentage={percentage} />
            ) : (
              langT('commons', 'syncing')
            )}
          </div>
        );
      },
    });
  }

  ret.push({
    key: 'remark',
    style: { width: 250 },
    name: <div className={styles.remTitle}>{langT('commons', 'remark')}</div>,
    render: (dat) => {
      const { remark, sourceFile } = dat;
      return (
        <div className={styles.remRender}>
          <RemarkEditor
            remark={remark}
            flag={flag}
            editing={flag && value.sourceFile === sourceFile}
            value={value.val}
            onChange={(val) =>
              setValue({
                ...value,
                val,
              })
            }
            onBack={() => {
              setFlag(false);
            }}
            onStore={() => {
              if (value.val.length > 10) {
                // message.error('保存失败！最多输入10个字符。');
              } else {
                setFlag(false);
                updateRemark(value);
              }
            }}
            onAdd={() => {
              if (flag) return;
              setValue({ val: '', sourceFile });
              setFlag(true);
            }}
            onEdit={() => {
              if (flag) return;
              setValue({ val: remark, sourceFile });
              setFlag(true);
            }}
          />
        </div>
      );
    },
  });

  if (showPlay) {
    ret.push({
      key: 'play',
      name: <div className={styles.opTitle}>播放</div>,
      style: { width: 120 },
      render: (dat) => {
        const { percentage, type: dtype } = dat;
        if (dtype === 'raw') {
          return (
            <div
              className={classnames(styles.play, percentage !== 101 && type !== 'iqretri' ? styles.playDisable : null)}
            >
              <div
                onClick={() => {
                  onOperate(dat);
                }}
              >
                {icons.play(percentage !== 101 && type !== 'iqretri')}
              </div>
            </div>
          );
        }
        return null;
      },
    });
  }

  return ret;
};

export default columnTemplate;
