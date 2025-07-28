import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import { Input, Checkbox } from "dui";
// import logger from '@dc/logger';
import Cascader from "./Cascader";
import useArea from "./useArea";
import { getSystemInfo } from "../../../utils/capacitorUtils";
import styles from "./style.module.less";

const ConfigComponent = (props) => {
  const { configJson, onConfigChange } = props;
  const [checked, setChecked] = useState(true);
  const [data, setData] = useState(null);
  const { options, zone, onSelectValue } = useArea(data?.province);

  const [isWeb, setIsWeb] = useState(false);

  useEffect(() => {
    getSystemInfo(() => {
      setIsWeb(window.App.platform === "web");
    });
  }, []);

  const getData = (value) => {
    if (value) {
      const { cloud, cloud1, map, server, province, video } = JSON.parse(value);
      const obj = {
        serverIp: server.split(":")[0],
        serverPort: server.split(":")[1],
        cloudIp: cloud.split(":")[0],
        cloudPort: cloud.split(":")[1],
        cloudIp1: cloud1.split(":")[0],
        cloudPort1: cloud1.split(":")[1],
        mapIp: map.split(":")[0],
        mapPort: map.split(":")[1],
        // wsIp: ws.split(':')[0],
        // wsPort: ws.split(':')[1],
        videoIp: video ? video.split(":")[0] : "",
        videoPort: video ? video.split(":")[1] : "",
        province,
      };
      setData(obj);
      if (
        obj.serverIp === obj.cloudIp &&
        obj.serverIp === obj.mapIp &&
        obj.serverIp === obj.wsIp
      ) {
        setChecked(false);
      }
    }
  };

  useEffect(() => {
    getData(configJson);
  }, [configJson]);

  useEffect(() => {
    if (data) {
      onConfigChange(
        JSON.stringify({
          server: `${data.serverIp}:${data.serverPort}`,
          cloud: `${checked ? data.cloudIp : data.serverIp}:${data.cloudPort}`,
          cloud1: `${checked ? data.cloudIp1 : data.serverIp1}:${
            data.cloudPort1
          }`,
          // ws: `${checked ? data.wsIp : data.serverIp}:${data.wsPort}`,
          map: `${checked ? data.mapIp : data.serverIp}:${data.mapPort}`,
          province: zone ? zone[0].code : "510000",
          procinceName: zone ? zone[0].city : "四川省",
          video: `${checked ? data.videoIp : data.serverIp}:${data.videoPort}`,
        })
      );
    }
  }, [data, checked, zone]);

  return (
    <div className={styles.mainConfig}>
      <div>
        <span>server</span>
        <Input
          style={{ width: "150px" }}
          value={data?.serverIp}
          placeholder="请输入IP地址"
          onChange={(e) => {
            if (!checked) {
              setData({
                ...data,
                serverIp: e,
                cloudIp: e,
                mapIp: e,
                wsIp: e,
              });
            } else {
              setData({
                ...data,
                serverIp: e,
              });
            }
          }}
        />
        <div className={styles.spanPadding}>:</div>
        <Input
          style={{ width: "80px" }}
          value={data?.serverPort}
          placeholder="请输入端口号"
          onChange={(e) => {
            setData({
              ...data,
              serverPort: e,
            });
          }}
        />
      </div>
      <div style={{ display: "flex" }}>
        <span>city</span>
        <Cascader
          options={options}
          className={styles.cascader}
          values={zone}
          splitter=" # "
          keyMap={{ KEY: "code", LABEL: "city" }}
          onSelectValue={onSelectValue}
        />
      </div>
      <div className={styles.checkbox}>
        <Checkbox
          checked={checked}
          onChange={() => {
            setChecked(!checked);
          }}
        >
          高级
        </Checkbox>
      </div>
      {checked && (
        <>
          <div>
            <span>cloud</span>
            <Input
              style={{ width: "150px" }}
              value={data?.cloudIp}
              placeholder="请输入IP地址"
              onChange={(e) => {
                setData({
                  ...data,
                  cloudIp: e,
                });
              }}
            />
            <div className={styles.spanPadding}>:</div>
            <Input
              style={{ width: "80px" }}
              value={data?.cloudPort}
              placeholder="请输入端口号"
              onChange={(e) => {
                setData({
                  ...data,
                  cloudPort: e,
                });
              }}
            />
          </div>
          <div>
            <span>cloud1</span>
            <Input
              style={{ width: "150px" }}
              value={data?.cloudIp1}
              placeholder="请输入IP地址"
              onChange={(e) => {
                setData({
                  ...data,
                  cloudIp1: e,
                });
              }}
            />
            <div className={styles.spanPadding}>:</div>
            <Input
              style={{ width: "80px" }}
              value={data?.cloudPort1}
              placeholder="请输入端口号"
              onChange={(e) => {
                setData({
                  ...data,
                  cloudPort1: e,
                });
              }}
            />
          </div>
          {/* <div>
            <span>ws</span>
            <Input
              style={{ width: '150px' }}
              value={data?.wsIp}
              placeholder="请输入IP地址"
              onChange={(e) => {
                setData({
                  ...data,
                  wsIp: e,
                });
              }}
            />
            <div className={styles.spanPadding}>:</div>
            <Input
              style={{ width: '80px' }}
              value={data?.wsPort}
              placeholder="请输入端口号"
              onChange={(e) => {
                setData({
                  ...data,
                  wsPort: e,
                });
              }}
            />
          </div> */}
          {isWeb && (
            <div>
              <span>map</span>
              <Input
                style={{ width: "150px" }}
                value={data?.mapIp}
                placeholder="请输入IP地址"
                onChange={(e) => {
                  setData({
                    ...data,
                    mapIp: e,
                  });
                }}
              />
              <div className={styles.spanPadding}>:</div>
              <Input
                style={{ width: "80px" }}
                value={data?.mapPort}
                placeholder="请输入端口号"
                onChange={(e) => {
                  setData({
                    ...data,
                    mapPort: e,
                  });
                }}
              />
            </div>
          )}
          <div>
            <span>video</span>
            <Input
              style={{ width: "150px" }}
              value={data?.videoIp}
              placeholder="请输入IP地址"
              onChange={(e) => {
                setData({
                  ...data,
                  videoIp: e,
                });
              }}
            />
            <div className={styles.spanPadding}>:</div>
            <Input
              style={{ width: "80px" }}
              value={data?.videoPort}
              placeholder="请输入端口号"
              onChange={(e) => {
                setData({
                  ...data,
                  videoPort: e,
                });
              }}
            />
          </div>
        </>
      )}
    </div>
  );
};

ConfigComponent.defaultProps = {
  configJson: "",
  onConfigChange: () => {},
};

ConfigComponent.propTypes = {
  configJson: PropTypes.string,
  onConfigChange: PropTypes.func,
};

export default ConfigComponent;
