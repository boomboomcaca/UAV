import React, { useState, useEffect } from "react";
import { Radio, Button1 } from "dui";
import Template from "../../components/Template/Index.jsx";
import { AddIcon } from "../SystemConfig/WhiteRegions/RegionList/Icons.jsx";
import CardItem from "./Item/Index.jsx";
import AddItem from "./AddItem/Index.jsx";
import { mainConf } from "../../config/index.js";
import styles from "./index.module.less";

const WhiteBlackList = (props) => {
  const [selType, setSelType] = useState("黑名单");
  const [adding, setAdding] = useState(false);
  const getMockData = () => {
    const list = [];
    for (let i = 0; i < 10; i++) {
      list.push({
        id: `list${i}`,
        index: i + 1,
        sn: `SN2753MD17EM17M${i}`,
        productor: "奥创",
        model: "Model S",
        type: i % 2,
        admin: "God",
        remark: "I am in list",
      });
    }
    return list;
  };
  const [dataList, setDataList] = useState(
    mainConf.mock ? getMockData() : undefined
  );
  return (
    <Template title="黑白名单">
      <div className={styles.headButtons}>
        <Radio
          theme="highLight"
          options={["黑名单", "白名单", "所有"]}
          value={selType}
          onChange={(e) => setSelType(e)}
        />
        <Button1
          // size="large"
          style={{ width: "64px" }}
          onClick={() => {
            // TODO add
          }}
        >
          <div className={styles.btnCon} onClick={() => setAdding(true)}>
            <AddIcon fill="transparent" size="27" />
            <span>添加</span>
          </div>
        </Button1>
      </div>
      <div className={styles.itemList}>
        {dataList?.map((item, index) => {
          if (
            selType === "所有" ||
            (item.type === 0 && selType === "黑名单") ||
            (item.type === 1 && selType === "白名单")
          ) {
            return (
              <CardItem
                recordInfo={item}
                title={item.model}
                onDelete={(e) => console.log("del item:::", e)}
              />
            );
          }
          return null;
        })}
      </div>

      <AddItem
        show={adding}
        onCancel={() => setAdding(false)}
        onOk={(e) => {
          // TODO 添加
          console.log(e);
          setAdding(false);
        }}
      />
    </Template>
  );
};

export default WhiteBlackList;
