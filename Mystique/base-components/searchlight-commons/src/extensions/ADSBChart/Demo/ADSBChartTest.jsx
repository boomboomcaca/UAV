import React, { useState, useRef, useEffect } from 'react';
import ADSBChart from '../ADSBChart.jsx';
import style from './style.module.less';
function ADSBChartTest() {
  const [axisValues] = useState([
    '00:00',
    '00:30',
    '01:00',
    '01:30',
    '02:00',
    '02:30',
    '03:00',
    '03:30',
    '04:00',
    '04:30',
    '05:00',
    '05:30',
    '06:00',
    '06:30',
    '07:00',
    '07:30',
    '08:00',
    '08:30',
    '09:00',
    '09:30',
    '10:00',
    '10:30',
    '11:00',
    '11:30',
    '12:00',
    '12:30',
    '13:00',
    '13:30',
    '14:00',
    '14:30',
    '15:00',
    '15:30',
    '16:00',
    '16:30',
    '17:00',
    '17:30',
    '18:00',
    '18:30',
    '19:00',
    '19:30',
    '20:00',
    '20:30',
    '21:00',
    '21:30',
    '22:00',
    '22:30',
    '23:00',
    '23:30',
  ]);

  const [axisValues1, setAxisValues1] = useState([]);
  const [markData1, setMarkData1] = useState(['17:00']);

  const [selectedAudio, setSelectedAudio] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      setAxisValues1([
        '00:00',
        '01:00',
        '02:00',
        '03:00',
        '04:00',
        '05:00',
        '06:00',
        '07:00',
        '08:00',
        '09:00',
        '10:00',
        '11:00',
        '12:00',
        '13:00',
        '14:00',
        '15:00',
        '16:00',
        '17:00',
        '18:00',
        '19:00',
        '20:00',
        '21:00',
        '22:00',
        '23:00',
      ]);

      // setMarkData1(undefined);
    }, 1000);
    setTimeout(() => {
      setMarkData1(undefined);
    }, 10000);
  }, []);

  const seriesData = [
    3, 2, 2, -26, -27, 30, 55, 50, 40, 30, 30, 30, 40, 50, 60, 75, 80, 70, 60, 40, 30, 20, 20, 26, 27, 30, 55, 50, 40,
    30, 30, 30, 40, 50, 60, 75, 80, 70, 60, 40, -30, -20, 20, 26, 27, 30, 55, 50,
  ];
  const [seriesData1] = useState([
    10, 30, 50, 26, 37, 40, 55, 50, 60, 50, 20, 30, 40, 30, 50, 65, 40, 50, 60, 10, 10, 30, 50, 26, 37, 40, 55, 50, 60,
    50, 20, 30, 40, 30, 50, 65, 40, 50, 60, 10, 26, 37, 40, 55, 50, 60, 50, 20,
  ]);

  const [seriesDataTest, setDataTest] = useState([
    {
      name: 'test',
      data: seriesData,
    },
  ]);

  useEffect(() => {
    setTimeout(() => {
      setDataTest(undefined);
    }, 5000);
  }, []);

  const seriesData2 = [
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    null,
    2.5,
    null,
    null,
    null,
    null,
    null,
    null,
  ];

  const adsbRef = useRef();

  return (
    <div className={style.App}>
      <button
        onClick={() => {
          console.log(adsbRef.current);
          if (adsbRef.current) {
            adsbRef.current.resize();
          }
        }}
      >
        Resize
      </button>
      <div className={style.adsbchartDemo}>
        <div className={style.adsb}>
          <ADSBChart
            ref={adsbRef}
            title="相似度比较"
            // X 轴 标签  时间刻度数组
            axisXValues={axisValues}
            // 显示选择背景
            selectArea
            // 图列
            legend
            // 选择变更事件
            onSelectChange={(e) => {
              console.log('on select chaged 1 === ', e);
            }}
            // 图表数据
            seriesDatas={[
              {
                name: 'test',
                // 可选
                color: '#00FF00',
                data: seriesData,
              },
              {
                name: 'test1',
                data: seriesData1,
              },
            ]}
          />
        </div>
        <div className={style.adsb}>
          <ADSBChart
            title="相似度比较"
            // X 轴 标签  时间刻度数组
            axisXValues={axisValues}
            // 图表数据
            seriesDatas={[
              {
                name: 'test',
                data: seriesData,
              },
              {
                name: 'test1',
                data: seriesData1,
              },
            ]}
          />
        </div>
        <div className={style.adsb}>
          <ADSBChart
            title="频率 101.7 MHz"
            // X 轴 标签  时间刻度数组
            axisXValues={axisValues1}
            // 设置图表类型 默认 line   有效值  line、area、bar
            type="area"
            // 显示选择背景
            selectArea
            // 选择变更事件
            onSelectChange={(e) => {
              console.log('on select chaged 3 ===', e);
            }}
            // 图表数据
            seriesDatas={seriesDataTest}
            markData={markData1}
            // markData={['06:30', '07:30', '08:30', '16:30', '17:30', '21:30', '22:30', '23:30']}
          />
        </div>
        <div className={style.adsb}>
          <ADSBChart
            title="2022年7月1日6:00-7:00"
            // X 轴 标签  时间刻度数组
            axisXValues={axisValues}
            // 设置图表类型 默认 line   有效值  line、area、bar
            type="bar"
            // 选择变更事件
            onSelectChange={(e) => {
              console.log('on select chaged 4 ===', e);
            }}
            // 音频播放事件
            onPlayAudio={(e) => {
              console.log('on play audio 1 ===', e);
              setSelectedAudio(e);
            }}
            // selectArea
            // 图表数据
            seriesDatas={[
              {
                name: 'test',
                data: seriesData,
              },
            ]}
            markData={['06:30', '07:30', '08:30', '16:30', '17:30', '21:30', '22:30', '23:30']}
          />
        </div>
      </div>
      {selectedAudio && <div>{JSON.stringify(selectedAudio)}</div>}
    </div>
  );
}

export default ADSBChartTest;
