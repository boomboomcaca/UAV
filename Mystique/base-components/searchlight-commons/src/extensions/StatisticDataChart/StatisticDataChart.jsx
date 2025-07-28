import React, { useEffect, useState, useRef, useImperativeHandle } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import { Line } from '@dc/charts';
import { combineData, createFormateTime } from './util';
import { solidData } from '../../lib/tools';
import styles from './StatisticDataChart.module.less';
/**
 * 单频测向 电平统计图
 */
const StatisticDataChart = React.forwardRef((props, ref) => {
  const { className, point } = props;
  const [pointData, setPointData] = useState([]);
  const pointDataRef = useRef(pointData);
  const lineDataRef = useRef([]);
  const chartDataRef = useRef([]);
  const preTimeRef = useRef(0);
  const publish = useRef(null);

  useEffect(() => {
    return () => {};
  }, []);
  // useMemo(() => {
  //   if (
  //     point &&
  //     point.time &&
  //     Object.prototype.hasOwnProperty.call(point, 'level') &&
  //     Math.abs(point.time - preTimeRef.current) > 1000
  //   ) {
  //     preTimeRef.current = point.time;
  //     chartDataRef.current.push(point.level);
  //     pointDataRef.current.push(point);
  //     if (chartDataRef.current.length > 50000) {
  //       chartDataRef.current.shift();
  //     }
  //     if (pointDataRef.current.length > 400) {
  //       pointDataRef.current.shift();
  //     }
  //     const drawData = combineData(chartDataRef.current, 401);
  //     lineDataRef.current = drawData;
  //     setPointData(combineData(pointDataRef.current, 401));
  //     // 频谱数据
  //     publish.current({
  //       pstype: 'spectrum',
  //       series: [
  //         {
  //           name: 'main',
  //           data: executeSolid(drawData.slice(0, 400)),
  //           main: true,
  //           color: '#FA8C16',
  //         },
  //       ],
  //     });
  //   }
  // }, [point]);
  // 自定义暴露给父组件的方法或者变量
  useImperativeHandle(ref, () => ({
    onDraw: (currentPoint) => {
      if (
        currentPoint &&
        currentPoint.time &&
        Object.prototype.hasOwnProperty.call(currentPoint, 'level') &&
        Math.abs(currentPoint.time - preTimeRef.current) > 1000
      ) {
        preTimeRef.current = currentPoint.time;
        chartDataRef.current.push(currentPoint.level);
        pointDataRef.current.push(currentPoint);
        if (chartDataRef.current.length > 50000) {
          chartDataRef.current.shift();
        }
        if (pointDataRef.current.length > 400) {
          pointDataRef.current.shift();
        }
        const drawData = combineData(chartDataRef.current, 401);
        lineDataRef.current = drawData;
        setPointData(combineData(pointDataRef.current, 401));
        // 频谱数据
        publish.current({
          pstype: 'spectrum',
          series: [
            {
              name: 'main',
              data: solidData(drawData.slice(0, 400)),
              main: true,
              color: '#FA8C16',
            },
          ],
        });
      }
    },
    reset: () => {
      // 重置
      publish.current({ pstype: 'reset' });
    },
  }));
  return (
    <div className={classnames(styles.container, className)}>
      <Line
        axisY={{
          show: false,
        }}
        publish={(p) => {
          publish.current = p;
        }}
      />
      <div className={styles.mask}>
        <div className={styles.mask_title}>
          {/* 统计 */}
          {langT('commons', 'StatisticDataChartTitle1')}
        </div>
        <div className={styles.mask_title}>
          <span>{pointData.length > 1 ? createFormateTime(pointData[0].time) : ''}</span>
          <span>{pointData.length > 1 ? createFormateTime(pointData[pointData.length - 1].time) : ''}</span>
        </div>
      </div>
    </div>
  );
});
StatisticDataChart.defaultProps = {
  className: null,
  point: {},
};

StatisticDataChart.propTypes = {
  className: PropTypes.any,
  point: PropTypes.object,
};
export default StatisticDataChart;
