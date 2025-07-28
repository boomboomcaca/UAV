import React, { useEffect, useState, useRef } from "react";
import MapControl from "../../../../../../components/mapControl/Index.jsx";
import PropTypes from "prop-types";
import { Checkbox, message, Radio } from "dui";
import { getTracks } from "../../../../../../api/newServer";
import { mainConf } from "../../../../../../config/index.js";
import styles from "./style.module.less";

const Tracks = (props) => {
  const { record } = props;
  const [map, setMap] = useState();
  const mapRef = useRef();
  const [uavs, setUavs] = useState();
  const [selUav, setSeluav] = useState();

  useEffect(() => {
    if (map && uavs) {
      map.drawPlanes(uavs.filter((item) => selUav.includes(item.value)));
      map.showPalneTrack(true);
    }
  }, [map, uavs, selUav]);

  useEffect(() => {
    // 查询取证信息
    if (record && map && !mainConf.mock) {
      const { uavs } = record;
      if (uavs && uavs.length > 0) {
        setUavs(
          uavs.map((item) => {
            return {
              label: item.model || "不明飞行物",
              value: item.id,
              coordinates: [
                [item.lastFlightLongitude, item.lastFlightLatitude],
              ],
            };
          })
        );
        setSeluav(uavs.map((item) => item.id));
      }
      getTracks(record.id)
        .then((rr) => {
          console.log("tracks:::::", rr);
          // rr.blob((bb) => {
          //   console.log(bb);
          // });
          rr.json()
            .then((json) => {
              console.log("track result:::", json);
              setUavs(
                uavs.map((item) => {
                  const traceData = json.filter(
                    (it) => it.uavSerialNum === item.electronicFingerprint
                  );
                  // TODO 构造轨迹信息
                  let coordinates = [
                    [item.lastFlightLongitude, item.lastFlightLatitude],
                  ];
                  if (traceData && traceData.length > 0) {
                    coordinates = traceData.map((t) => {
                      return [t.longitude, t.latitude];
                    });
                  }
                  return {
                    label: item.model || "不明飞行物",
                    value: item.id,
                    coordinates,
                  };
                })
              );
              setSeluav(uavs.map((item) => item.id));
            })
            .catch((e) => {
              console.log("get trace error1:::", e);
            });
        })
        .catch((er) => {
          console.log("get trace error:::", er);
        });
    }
  }, [record, map]);

  useEffect(() => {
    if (mainConf.mock) {
      // DEMO
      const types = ["uav", "adsb", "fighter", "unknown"];
      const datas = [];
      let referCoords = [
        [104.0765756066678, 30.7142418297975],
        [104.07743987025782, 30.71427721213942],
        [104.07818066762167, 30.71385262318286],
        [104.07937417448562, 30.713428032357655],
        [104.08011497184947, 30.71303882245823],
        [104.08089692462204, 30.712826525487984],
        [104.08253387626479, 30.712374512590202],
        [104.08319236281119, 30.712551427593368],
        [104.0845504913114, 30.712692959363125],
        [104.08607324144788, 30.71279910805336],
        [104.08883065484201, 30.712976022426915],
        [104.08957145220597, 30.713471380530066],
        [104.09105304693361, 30.71403750096114],
        [104.09208193216136, 30.714780528983283],
        [104.093110817389, 30.715841987656674],
        [104.09451010129794, 30.71679729047281],
        [104.0950451216168, 30.717717202759772],
      ];
      referCoords = referCoords.map((it) => [it[0] + 0.12, it[1] + 0.01]);
      for (let i = 0; i < 5; i += 1) {
        datas.push({
          value: `plane_${i}`,
          label: "Air2s",
          visible: true,
          type: types[i % 4],
          coordinates: referCoords.map((item) => {
            const lng = i % 2 == 1 ? item[0] + i * 0.008 : item[0] - i * 0.008;
            const lat = i % 3 == 0 ? item[1] + i * 0.008 : item[1] - i * 0.008;
            return [lng - 0.15, lat - 0.15];
          }),
          description: "none",
        });
      }
      setUavs(datas);
      setSeluav(datas.map((d) => d.value));
    }
  }, []);

  return (
    <div className={styles.trackRoot}>
      <MapControl
        regionBar
        navigator
        toolBar
        // legend={false}
        onLoaded={(map) => {
          mapRef.current = map;
          setMap(map);
          setTimeout(() => {
            map.resize();
          }, 500);
        }}
      />
      <div className={styles.checkItems}>
        {
          uavs && (
            <Checkbox.Group
              theme="highLight"
              options={uavs}
              value={selUav}
              onChange={(e) => {
                console.log("e", e);
                setSeluav(e);
              }}
            />
          )
          // uavs.map((item) => {
          //   return (
          //     <Checkbox.Traditional
          //       checked={item.visible}
          //       onChange={(bl) => {
          //         //  setShowRegion0(bl);
          //         item.visible = bl;
          //         setUavs(uavs.slice(0));
          //       }}
          //     >
          //       {item.model}
          //     </Checkbox.Traditional>
          //   );
          // })
        }
      </div>
    </div>
  );
};

export default Tracks;
