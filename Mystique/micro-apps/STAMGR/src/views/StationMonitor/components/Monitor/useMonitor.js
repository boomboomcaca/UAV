import { useState, useEffect, useRef } from 'react';
import notifilter from 'notifilter';
import getConfig from '@/config';
import useDictionary from '@/hooks/useDictionary';
import useDeviceListLite from '@/hooks/useDeviceListLite';
import { testMain, testPower, testSensor, testAlarm, testCamera } from './data';

const { wsNotiUrl } = getConfig();

function useMonitor(edgeId) {
  const [mainData, setMainData] = useState({});
  const [powerData, setPowerData] = useState([]);
  const [sensorData, setSensorData] = useState([]);
  const [alarmData, setAlarmData] = useState([]);
  const [cameraData, setCameraData] = useState([]);
  // const [mainData, setMainData] = useState(testMain);
  // const [powerData, setPowerData] = useState(testPower);
  // const [sensorData, setSensorData] = useState(testSensor);
  // const [alarmData, setAlarmData] = useState(testAlarm);
  // const [cameraData, setCameraData] = useState(testCamera);

  const { devices: videoSurveillances } = useDeviceListLite(edgeId, 'control', false, {
    model: 'VideoEnvironment',
    reverse: false,
  });

  const { devices: envirSurveillances } = useDeviceListLite(edgeId, 'control', false, {
    model: 'VideoEnvironment',
    reverse: true,
  });

  useEffect(() => {
    // setMainData(testMain);
    // setPowerData(testPower);
    setSensorData(testSensor);
    setAlarmData(testAlarm);
    // setCameraData(testCamera);
  }, []);

  useEffect(() => {
    if (videoSurveillances && videoSurveillances.length > 0) {
      const videoes = [];
      videoSurveillances.forEach((vs) => {
        window.console.log(vs);
        const uri =
          vs.parameters.find((vsp) => {
            return vsp.name === 'host';
          })?.value || '';
        videoes.push({ name: vs.displayName, url: uri, id: vs.id });
      });
      setCameraData(videoes);
    }
  }, [videoSurveillances]);

  useEffect(() => {
    if (envirSurveillances && envirSurveillances.length > 0) {
      const switches = [];
      envirSurveillances.forEach((es) => {
        const ons = es.parameters.filter((esp) => {
          const reg = /Switch\d*$/;
          return esp.name.match(reg) !== null;
        });
        window.console.log(ons);
        ons.forEach((on) => {
          const enable = es.parameters.find((esp) => {
            return esp.name === `${on.name}Enabled`;
          });
          const display = es.parameters.find((esp) => {
            return esp.name === `${on.name}Name`;
          });
          if ((enable && enable.value) || !enable) {
            switches.push({
              type: 'switchState',
              moduleID: es.id,
              name: on.name,
              display: display?.value || on.displayName,
              state: on.value,
              info: [],
            });
          }
        });
      });
      setPowerData(switches);
    }
  }, [envirSurveillances]);

  const { dictionary } = useDictionary(['securityAlarm']);

  const allAlarmRef = useRef(null);

  useEffect(() => {
    if (dictionary) {
      const allAlarm = dictionary[0]?.data?.map((d) => {
        return {
          name: d.key,
          display: d.value,
          unit: '',
          value: false,
          message: '',
        };
      });
      allAlarmRef.current = allAlarm;
    }
  }, [dictionary]);

  useEffect(() => {
    let unregister = null;
    if (edgeId) {
      unregister = notifilter.register({
        url: wsNotiUrl,
        onmessage: (res) => {
          const { result } = res;
          // const now = new Date().getTime();
          const time = parseInt(result.timestamp / 1000000, 10);
          result.dataCollection?.forEach((dc) => {
            if (dc.edgeId === edgeId) {
              if (dc.type === 'switchState') {
                if (dc.name === 'main') {
                  const mata = { ...dc };
                  setMainData(mata);
                } else {
                  const cpd = powerData.find((ap) => {
                    return ap.name === dc.name;
                  });
                  if (cpd) {
                    cpd.info = dc.info;
                    cpd.state = dc.state;
                    cpd.switchType = dc.switchType;
                  } else {
                    powerData.push(dc);
                  }
                  setPowerData([...powerData]);
                }
              } else if (dc.type === 'environment') {
                const sdata = [...sensorData];
                dc.info.forEach((dinfo) => {
                  const cpd = sdata.find((ap) => {
                    return ap.name === dinfo.name;
                  });
                  if (cpd) {
                    sdata[sdata.indexOf(cpd)] = { ...dinfo };
                  } else {
                    sdata.push(dinfo);
                  }
                });
                setSensorData(sdata);
              } else if (dc.type === 'securityAlarm') {
                const allAlarm = [...allAlarmRef.current];
                dc.info.forEach((dinfo) => {
                  const find = allAlarm.find((ap) => {
                    return ap.name === dinfo.name;
                  });
                  const tinfo = { ...dinfo, time };
                  if (find) {
                    allAlarm.splice(allAlarm.indexOf(find), 1);
                  }
                  allAlarm.unshift(tinfo);
                  // for (let i = 0; i < alarmData.length; i += 1) {
                  //   if (now - alarmData[i].time > 20000) {
                  //     alarmData.splice(i, 1);
                  //   }
                  // }
                });
                setAlarmData([...allAlarm]);
              }
            }
          });
        },
        edgeId: [
          /* edgeID */
        ],
        dataType: ['securityAlarm', 'environment', 'switchState'],
      });
    }
    return () => {
      unregister?.();
    };
  }, [edgeId]);

  return { mainData, powerData, sensorData, alarmData, cameraData, videoSurveillances };
}

export default useMonitor;
