import React, { useEffect, useLayoutEffect, useState, useContext } from "react";
import PropTypes from "prop-types";
import ChartContext from "../../context/chartContext.jsx";
import { ReactComponent as ClearSVG } from "../../assets/tb_clear.svg";
import { ReactComponent as CursorSVG } from "../../assets/tb_chartCursor.svg";
import { ReactComponent as MarkerSVG } from "../../assets/tb_diamond.svg";

import styles from "./style.module.less";

/**
 *{name:String,visible:Boolean,enabled:Boolean,checked:Boolean}
 * @param {{ menuItems:Array<String>, series:Array<{name:String,color:String,type:String,visible:Boolean}>, onChange:Function }} props
 * @returns
 */
const ToolBar = (props) => {
  const { menuItems, series, onChange } = props;
  //   const storageName = 'tollbar-settings';
  const ver = "0.1";
  // 按钮状态信息
  const [itemsInfo, setItemsInfo] = useState([]);
  const [visibleItems, setVisibleItems] = useState([]);
  const [itemBinding, setItemBinding] = useState({});

  const ctx = useContext(ChartContext);
  const [state = "", dispatch = null] = ctx;

  const { markers } = state;

  useEffect(() => {
    // console.log('markers change:::', markers);
  }, [markers]);

  useEffect(() => {
    console.log("menu items:::", menuItems);
    if (menuItems && menuItems.length > 0) {
      setVisibleItems(menuItems);
    } else {
      //   const items = [];
      const keys = Object.keys(toolItems);
      //   keys.forEach((key) => {
      //     items.push({ name: key, visible: true, enabled: true, checked: key === toolItems.cursor });
      //   });
      //   setItemsInfo(items);
      setVisibleItems(keys);
    }
  }, [menuItems]);

  // 图例状态
  //   const [seriesVisible, setSeriesVisible] = useState({});

  //   /**
  //    * 从本地缓存读取上一次按钮状态信息
  //    */
  //   useLayoutEffect(() => {
  //     const prev = window.localStorage.getItem(storageName);
  //     // 默认状态
  //     let settings = {
  //       cursor: true,
  //       marker: false,
  //     };
  //     if (prev) {
  //       const setInfo = JSON.parse(prev);
  //       if (setInfo.ver === ver) {
  //         settings = setInfo;
  //       }
  //     }

  //     setItemsState(settings);
  //   }, []);

  //   useEffect(() => {
  //     return () => {
  //       if (itemsState) {
  //         itemsState.ver = ver;
  //         window.localStorage.setItem(storageName, JSON.stringify(itemsState));
  //       }
  //     };
  //   }, [itemsState]);

  useEffect(() => {
    // console.log('state changed:::', state);
  }, [state]);

  return (
    <div className={styles.toolbarRoot}>
      {/* {itemsInfo.map((item, index) => {
        if (item.visible) {
          return (
            <Button
              type={item.checked ? 'primary' : 'default'}
              onClick={() => {
                const newItemsInfo = [...itemsInfo];
                newItemsInfo[index].checked = !newItemsInfo[index].checked;
                setItemsInfo(newItemsInfo);
              }}
            >
              {item.name}
            </Button>
          );
        }
        return null;
      })} */}

      {visibleItems.includes(toolItems.clear) && (
        <ClearSVG
          className={styles.clearButton}
          onClick={() => {
            onChange({
              action: toolItems.clear,
            });
          }}
        />
      )}
      {visibleItems.includes(toolItems.cursor) && (
        <CursorSVG
          className={`${styles.cursor} ${state.showCursor && styles.sel}`}
          onClick={() => {
            dispatch({
              type: "showCursor",
              value: { showCursor: !state.showCursor },
            });
          }}
        />
      )}
      {visibleItems.includes(toolItems.marker) && (
        <MarkerSVG
          className={`${styles.marker} ${state.allowAddMarker && styles.sel}`}
          onClick={() => {
            dispatch({
              type: "allowAddMarker",
              value: { allowAddMarker: !state.allowAddMarker },
            });
          }}
        />
      )}
      {visibleItems.includes(toolItems.legend) &&
        series &&
        series.map((s) => {
          <div
            className={styles.seriesItem}
            onClick={() => {
              onChange({
                action: toolItems.marker,
                value: {
                  name: s.name,
                  value: !s.visible,
                },
              });
            }}
          >
            <div>png</div>
            <span>{s.name}</span>
          </div>;
        })}
    </div>
  );
};

const toolItems = {
  clear: "clear",
  unit: "unit",
  cursor: "cursor",
  marker: "marker",
  legend: "legend",
};

ToolBar.defaultProps = {
  menuItems: ["clear", toolItems.cursor, toolItems.marker, toolItems.legend],
  series: [],
  onChange: () => {},
};

ToolBar.propTypes = {
  menuItems: PropTypes.arrayOf(PropTypes.string),
  series: PropTypes.arrayOf({
    name: PropTypes.string,
    color: PropTypes.string,
    type: PropTypes.string,
  }),
  onChange: PropTypes.func,
};

export default ToolBar;
export { toolItems };
