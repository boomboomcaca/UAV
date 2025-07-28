import React, { useEffect, useState, useRef, useLayoutEffect } from "react";
import PropTypes from "prop-types";
import AllInOne from "./Index.jsx";
import StreamChartHelper from "../utils/streamChartHelper.js";
import { ChartTypes } from "../utils/enums.js";

const Stream = (props) => {
  const { onLoad, distributionBar } = props;

  /**
   * @type {[StreamChartHelper]}
   */
  const [chartHelper, setChartHelper] = useState();

  useLayoutEffect(() => {
    if (!chartHelper) {
      setChartHelper(new StreamChartHelper({ distributionBar }));
    }
  }, [chartHelper, distributionBar]);

  useEffect(() => {
    return () => {
      if (chartHelper) chartHelper.dispose();
    };
  }, [chartHelper]);

  return (
    <>
      {chartHelper && (
        <AllInOne
          {...props}
          showCursor={false}
          allowAddMarker={false}
          allowZoom={false}
          visibleCharts={[ChartTypes.spectrum]}
          chartHelper={chartHelper}
          // 创建横向bar分布统计图
          distributionBar={distributionBar}
          chartLoad={(e) => {
            console.log("chart loaded:::", e);
            onLoad(chartHelper);
          }}
        />
      )}
    </>
  );
};

Stream.defaultProps = {
  onLoad: () => {},
  onThresholdChange: () => {},
  axisX: { inside: false, component: null },
  axisY: {
    inside: false,
    autoRange: false,
    tickVisible: true,
  },
  streamTime: 10,
  showThreshold: false,
};

Stream.propTypes = {
  onLoad: PropTypes.func,
  onThresholdChange: PropTypes.func,
  axisX: PropTypes.any,
  axisY: PropTypes.any,
  showThreshold: PropTypes.bool,
  streamTime: PropTypes.number,
  distributionBar: PropTypes.bool,
};

export default Stream;
