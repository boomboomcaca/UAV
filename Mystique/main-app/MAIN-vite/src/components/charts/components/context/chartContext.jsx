import React from "react";

const initState = {
  showCursor: true,
  allowAddMarker: true,
  // cursorPosX: -1,
  // cursorPosY: -1,
  // cursorXDataIndex: -1,
  // cursorYDataIndex: -1,cursor mouse move
  markers: [],
  // mouseInCenterMK: '',
  cursorInfo: {
    cursorPosX: -1,
    cursorPosY: -1,
    level: -9999,
    timestamp: 0,
    frequency: 0,
  },
  cursorCaption: {
    timespan: 0,
    level: 0,
    freq: 0,
  },
  minimumY: -20,
  maximumY: 80,
  unit: "dBμV",
  colorBlends: ["#FF0000", "#00FF00", "#0000FF"],
  zoomInfo: { start: 0, end: 1, startIndex: 0, endIndex: 1, zoomLen: 1 },
  // 当前正在被拖动的marker
  draggingMarker: "",
  prevData: {},
  thrPosition: 0,
};

const actions = {
  showCursor: "showCursor",
  allowAddMarker: "allowAddMarker",
  updatecursor: "updatecursor",
  // cursorCaption: 'cursorCaption',
  addMarker: "addMarker",
  removeMarker: "removeMarker",
  updateMarker: "updateMarker",
  updateMarker1: "updateMarker1",
  setAxisYRange: "setAxisYRange",
  chartZoom: "chartZoom",
  // 鼠标所在的marker
  mouseCenterMK: "mouseCenterMK",
  updateThr: "updateThr",
};

const ChartContext = React.createContext(initState);

const reducer = (state, action) => {
  // console.log(action);
  switch (action.type) {
    // case 'showCursor':
    // case 'allowAddMarker':
    // case 'cursorPosX':
    // case 'cursorPosY':
    case actions.showCursor:
    case actions.allowAddMarker:
    case actions.mouseCenterMK:
    // case actions.cursorPosX:
    // case actions.cursorPosY:
    case actions.updatecursor:
    case actions.updateThr:
    case actions.updateMarker1: {
      const newSate = {
        ...state,
        ...action.value,
      };
      //   console.log(newSate);
      return newSate;
    }
    // case 'cursorCaption':
    //   const newSate = {
    //     ...state,
    //     ...action.value,
    //   };
    //   //   console.log(newSate);
    //   return newSate;
    //   break;
    case "addMarker": {
      const newSate = {
        ...state,
      };
      //   newSate.markers = state.markers.slice(0);
      newSate.markers.push(action.value);
      return newSate;
    }

    case "removeMarker":
      break;
    case "updateMarker": {
      const newSate = {
        ...state,
      };
      const { id } = action.value;
      const mk = newSate.markers.findIndex((m) => m.id === id);
      if (mk > -1) {
        newSate.markers[mk] = { ...newSate.markers[mk], ...action.value };
      }
      return newSate;
    }
    // case 'updateMarker1': {
    //   const newSate = {
    //     ...state,
    //   };
    //   const { markers } = action.value;
    //   newSate.markers = markers;
    //   return newSate;
    // }
    case actions.setAxisYRange: {
      const newSate = {
        ...state,
      };
      const { gap, minimumY, maximumY } = action.value;
      if (gap !== undefined) {
        newSate.minimumY += gap;
        newSate.maximumY += gap;
      } else {
        newSate.minimumY = minimumY;
        newSate.maximumY = maximumY;
      }

      return newSate;
    }
    case actions.chartZoom: {
      const newSate = {
        ...state,
      };
      newSate.zoomInfo = action.value;
      return newSate;
    }
    default:
      return state;
  }
};

export default ChartContext;
export { reducer, initState, actions };
