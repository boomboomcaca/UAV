import React, { useEffect, useRef, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import initDcMap from 'dc-map';
// import { StationIcons } from 'searchlight-commons';
import PubSub from 'pubsub-js';
import StationIcons from '../../../components/StationIcons';
import { pubSubKey, clickMapPubSubKey } from '../setting';
// import flightOff from '../../../assets/images/flightOff.png';
// import flightOn from '../../../assets/images/flightOn.png';
import 'dc-map/dist/main.css';
import styles from './index.module.less';

const NodesName = 'mrscan_nodes';
const Map = React.forwardRef((props, ref1) => {
  const { stations, selectEdges, mapOptions } = props;

  const options = {
    container: 'MRSCAN-map',
    zoom: 12,
    coordinate: 'WGS84',
    mapType: mapOptions?.mapType || 'amap',
    customUrl: mapOptions?.webMapUrl || mapOptions?.customUrl || 'http://192.168.102.31:6088',
    fontUrl: mapOptions?.fontUrl || 'http://192.168.102.103:6066/public',
    type: 'grid',
    showBaseTool: false,
    showMeasureTool: false,
    showScaleControl: false,
    baseToolPosition: 'top-right',
    measureToolPosition: 'top-right',
  };
  const mapInstance = (window.m = useRef(initDcMap(options)).current);

  useLayoutEffect(() => {
    mapInstance.mount();
    mapInstance.handleOAStationsClick(NodesName, (e) => {
      PubSub.publish(clickMapPubSubKey, {
        selEdgeIds: e.id,
      });
    });
  }, []);
  //  接受选中的站点，并绘制map
  useEffect(() => {
    if (mapInstance && stations.length > 0 && selectEdges) {
      const arr = [...stations];
      const selEdgeIds = selectEdges.map((e) => {
        return e.id;
      });
      arr.forEach((e) => {
        if (selEdgeIds.includes(e.id)) {
          e.selected = 'Y';
        } else {
          e.selected = 'N';
        }
      });
      setView(arr);
      PubSub.subscribe(pubSubKey, (evt, dat) => {
        arr.forEach((e) => {
          if (dat.selEdgeIds.includes(e.id)) {
            e.selected = 'Y';
          } else {
            e.selected = 'N';
          }
        });
        setView(arr, false);
      });
    }
  }, [mapInstance, stations, selectEdges]);

  useEffect(() => {
    return () => {
      PubSub.unsubscribe(pubSubKey);
      PubSub.unsubscribe(clickMapPubSubKey);
    };
  }, []);

  const setView = (arr, freshPosition = true) => {
    const json = [];
    if (arr && arr.length > 0) {
      const t = [...arr];
      const newlist = t.filter((item) => {
        return item.latitude;
      });
      newlist.forEach((it) => {
        json.push({
          width: it.selected === 'Y' ? 66 : 58,
          height: it.selected === 'Y' ? 66 : 58,
          outLineColor: it.selected === 'Y' ? '#FF4C2B' : '',
          longitude: it.longitude,
          latitude: it.latitude,
          // status: it.moduleState,
          category: Number(it.category),
          id: it.id,
          edgeID: it.id,
          length: 5,
          name: it.name,
          selected: it.selected,
          // prop: it,
          innerImage: StationIcons(it.type, 'idle'),
          lineLength: 0,
          zIndex: it.selected === 'Y' ? 1 : 0,
          stationWidth: it.selected === 'Y' ? 22 : 17,
          stationHeight: it.selected === 'Y' ? 22 : 17,
        });
      });

      if (mapInstance && json.length > 0 && json instanceof Array) {
        setTimeout(() => {
          try {
            mapInstance?.removePopup('nodes_popup');
            mapInstance?.drawOAStations(json, NodesName);
            freshPosition && getPosition(json);
            json.forEach((e) => {
              if (e.selected === 'Y') {
                mapInstance.drawPopup(
                  { id: e.id, latitude: e.latitude, longitude: e.longitude },
                  { popupText: e.name, offset: 33 },
                  'nodes_popup',
                );
              }
            });
          } catch (error) {
            console.log(error);
          }
        }, 300);
      } else {
        mapInstance?.drawOAStations(json, NodesName);
      }
    }
  };

  const getPosition = (edges) => {
    let minLat = 90;
    let maxLat = -90;
    let minLng = 180;
    let maxLng = -180;
    edges.forEach((e) => {
      if (e.latitude < minLat) minLat = e.latitude;
      if (e.latitude > maxLat) maxLat = e.latitude;
      if (e.longitude < minLng) minLng = e.longitude;
      if (e.longitude > maxLng) maxLng = e.longitude;
    });

    let lat = 0;
    let lng = 0;

    if (maxLat > 0 && minLat < 0) {
      lat = (maxLat - Math.abs(minLat)) / 2;
    } else {
      lat = (minLat + maxLat) / 2;
    }

    if (maxLng > 0 && minLng < 0) {
      lng = (maxLng - Math.abs(minLng)) / 2;
    } else {
      lng = (minLng + maxLng) / 2;
    }
    const list = [];
    edges.forEach((it) => {
      list.push([it.longitude * 1, it.latitude * 1]);
    });
    mapInstance.setPosition([lng, lat], true);
    if (list.length > 0) mapInstance.fitBounds(list, 30);
  };
  return <div id="MRSCAN-map" className={styles.mapBox} />;
});

Map.defaultProps = {
  stations: [],
  selectEdges: [],
  mapOptions: {},
};

Map.propTypes = {
  stations: PropTypes.array,
  selectEdges: PropTypes.array,
  mapOptions: PropTypes.object,
};

export default Map;
