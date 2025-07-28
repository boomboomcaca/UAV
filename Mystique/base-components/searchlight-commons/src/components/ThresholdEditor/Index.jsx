import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import { Button, message } from 'dui';
import { DrawIcon, MoveIcon, ZoomInIcon, ZoomOutIcon } from 'dc-icon';
import langT from 'dc-intl';
import { useUpdateEffect } from 'ahooks';
import AxisY from './AxisY.jsx';
import AxisX from './AxisX.jsx';
import Editor from './editor';
import NewSegmentsEditor from '../NewSegmentsEditor/NewSegmentsEditor.jsx';
import Operator from './components/Operator.jsx';
import styles from './index.module.less';
import propTypes from 'prop-types';

const ThresholdEditor = (props) => {
  const { className } = props;
  const contianerRef = useRef();
  // 缓存缩放位置信息
  const [zoomInfo, setZoomInfo] = useState();
  const {
    referenceData: baseData,
    thresholdData,
    minimumY,
    maximumY,
    axisXLabels,
    unit,
    xUnit,
    segments,
    onEnsure,
    options,
    onZoomChange,
    customAxisX,
  } = props;
  const [bindSegments, setBindSegments] = useState();
  // 频段点数
  const scanPointsRef = useRef();

  const [selSeg, setSelSeg] = useState();
  const [editor, setEditor] = useState();
  // const [editing, setEditing] = useState(true);
  // edit:编辑  drag:上下拖动 zoom:缩放
  const [operation, setOperation] = useState('edit');
  const onZoom = (start, end) => {
    if (onZoomChange) {
      onZoomChange({ zoomStart: start, zoomEnd: end });
    }
  };

  useEffect(() => {
    if (contianerRef.current && !editor) {
      const e = new Editor(contianerRef.current, options);
      setEditor(e);
      setTimeout(() => {
        e.resize();
      }, 500);
    }
  }, [options, editor, contianerRef.current]);

  useEffect(() => {
    if (editor && (baseData || thresholdData)) {
      editor.setBaseData(baseData, thresholdData);
      editor.startEdit();
      setZoomInfo({ zoomStart: 0, zoomEnd: baseData.length - 1 });
    }
  }, [editor, baseData, thresholdData]);

  useEffect(() => {
    if (segments && editor) {
      let totalPoints = 0;
      const temp = [...segments];
      for (let i = 0; i < temp.length; i += 1) {
        const seg = temp[i];
        const { startFrequency, stopFrequency, stepFrequency } = seg;
        const pointCount = Math.round(((stopFrequency - startFrequency) * 1000) / stepFrequency) + 1;
        seg.pointCount = pointCount;
        seg.startIndex = totalPoints;
        totalPoints += pointCount;
      }
      // axisXTickLabelsRef.current = axisXLabels ? [axisXLabels] : xTickLabels;
      if (!baseData && !thresholdData) {
        // 弄一个默认值
        const defaulThr = [];
        for (let i = 0; i < totalPoints; i += 1) {
          defaulThr[i] = 20;
        }
        editor.setBaseData(undefined, defaulThr);
        scanPointsRef.current = totalPoints;
        setZoomInfo({ zoomStart: 0, zoomEnd: totalPoints - 1 });
        editor.startEdit();
      }
      setBindSegments(temp);
      setSelSeg(undefined);
    } else {
      setBindSegments(undefined);
    }
  }, [segments, editor, baseData, thresholdData]);

  useUpdateEffect(() => {
    if (editor) {
      if (selSeg) {
        const { segment } = selSeg;
        setZoomInfo({ zoomStart: segment.startIndex, zoomEnd: segment.startIndex + segment.pointCount - 1 });
        editor.zoom(segment.startIndex, segment.startIndex + segment.pointCount - 1);
        onZoom(segment.startIndex, segment.startIndex + segment.pointCount - 1);
      } else {
        resetZoom();
      }
    }
  }, [editor, selSeg]);

  useEffect(() => {
    if (editor) {
      editor.setAxisYRange(minimumY, maximumY);
    }
  }, [minimumY, maximumY, editor]);

  const resetZoom = () => {
    if (scanPointsRef.current > 0 || (baseData && baseData.length > 0)) {
      const totalLen = scanPointsRef.current || baseData.length;

      if (selSeg) {
        const { segment } = selSeg;
        const { startIndex, pointCount } = segment;
        editor.zoom(startIndex, startIndex + pointCount - 1);
        onZoom(startIndex, startIndex + pointCount - 1);
        setZoomInfo({ zoomStart: startIndex, zoomEnd: startIndex + pointCount - 1 });
      } else {
        editor.zoom(0, totalLen - 1);
        onZoom(0, totalLen - 1);
        setZoomInfo({ zoomStart: 0, zoomEnd: totalLen - 1 });
        setSelSeg(undefined);
      }
    }
  };

  return (
    <div
      style={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between',
        paddingRight: '20px',
      }}
      className={className}
    >
      <div
        style={{
          display: 'flex',
          flexDirection: 'row',
          justifyContent: 'space-between',
          height: '48px',
          alignItems: 'center',
        }}
      >
        <div style={{ display: 'flex', flexDirection: 'row', marginLeft: '47px' }}>
          <div
            className={styles.toolButton}
            onClick={() => {
              setOperation('edit');
              editor.startEdit();
              editor.stopDrag();
            }}
          >
            <DrawIcon iconSize={24} color={operation !== 'edit' ? 'var(--theme-font-100)' : '#3CE5D3'} />{' '}
            <span>{langT('commons', 'draw')}</span>
          </div>
          <div
            className={styles.toolButton}
            style={{ marginLeft: '25px' }}
            onClick={() => {
              setOperation('drag');
              editor.startDrag();
              editor.stopEdit();
            }}
          >
            <MoveIcon iconSize={24} color={operation !== 'drag' ? 'var(--theme-font-100)' : '#3CE5D3'} />{' '}
            <span>{langT('commons', 'move')}</span>
          </div>

          <div
            className={styles.toolButton}
            style={{ marginLeft: '25px' }}
            onClick={() => {
              // console.log('set operation zoom');
              setOperation('zoom');
            }}
          >
            <ZoomInIcon iconSize={24} color={operation !== 'zoom' ? 'var(--theme-font-100)' : '#3CE5D3'} />
            <span>缩放</span>
          </div>
          <div
            className={styles.toolButton}
            style={{ marginLeft: '25px' }}
            onClick={() => {
              resetZoom();
            }}
          >
            <ZoomOutIcon iconSize={24} color={zoomInfo !== undefined ? '#3CE5D3' : 'var(--theme-font-100)'} />
            <span>重置</span>
          </div>
        </div>
        <Button
          size="middle"
          style={{ color: '#3CE5D3', margin: 0 }}
          onClick={() => {
            if (onEnsure) {
              onEnsure(editor.getThreshold());
            }
          }}
        >
          {langT('commons', 'confirm')}
        </Button>
      </div>
      {bindSegments && bindSegments.length > 1 && (
        <div style={{ marginLeft: '45px' }}>
          <NewSegmentsEditor
            editable={false}
            segmentList={bindSegments}
            selectSegment={selSeg}
            selectedChange={(e) => {
              // TODO 更新X轴标签
              setSelSeg(e.flag ? e : undefined);
            }}
          />
        </div>
      )}
      <div style={{ height: '100%', display: 'flex', flexDirection: 'row', justifyContent: 'space-between' }}>
        <AxisY minimumY={minimumY} maximumY={maximumY} unit={unit} />
        <div style={{ flex: 1, position: 'relative' }}>
          <div
            ref={contianerRef}
            className={styles.chartCon}
            style={{ top: 0, left: 0, width: '100%', height: '100%' }}
          />
          {operation === 'zoom' && (
            <Operator
              onZoomChange={(e) => {
                // console.log('onZoomChange', e);
                // TODO 更新X轴 仅缩放
                const { start, end, width } = e;
                if (start < end) {
                  // 放大
                  // 计算索引
                  const startPercent = start / width;
                  const stopPercent = end / width;
                  const totalLen = scanPointsRef.current || baseData.length;
                  if (totalLen > 20) {
                    const { zoomStart, zoomEnd } = zoomInfo;
                    const len = zoomEnd - zoomStart;
                    if (len > 19) {
                      let zStart = Math.round(len * startPercent) + zoomStart;
                      let zStop = Math.round(len * stopPercent) + zoomStart;
                      if (zStop + 19 >= totalLen) {
                        zStop = totalLen - 1;
                        zStart = zStop - 19;
                      }
                      setZoomInfo({ zoomStart: zStart, zoomEnd: zStop });
                      editor.zoom(zStart, zStop);
                      onZoom(zStart, zStop);
                    } else {
                      message.info('已经是最大级别了');
                    }
                  } else {
                    message.info('已经是最大级别了');
                  }
                }
                if (start > end) {
                  // 缩小
                  resetZoom();
                }
              }}
            />
          )}
        </div>
      </div>
      {!customAxisX && (
        <AxisX
          labels={axisXLabels}
          segments={bindSegments}
          selectedSegment={selSeg?.segment}
          zoomStart={zoomInfo?.zoomStart}
          zoomStop={zoomInfo?.zoomEnd}
          unit={xUnit}
        />
      )}
    </div>
  );
};

ThresholdEditor.defaultProps = {
  options: {
    lineColor: '#FF4C2B',
    thikness: 2,
    pen: '#00FF00',
    onSizeChange: () => {},
  },
  minimumY: -20,
  maximumY: 80,
  segments: undefined,
  axisXLabels: undefined,
  unit: 'dBμV',
  xUnit: 'MHz',
  thresholdData: undefined,
  // 仅在 传入了segments 的情况下此值才可设置为undefined
  referenceData: undefined,
  className: undefined,
  onZoomChange: () => {},
  customAxisX: true,
};

ThresholdEditor.propTypes = {
  referenceData: PropTypes.array,
  onEnsure: PropTypes.func.isRequired,
  thresholdData: PropTypes.array,
  options: {
    lineColor: PropTypes.string,
    thikness: PropTypes.number,
    pen: PropTypes.string,
  },
  minimumY: PropTypes.number,
  maximumY: PropTypes.number,
  segments: PropTypes.array,
  axisXLabels: propTypes.array,
  unit: PropTypes.string,
  xUnit: PropTypes.string,
  className: PropTypes.string,
  onZoomChange: PropTypes.func,
  customAxisX: PropTypes.bool,
};

export default memo(ThresholdEditor);
