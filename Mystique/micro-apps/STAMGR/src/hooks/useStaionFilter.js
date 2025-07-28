import { useState, useEffect } from 'react';
import { getGroupCount } from './assitant';

/* TODO 简化xfilters
  statefilters,
  setStateFilters,
  onStateChanged,
  stateValues,
  setStateValues, */

function useStationFilter(stations) {
  const [showFilter, setShowFilter] = useState(false);

  const [zonefilters, setZoneFilters] = useState(null);
  const [categoryfilters, setCategoryFilters] = useState(null);
  const [groupfilters, setGroupFilters] = useState(null);
  const [statefilters, setStateFilters] = useState(null);

  const [zoneValues, setZoneValues] = useState([]);
  const [categoryValues, setCategoryValues] = useState([]);
  const [groupValues, setGroupValues] = useState([]);
  const [stateValues, setStateValues] = useState([]);

  const getNewValues = (tag, title, f, chk, values, func) => {
    if (chk) {
      values.push({ ...f, tag, title });
    } else {
      const find = values.find((v) => {
        return v.id === f.id;
      });
      if (find) {
        const idx = values.indexOf(find);
        values.splice(idx, 1);
      }
    }
    func([...values]);
  };

  const onCategoryChanged = (f, chk, title) => {
    getNewValues('category', title, f, chk, categoryValues, setCategoryValues);
  };
  const onZoneChanged = (f, chk, title) => {
    getNewValues('zone', title, f, chk, zoneValues, setZoneValues);
  };
  const onGroupChanged = (f, chk, title) => {
    getNewValues('groupName', title, f, chk, groupValues, setGroupValues);
  };
  const onStateChanged = (f, chk, title) => {
    getNewValues('stateStr', title, f, chk, stateValues, setStateValues);
  };

  const Reset = () => {
    setZoneValues([]);
    setCategoryValues([]);
    setGroupValues([]);
    setStateValues([]);
  };

  const Confrim = () => {
    setZoneValues(
      zoneValues.map((v) => {
        return { ...v, active: true };
      }),
    );
    setCategoryValues(
      categoryValues.map((v) => {
        return { ...v, active: true };
      }),
    );
    setGroupValues(
      groupValues.map((v) => {
        return { ...v, active: true };
      }),
    );
    setStateValues(
      stateValues.map((v) => {
        return { ...v, active: true };
      }),
    );
  };

  const activeFilter = (val, values, func) => {
    const find = values.find((v) => {
      return v.id === val.id;
    });
    if (find) {
      if (find.active === true) {
        find.active = false;
      } else {
        find.active = true;
      }
      func([...values]);
    }
  };

  const disposeFilter = (val, values, func) => {
    const find = values.find((v) => {
      return v.id === val.id;
    });
    if (find) {
      const idx = values.indexOf(find);
      values.splice(idx, 1);
      func([...values]);
    }
  };

  const onActiveFilter = (val) => {
    if (val.tag === 'zone') {
      activeFilter(val, zoneValues, setZoneValues);
    }
    if (val.tag === 'category') {
      activeFilter(val, categoryValues, setCategoryValues);
    }
    if (val.tag === 'groupName') {
      activeFilter(val, groupValues, setGroupValues);
    }
    if (val.tag === 'stateStr') {
      activeFilter(val, stateValues, setStateValues);
    }
  };

  const onDisposeFilter = (val) => {
    if (val.tag === 'zone') {
      disposeFilter(val, zoneValues, setZoneValues);
    }
    if (val.tag === 'category') {
      disposeFilter(val, categoryValues, setCategoryValues);
    }
    if (val.tag === 'groupName') {
      disposeFilter(val, groupValues, setGroupValues);
    }
    if (val.tag === 'stateStr') {
      disposeFilter(val, stateValues, setStateValues);
    }
  };

  useEffect(() => {
    if (showFilter) {
      const categoryGroup = [];
      const zoneGroup = [];
      const groupGroup = [];
      const stateGroup = [];
      stations.forEach((s) => {
        getGroupCount(categoryGroup, s, 'category', 'categoryStr');
        getGroupCount(zoneGroup, s, 'zone');
        getGroupCount(groupGroup, s, 'groupName');
        getGroupCount(stateGroup, s, 'stateStr');
      });
      setCategoryFilters(categoryGroup);
      setZoneFilters(zoneGroup);
      setGroupFilters(groupGroup);
      setStateFilters(stateGroup);
    }
  }, [showFilter, stations]);

  return {
    showFilter,
    setShowFilter,
    categoryfilters,
    setCategoryFilters,
    zonefilters,
    setZoneFilters,
    categoryValues,
    onCategoryChanged,
    zoneValues,
    onZoneChanged,
    groupfilters,
    setGroupFilters,
    onGroupChanged,
    groupValues,
    setGroupValues,
    statefilters,
    setStateFilters,
    onStateChanged,
    stateValues,
    setStateValues,
    Reset,
    Confrim,
    onActiveFilter,
    onDisposeFilter,
  };
}

export default useStationFilter;
