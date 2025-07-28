import { freq, ifbw, findParam } from '../utils/helper';
import { getTimeStamp } from '../../../lib';

function useSortLocal(setDisplayList) {
  const onOrderByKey = (key, order, dat) => {
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'asc' ? (l1[key] > l2[key] ? 1 : -1) : l2[key] > l1[key] ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const onOrderByEdgeName = (order, dat) => {
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'asc' ? (l1.edgeName > l2.edgeName ? 1 : -1) : l2.edgeName > l1.edgeName ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const onOrderBySegment0 = (order, dat) => {
    if (dat.length > 0) {
      try {
        const list = dat.sort((l1, l2) => {
          const p1 = JSON.parse(l1.params);
          const p2 = JSON.parse(l2.params);
          return order === 'asc'
            ? findParam(p1, 'scanSegments')?.value?.[0]?.startFrequency -
                findParam(p2, 'scanSegments')?.value?.[0]?.startFrequency
            : findParam(p2, 'scanSegments')?.value?.[0]?.startFrequency -
                findParam(p1, 'scanSegments')?.value?.[0]?.startFrequency;
        });
        setDisplayList([...list]);
      } catch (error) {
        window.console.log(error);
      }
    }
  };

  const onOrderByFrequency = (order, dat) => {
    if (dat.length > 0) {
      try {
        const list = dat.sort((l1, l2) => {
          const p1 = JSON.parse(l1.params);
          const p2 = JSON.parse(l2.params);
          return order === 'asc'
            ? findParam(p1, freq).value - findParam(p2, freq).value
            : findParam(p2, freq).value - findParam(p1, freq).value;
        });
        setDisplayList([...list]);
      } catch (error) {
        window.console.log(error);
      }
    }
  };

  const onOrderByDataStartTime = (order, dat) => {
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'desc'
          ? getTimeStamp(l2.dataStartTime) - getTimeStamp(l1.dataStartTime)
          : getTimeStamp(l1.dataStartTime) - getTimeStamp(l2.dataStartTime);
      });
      setDisplayList([...list]);
    }
  };

  const onOrderByBandwidth = (order, dat) => {
    if (dat.length > 0) {
      try {
        const list = dat.sort((l1, l2) => {
          const p1 = JSON.parse(l1.params);
          const p2 = JSON.parse(l2.params);
          return order === 'asc'
            ? findParam(p1, ifbw)?.value - findParam(p2, ifbw)?.value
            : findParam(p2, ifbw)?.value - findParam(p1, ifbw)?.value;
        });
        setDisplayList([...list]);
      } catch (error) {
        window.console.log(error);
      }
    }
  };

  const onOrderByTimespan = (order, dat) => {
    const getDelta = (p) => {
      return getTimeStamp(p.dataStopTime) - getTimeStamp(p.dataStartTime);
    };
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'asc' ? getDelta(l1) - getDelta(l2) : getDelta(l2) - getDelta(l1);
      });
      setDisplayList([...list]);
    }
  };

  const onOrderBySegmentInfo = (order, dat) => {
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'asc' ? (l1.segmentInfo > l2.segmentInfo ? 1 : -1) : l2.segmentInfo > l1.segmentInfo ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const onOrderByTest = (order, dat) => {
    if (dat.length > 0) {
      const list = dat.sort((l1, l2) => {
        return order === 'asc' ? (l1.test > l2.test ? 1 : -1) : l2.test > l1.test ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const sortLocalFunc = (sort, d) => {
    const { key, state } = sort;
    if (key) {
      if (key === 'segments') {
        onOrderBySegment0(state, d);
      }
      if (key === 'frequency') {
        onOrderByFrequency(state, d);
      }
      if (key === 'dataStartTime') {
        onOrderByDataStartTime(state, d);
      }
      if (key === 'bandwidth') {
        onOrderByBandwidth(state, d);
      }
      if (key === 'edgeName') {
        onOrderByEdgeName(state, d);
      }
      if (key === 'timeSpan') {
        onOrderByTimespan(state, d);
      }
      if (key === 'segmentInfo') {
        onOrderBySegmentInfo(state, d);
      }
      if (key === 'test') {
        onOrderByTest(state, d);
      }
    }
  };

  return { sortLocalFunc };
}

export default useSortLocal;
