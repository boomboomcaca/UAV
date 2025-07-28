/* eslint-disable react/destructuring-assignment */
import React from 'react';
import { DeleteIcon } from 'dc-icon';
import { Input } from 'dui';
import langT from 'dc-intl';
import TagButton from '../TagButton';
import Select from '../Select/index';
import HalfSpan from '../HalfSpan';

import styles from './column.module.less';

import Picon from './p.png';
import Ficon from './f.png';
import IFIcon1 from './IFIcon1.png';
import DFIcon1 from './DFIcon1.png';

const { Option } = Select;

const getHexValue = (color, opacity = 1) => {
  try {
    const a = parseInt(color.a * opacity, 10);
    const hA = a.toString(16);
    const hR = color.r.toString(16);
    const hG = color.g.toString(16);
    const hB = color.b.toString(16);
    const hAs = a < 16 ? `0${hA}` : hA;
    const hRs = color.r < 16 ? `0${hR}` : hR;
    const hGs = color.g < 16 ? `0${hG}` : hG;
    const hBs = color.b < 16 ? `0${hB}` : hB;
    return `#${hRs}${hGs}${hBs}${hAs}`;
  } catch (error) {
    // window.console.log(error);
  }
  return '#FFFFFF';
};

const columns = (yUnit, type, onMarkerChanged) => {
  let cols = [
    {
      key: 'name',
      name: langT('commons', 'xuhao'),
      render: (record) => {
        return (
          <div className={styles.cell}>
            <div className={styles.index}>{record.mk}</div>
            <div key={record.id} style={{ color: getHexValue(record.color) }}>
              <div>{record.name}</div>
            </div>
          </div>
        );
      },
    },
    {
      key: 'mode',
      name: `marker${langT('scan', '8024')}`,
      render: (record) => (
        <div style={{ width: '100%', height: '100%', backgroundColor: '#04051B' }}>
          <TagButton
            selectedItem={record.peak ? 'Peak' : 'Fixed'}
            selections={[
              {
                label: 'Peak',
                content: (
                  <>
                    <img src={Picon} alt="pIcon" style={{ marginRight: '4px' }} />
                    Peak
                  </>
                ),
              },
              {
                label: 'Fixed',
                content: (
                  <>
                    <img src={Ficon} alt="pIcon" style={{ marginRight: '4px' }} />
                    Fixed
                  </>
                ),
              },
            ]}
            onSelectionChanged={(idx, item) => {
              onMarkerChanged({ tag: 'mode', record, more: { idx, item } });
            }}
          />
        </div>
      ),
    },
    {
      key: 'x',
      name: (
        <div>
          <span>X</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>(MHz)</span>
        </div>
      ),
      style: { width: 180 },
      render: (record) => (
        <div
          style={{
            width: '100%',
            height: '100%',
            position: 'relative',
          }}
        >
          <HalfSpan className={record.FC ? styles.xx : styles.x} value={record.frequency?.toFixed(record.FC ? 6 : 3)} />
          <div
            className={record.FC ? styles.fcSelect : styles.fc}
            onClick={(e) => {
              e.stopPropagation();
              onMarkerChanged({ tag: 'fc', record });
            }}
          >
            fc
          </div>
        </div>
      ),
    },
    {
      key: 'y',
      name: (
        <div>
          <span>Y</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>{`(${yUnit})`}</span>
        </div>
      ),
      render: (record) => {
        const txt = (type ? record.level?.toFixed(3) : record.y?.toFixed(3)) || '--';
        return <div style={{ textAlign: 'right' }}>{txt}</div>;
      },
    },
  ];

  cols = cols.concat([
    {
      key: 'refMK',
      name: 'Ref-MK',
      render: (record) => (
        <div
          style={{
            width: '100%',
            height: '100%',
            backgroundColor: '#04051B',
            display: 'flex',
            alignItems: 'center',
          }}
        >
          <Select
            clickStop
            style={{ width: '100%' }}
            value={record.refMK?.id || -1}
            onChange={(val) => {
              onMarkerChanged({ tag: 'ref', record, more: { id: val } });
            }}
            className={styles.select}
          >
            {record.refs?.map((d) => {
              return (
                <Option value={d.id} key={d.id}>
                  <div>
                    <span>{d.name}</span>
                    {d.segIndex && d.segIndex > 0 ? (
                      <span style={{ fontSize: 12, opacity: 0.5, marginLeft: 10 }}>{`${getTag(d.segIndex)}`}</span>
                    ) : null}
                  </div>
                </Option>
              );
            })}
          </Select>
        </div>
      ),
    },
    {
      key: 'deltaX',
      name: (
        <div>
          <span>△X</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>(MHz)</span>
        </div>
      ),
      render: (record) => <div style={{ textAlign: 'right' }}>{record.deltaX?.toFixed(record.FC ? 6 : 3)}</div>,
    },
    {
      key: 'deltaY',
      name: (
        <div>
          <span>△Y</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>{`(${yUnit})`}</span>
        </div>
      ),
      render: (record) => <div style={{ textAlign: 'right' }}>{record.deltaY?.toFixed(3)}</div>,
    },
  ]);
  if (type) {
    cols.push({
      key: 'deltaT',
      name: (
        <div>
          <span>△T</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>(s)</span>
        </div>
      ),
      render: (record) => <div style={{ textAlign: 'right' }}>{record.showRef ? record.deltaT : ''}</div>,
    });
  }
  cols.push({
    key: 'op',
    name: '操作',
    style: { width: 230 },
    render: (record) => (
      <>
        <label
          style={{
            width: '50%',
            height: '100%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-end',
            paddingRight: '12px',
          }}
          className={styles.btnlabelCont}
          onClick={(e) => {
            e.stopPropagation();
            let checked = false;
            if (e.target) {
              try {
                checked = e.target.querySelector('input').checked;
              } catch (err) {
                console.error(err);
              }
            }
            if (checked) {
              onMarkerChanged({ tag: 'openIF', record });
            } else {
              onMarkerChanged({ tag: 'closeIF', record });
            }
          }}
        >
          <input type="checkbox" style={{ display: 'none' }} value="if" name="toWhere" />
          <div className={`${styles.operateIcon} ${styles.openIF}`} />
        </label>
        <label
          style={{
            width: '50%',
            height: '100%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-start',
            paddingLeft: '12px',
          }}
          className={styles.btnlabelCont}
          onClick={(e) => {
            e.stopPropagation();
            let checked = false;
            if (e.target) {
              try {
                checked = e.target.querySelector('input').checked;
              } catch (err) {
                console.error(err);
              }
            }
            if (checked) {
              onMarkerChanged({ tag: 'openDF', record });
            } else {
              onMarkerChanged({ tag: 'closeDF', record });
            }
          }}
        >
          <input type="checkbox" style={{ display: 'none' }} value="df" name="toWhere" />
          <div className={`${styles.operateIcon} ${styles.openDF}`} />
        </label>
        {/* <div
          className={styles.operateIcon}
          style={{ marginRight: '24px' }}
          onClick={(e) => {
            e.stopPropagation();
            onMarkerChanged({ tag: 'openIF', record });
          }}
        >
          <img src={IFIcon1} alt="IFIcon1" />
        </div>
        <div
          className={styles.operateIcon}
          onClick={(e) => {
            e.stopPropagation();
            onMarkerChanged({ tag: 'openDF', record });
          }}
        >
          <img src={DFIcon1} alt="DFIcon1" />
        </div> */}
      </>
    ),
  });
  return cols;
};

export default { columns };
