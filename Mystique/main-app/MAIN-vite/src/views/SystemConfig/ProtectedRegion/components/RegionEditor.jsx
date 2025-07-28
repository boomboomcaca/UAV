import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import { MultipleSwitch } from "dui";
import { RegionType } from "../../../../components/map/components";
import ItemEditor from "./ItemEditor.jsx";
import styles from "./style.module.less";

const RegionEditor = (props) => {
  const { onChange, onDraw, onOk, regionConfig } = props;
  const regionsItems = [
    {
      name: RegionType.region0,
      label: "保护区（点）",
      value: RegionType.region0,
    },
    {
      name: RegionType.region2,
      label: "识别处置区",
      value: RegionType.region2,
    },
    { name: RegionType.region3, label: "警戒区", value: RegionType.region3 },
    { name: RegionType.region5, label: "预警区", value: RegionType.region5 },
  ];
  const [selItem, setSelItem] = useState(RegionType.region0);

  return (
    <div className={styles.oproot}>
      <MultipleSwitch
        value={selItem}
        onChange={(iiem) => {
          // item选择项
          setSelItem(iiem.name);
          if (onChange) {
            onChange(iiem.name);
          }
        }}
        options={regionsItems}
      />
      <div>
        <ItemEditor
          type={selItem}
          config={regionConfig?.[selItem]}
          onDraw={(e) => {
            if (onDraw) onDraw(e);
          }}
          onOK={onOk}
        />
      </div>
    </div>
  );
};

RegionEditor.defaultProps = {
  onChange: () => {},
  onDraw: () => {},
  onOK: () => {},
  regionConfig: {},
};

RegionEditor.prototype = {
  onChange: PropTypes.func,
  onDraw: PropTypes.func,
  onOK: PropTypes.func,
  regionConfig: PropTypes.object,
};

export default RegionEditor;
