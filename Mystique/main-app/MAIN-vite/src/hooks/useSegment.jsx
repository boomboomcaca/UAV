import { useState, useRef, useLayoutEffect } from "react";
// import { lowFormatOut } from '@/Tools/tools';

function useSegment(parameterItems) {
  const segsRef = useRef([]);
  const segparametersRef = useRef([]);
  const compIndexRef = useRef(-1);

  // 频段信息
  const [segments, setSegments] = useState([]);
  // 高亮频段idx
  const [segIndex, setSegIndex] = useState(0);
  // 全屏频段idx
  const [compIndex, setCompIndex] = useState(-1);
  // 频谱分页
  const [segpage, setsegpage] = useState(1);
  // 设备支持的频段限制
  const [segLimit, setSegLimit] = useState({
    min: 20,
    max: 8000,
    stepItems: [],
  });

  const switchPage = (num) => {
    if (num > 0) {
      if (segments.length > 4 && segpage === 1) {
        setsegpage(2);
        if (segIndex < 4) {
          setSegIndex(4);
        }
      }
    } else if (segments.length > 4 && segpage === 2) {
      setsegpage(1);
      if (segIndex > 3) {
        setSegIndex(3);
      }
    }
  };

  useLayoutEffect(() => {
    segsRef.current = segments;
  }, [segments]);

  useLayoutEffect(() => {
    compIndexRef.current = compIndex;
  }, [compIndex]);

  useLayoutEffect(() => {
    const scanSegments = parameterItems.find(
      (item) => item.name === "scanSegments"
    );
    console.log("scanSegments :::", scanSegments);
    if (scanSegments) {
      segparametersRef.current = scanSegments.template;

      const nneLimit = {};
      scanSegments.template.forEach((temp) => {
        // 频率区间
        if (temp.name === "startFrequency") {
          nneLimit.min = temp.minimum;
          nneLimit.max = temp.maximum;
        }
        // 步进信息
        if (temp.name === "stepFrequency") {
          const snap = [...temp.values];
          snap.sort((a, b) => a - b);
          nneLimit.stepItems = snap;
        }
      });
      setSegLimit(nneLimit);
      // 恶心的转格式
      // const lowSeg = lowFormatOut(scanSegments.parameters);
      setSegments(scanSegments.parameters);
    }
  }, [parameterItems]);

  return {
    segsRef,
    segparametersRef,
    compIndexRef,
    segments,
    segIndex,
    setSegIndex,
    compIndex,
    setCompIndex,
    segpage,
    setsegpage,
    segLimit,
    switchPage,
  };
}

export default useSegment;
