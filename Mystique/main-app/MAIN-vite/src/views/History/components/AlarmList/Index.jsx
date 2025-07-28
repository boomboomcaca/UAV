import React, { useEffect, useState, useRef } from "react";
import { Loading, Empty, PopUp, message } from "dui";
import dayjs from "dayjs";
import { getDemos } from "./table";
import IdentifyInfo from "./components/IdentifyInfo/Index";
import Tracks from "./components/Tracks/Index";
import Handles from "./components/Handles/Index";
import RecordItem from "./RecordItem/Index.jsx";
import { getAlarmRecords } from "../../../../api/newServer";
import Modal from "../../../../components/Modal/Index";
import MapControl from "../../../../components/mapControl/Index.jsx";
import { mainConf } from "../../../../config";
import styles from "./style.module.less";
import { Player } from "video-react";

const AlarmList = (props) => {
  const { timeRange } = props;
  const [loading, setLoading] = useState(true);
  const [dataList, setDataList] = useState();
  const [showPop, setShowPop] = useState(false);
  const [detailItem, setDetailItem] = useState("");
  const mapRef = useRef();
  const [map, setMap] = useState();

  useEffect(() => {
    if (timeRange && !mainConf.mock) {
      const startTime = timeRange[0].format("YYYY-MM-DDTHH:mm:ss") + "Z"; // `${timeRange[0].utc().format("YYYY-MM-DD")} 00:00:00.000Z`;
      const stopTime = timeRange[1].format("YYYY-MM-DDTHH:mm:ss") + "Z"; // `${timeRange[1].utc().format("YYYY-MM-DD")} 23:59:59.999Z`;
      setLoading(true);
      getAlarmRecords(startTime, stopTime)
        .then((res) => {
          //
          res.json().then((json) => {
            // console.log("get records :::", json);
            if (json) {
              const showData = json.slice(0, 50);
              let index = 0;
              const dataList1 = [];
              const constructData = () => {
                const item = showData[index];
                const dt = dayjs(item.time);
                const convertItem = {
                  id: item.id,
                  rowid: index + 1,
                  time: item.time,
                  dateStr: dt.format("YYYY年MM月DD日"),
                  timeStr: dt.format("HH:mm:ss"),
                  keepTime: item.duration,
                  regions: item.invasionArea,
                  devices: item.detectionEquipments,
                  count: item.numOfFlyingObjects,
                  uavs: item.evidence
                    ? item.evidence.map((u) => {
                        const obj = { ...u };
                        obj.coordinates = [
                          [u.lastFlightLongitude, u.lastFlightLatitude],
                        ];
                        return obj;
                      })
                    : null,
                };

                if (mapRef.current) {
                  console.log(convertItem.uavs);
                  // 绘制点，然后截图
                  // if (convertItem.uavs)
                  mapRef.current.drawPlanes(convertItem.uavs || [], true);
                  setTimeout(() => {
                    convertItem.image = mapRef.current.getImage();
                  }, 500);
                }
                dataList1.push(convertItem);
                if (index < showData.length - 1) {
                  index += 1;
                  setTimeout(() => {
                    constructData();
                  }, 700);
                } else {
                  setTimeout(() => {
                    setDataList(dataList1);
                    setLoading(false);
                  }, 1000);
                }
              };
              constructData();
            } else {
            }
          });
        })
        .catch((er) => {
          console.log("get records error:::", er);
          message.error("获取数据失败");
          setDataList(undefined);
          setLoading(false);
        });
    }
  }, [map, timeRange]);

  useEffect(() => {
    if (mainConf.mock) {
      setLoading(true);
      setTimeout(() => {
        console.log("init history demos:::");
        const demos = getDemos();
        let index = 0;
        const snapShot = () => {
          const uavItem = demos[index];
          if (mapRef.current) {
            // 绘制点，然后截图
            mapRef.current.drawPlanes(uavItem.uavs || [], true);
            setTimeout(() => {
              uavItem.image = mapRef.current.getImage();
            }, 500);
          }
          if (index < demos.length - 1) {
            index += 1;
            setTimeout(() => {
              snapShot();
            }, 700);
          } else {
            setTimeout(() => {
              setDataList(demos);
              setLoading(false);
            }, 1000);
          }
        };
        snapShot();
      }, 1000);
    }
  }, []);

  return (
    <div className={styles.root}>
      <div
        className={styles.loading}
        style={{ pointerEvents: loading ? "all" : "none" }}
      >
        {!loading && !dataList && <Empty emptype={Empty.UAV} />}
        {loading && <Loading loadingMsg="数据加载中..." />}
        <div className={`${styles.mapCon} ${loading && styles.mpLoading}`}>
          <MapControl
            legend={false}
            zoom={10}
            regionBar={false}
            overview
            onLoaded={(map) => {
              mapRef.current = map;
              setTimeout(() => {
                setMap(map);
              }, 1000);
            }}
          />
        </div>
      </div>
      {dataList && (
        <div className={styles.content}>
          {dataList.map((item) => {
            return (
              <RecordItem
                recordInfo={item}
                onDetail={(e) => {
                  console.log("show detail,,,,,", e);
                  setDetailItem(item);
                  setShowPop(e);
                }}
              />
            );
          })}
        </div>
      )}
      <PopUp
        visible={showPop}
        popupTransition="rtg-zoom"
        onCancel={() => setShowPop(false)}
        mask
      >
        <div className={styles.popContainer}>
          <Modal
            title={
              showPop === "identy"
                ? "取证信息"
                : showPop === "trace"
                ? "飞行轨迹"
                : "处置记录"
            }
            onClose={() => setShowPop(false)}
          >
            {showPop === "identy" && <IdentifyInfo record={detailItem} />}
            {showPop === "trace" && <Tracks record={detailItem} />}
            {showPop === "handle" && <Handles record={detailItem} />}
          </Modal>
        </div>
      </PopUp>
    </div>
  );
};

export default AlarmList;
