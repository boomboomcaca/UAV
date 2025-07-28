import { useState, useEffect } from 'react';
import { Modal } from 'dui';
import { getState } from '@/hooks/dictKeys';
import { getEdgesList, getList, del, delList, update, updateGroupName, statistics } from '@/api/cloud';
import { stationType, stationaryCategory, mobileCategory, fmAddrType } from './dictKeys';
import { edgeUrl } from '../api/path';

const attachKeys = ['categoryStr', 'fmaddrtypeStr', 'altitudeStr', 'typeStr', 'state', 'stateStr', 'edgeID', 'edgeid'];

const defaultParams = {
  page: 1,
  rows: 1000,
  sort: 'desc',
  order: 'createTime',
};

function useStationList(dictionary, getDictValue, refreshKey) {
  const [key, setKey] = useState(0);

  const [params, setParams] = useState(defaultParams);

  const [stations, setStations] = useState([]);

  const [selectStations, setSelectStations] = useState([]);

  const [groupName, setGroupName] = useState('');
  const [showGroup, setShowGroup] = useState(false);

  useEffect(() => {
    statistics().then((res) => {
      window.console.log(res);
    });
  }, []);

  const refresh = () => {
    setKey(key + 1);
    setSelectStations([]);
  };

  const updateParams = (param) => {
    const ps = {};
    Object.keys(param).forEach((k) => {
      if (param[k] !== null) {
        ps[k] = param[k];
      }
    });
    setParams({ ...defaultParams, ...ps });
  };

  const onDelete = () => {
    if (selectStations.length > 0) {
      Modal.confirm({
        title: '站点删除',
        closable: false,
        content: '确定要删除这些站点吗？',
        onOk: () => {
          // const pros = [];
          // selectStations.forEach((s) => {
          //   pros.push(del(edgeUrl, { id: s.id }));
          // });
          // Promise.allSettled(pros).then((res) => {
          //   window.console.log(res);
          //   refresh();
          //   setSelectStations([]);
          // });
          const id = selectStations.map((ss) => {
            return ss.id;
          });
          delList(edgeUrl, { id }).then((res) => {
            window.console.log(res);
            refresh();
            setSelectStations([]);
          });
        },
      });
    }
  };

  const onGroup = () => {
    const pros = [];
    selectStations.forEach((s) => {
      // const newp = { ...s };
      // const keys = Object.keys(newp);
      // keys.forEach((k) => {
      //   // TODO 特殊处理？需更新接口
      //   if (newp[k] === null || (k === 'port' && newp[k] === '') || attachKeys.includes(k)) {
      //     delete newp[k];
      //   }
      // });
      // pros.push(update(edgeUrl, { ...newp, groupName }));

      // TODO updateGroupName
      pros.push(updateGroupName({ id: s.id, groupName }));
    });
    Promise.allSettled(pros).then((res) => {
      // if (res.result) {
      //   // TODO 分组提示
      // }
      refresh();
    });
  };

  useEffect(() => {
    if (key > 0) {
      const pros = [];
      pros.push(getEdgesList());
      pros.push(getList(edgeUrl, params));
      Promise.allSettled(pros).then((res) => {
        const [res1, res2] = res;
        const { result: egs } = res1.value;
        const { result } = res2.value;
        if (result) {
          setStations(
            result.map((r) => {
              const find = egs?.find((e) => {
                return e.id === r.id;
              });
              const item = {
                ...r,
                categoryStr: getDictValue(
                  dictionary,
                  // r.type === 'mobile' ? mobileCategory : r.type === 'stationary' ? stationaryCategory : null,
                  r.type,
                  r.category,
                ),
                fmaddrtypeStr: getDictValue(dictionary, fmAddrType, r.fmaddrtype),
                typeStr: getDictValue(dictionary, stationType, r.type),
                state: find?.state,
                stateStr: getState(find?.state).tag,
                groupName: r.groupName || '',
              };
              if (item.categoryStr === null) item.categoryStr = item.typeStr;
              return item;
            }),
          );
        }
      });
    }
  }, [key, params, refreshKey]);

  useEffect(() => {
    if (dictionary) refresh();
  }, [dictionary]);

  return {
    attachKeys,
    refresh,
    updateParams,
    stations,
    selectStations,
    setSelectStations,
    onDelete,
    groupName,
    setGroupName,
    showGroup,
    setShowGroup,
    onGroup,
  };
}

export default useStationList;
