import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
// import AlfMap, { RegionType } from "map-alf";
import MapControl from "../../../components/mapControl/Index";
import { Modal, message, Input } from "dui";
// import RegionEditor from "./components/RegionEditor.jsx";
import RegionList from "./RegionList/Index.jsx";
import { mainConf } from "../../../config";

import styles from "./style.module.less";
const WhiteRegions = (props) => {
  const mapInstanceRef = useRef();

  const [regList, setRegList] = useState([]);
  const [visibleList, setVisibleList] = useState([]);
  const [adding, setAdding] = useState(false);
  const usrRegionRef = useRef();
  const addNameRef = useRef();
  const [newItem, setNewItem] = useState();

  const getList = () => {
    const jsonStr = localStorage.getItem("white-region-list");
    if (jsonStr) {
      const localList = JSON.parse(jsonStr);
      setRegList(localList);
      setTimeout(() => {
        setVisibleList(localList.map((l) => l.name));
      }, 600);
    }
  };
  const saveList = (list) => {
    let jsonStr = "";
    if (list) {
      jsonStr = JSON.stringify(list);
    }
    localStorage.setItem("white-region-list", jsonStr);
  };
  const confirm = () => {
    Modal.confirm({
      title: "提示",
      closable: false,
      content: (
        <div>
          <Input
            placeholder="请输入名称"
            size="large"
            style={{ width: "100%" }}
            onChange={(e) => (addNameRef.current = e)}
          />
        </div>
      ),
      onOk: () => {
        if (addNameRef.current) {
          const newItem = { ...usrRegionRef.current[0] };
          newItem.name = addNameRef.current;
          setNewItem(newItem);
          setAdding(false);
        } else {
          message.info("请输入名称");
        }
      },
      onCancel: () => {
        setAdding(false);
      },
    });
  };

  useEffect(() => {
    // if (mainConf.mock) {
    //   setRegList([
    //     {
    //       geometry: {
    //         coordinates: [
    //           [104.10091615851366, 30.58617035671209],
    //           [104.08341146602248, 30.57465827485369],
    //           [104.07689010999553, 30.586751362524623],
    //           [104.10091615851366, 30.58617035671209],
    //         ],
    //         type: "Polygon",
    //       },
    //       id: "000x1",
    //       name: "000x1",
    //     },
    //     {
    //       geometry: {
    //         coordinates: [
    //           [104.06659323205741, 30.561050418202],
    //           [104.04908853956624, 30.549538336343602],
    //           [104.04256718353929, 30.561631424014536],
    //           [104.06659323205741, 30.561050418202],
    //         ],
    //         type: "Polygon",
    //       },
    //       id: "000x2",
    //       name: "000x2",
    //     },
    //   ]);
    // } else {
    getList();
    // }
  }, []);

  useEffect(() => {
    if (newItem) {
      const newList = [...regList, newItem];
      saveList(newList);
      setRegList(newList);
      setVisibleList([...visibleList, newItem.name]);
      setNewItem(undefined);
      addNameRef.current = undefined;
    }
  }, [newItem]);

  useEffect(() => {
    if (mapInstanceRef.current) {
      mapInstanceRef.current.showToolbar(
        adding
          ? {
              polygon: "绘面",
            }
          : undefined,
        adding
      );
    }
  }, [adding]);

  useEffect(() => {
    if (regList) {
      const visibles = regList.filter((reg) => visibleList.includes(reg.name));
      if (mapInstanceRef.current) {
        mapInstanceRef.current.drawWhiteRegions(
          visibles,
          visibles && visibles.length > 0
        );
      }
    }
  }, [visibleList, regList]);

  return (
    <div className={styles.protectRoot}>
      <div className={styles.mapContainer}>
        <MapControl
          regionBar
          onLoaded={(map) => {
            mapInstanceRef.current = map;
          }}
          onDrawFeature={(e) => {
            usrRegionRef.current = e;
            if (e) {
              confirm();
            }
            console.log("set ddggf:::", e);
          }}
        />
        <div className={styles.reglist}>
          <RegionList
            dataList={regList}
            visibles={visibleList}
            editing={adding}
            onShow={(e) => {
              if (visibleList.includes(e)) {
                setVisibleList(visibleList.filter((v) => v !== e));
              } else {
                setVisibleList([...visibleList, e]);
              }
            }}
            onDel={(e) => {
              const dels = regList.filter((i) => i.name !== e);
              setRegList(dels);
              // if (!mainConf.mock) {
              saveList(dels);
              // }
            }}
            onAdd={() => {
              if (mapInstanceRef.current) {
                setAdding(true);
              }
            }}
          />
        </div>
      </div>
    </div>
  );
};

export default WhiteRegions;
