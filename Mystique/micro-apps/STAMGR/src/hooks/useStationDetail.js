import { useState, useEffect } from 'react';
import { update, add } from '@/api/cloud';
import { edgeUrl } from '../api/path';

const attachKeys = ['categoryStr', 'fmaddrtypeStr', 'altitudeStr', 'typeStr', 'state', 'stateStr', 'edgeID', 'edgeid'];

function useStationDetail(type, param) {
  const [station, setStation] = useState({});

  const [newStation, setNewStation] = useState({});

  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setStation(param);
    setNewStation({ ...param });
  }, [type, param]);

  const toSaveStation = async (p) => {
    setSaving(true);
    const newp = { ...p };
    const keys = Object.keys(newp);
    keys.forEach((k) => {
      // TODO 特殊处理？需更新接口
      if (
        newp[k] === null ||
        (k === 'port' && newp[k] === '') ||
        (k === 'ip' && newp[k] === '') ||
        attachKeys.includes(k)
      ) {
        delete newp[k];
      }
    });
    let ret = null;
    if (type === 'new') {
      ret = await add(edgeUrl, newp);
    } else if (type === 'mod') {
      ret = await update(edgeUrl, newp);
    }
    setSaving(false);
    return ret;
  };

  return { station, setStation, newStation, setNewStation, toSaveStation, saving };
}

export default useStationDetail;
