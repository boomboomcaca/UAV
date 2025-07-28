/* eslint-disable no-loop-func */
import { useState, useEffect } from 'react';
import { getList } from '@/api/cloud';
import { areacodeUrl } from '../api/path';

const special = ['110000', '120000', '310000', '500000', '710000', '810000', '820000'];

function useArea(areacode) {
  const [options, setOptions] = useState(null);

  const [zone, setZone] = useState(null);

  const getAreas = async (params = { rows: 2000, level: 0 }) => {
    const ret = await getList(areacodeUrl, params);
    window.console.log(ret);
    if (params.level > 0) {
      if (options && options.length > 0) {
        let find = { children: options };
        let hire = 0;
        while (hire < params.level && find && find.children) {
          find = find.children.find((opt) => {
            return opt.code.indexOf(params['code.hlk'].substring(0, (hire + 1) * 2)) === 0;
          });
          hire += 1;
        }
        if (find) {
          find.children = ret.result;
        }
        setOptions([...options]);
      }
    } else {
      setOptions(ret.result);
    }
  };

  const getSomeAreas = async (code) => {
    const gets = [];
    const isSpecial = special.includes(`${code.substring(0, 2)}0000`);
    gets.push(getList(areacodeUrl, { rows: 2000, level: 0 }));
    if (!isSpecial) {
      gets.push(getList(areacodeUrl, { rows: 2000, level: 1, 'code.hlk': code.substring(0, 1 * 2) }));
    }
    gets.push(getList(areacodeUrl, { rows: 2000, level: 2, 'code.hlk': code.substring(0, 2 * (isSpecial ? 1 : 2)) }));
    Promise.allSettled(gets).then((res) => {
      let opts = null;
      let node = null;
      const sels = [];
      // TODO find out error of blank
      window.console.log(res);
      res.forEach((r, i) => {
        const { value } = r;
        if (value && value.result.length > 0) {
          let leaves = null;
          if (i === 0) {
            opts = value.result;
            node = opts;
            leaves = node;
          } else {
            node.children = value.result;
            leaves = node.children;
          }
          node = leaves.find((o) => {
            return o.code.substring(0, 2 * (i + 1)) === code.substring(0, 2 * (i + 1));
          });
          if (node) {
            sels.push(node);
          }
        }
      });
      setOptions(opts);
      setZone(sels);
    });
  };

  const onSelectValue = (val) => {
    setZone(val);
    if (val && val.length > 0) {
      const valx = val[val.length - 1];
      const isSpecial = special.includes(valx.code);
      const lvl = valx.level + 1;
      if (!valx.children) {
        getAreas({ rows: 2000, level: isSpecial ? 2 : lvl, 'code.hlk': valx.code.substring(0, lvl * 2) });
      }
    }
  };

  useEffect(() => {
    if (areacode) {
      getSomeAreas(areacode);
    } else {
      getAreas();
    }
  }, [areacode]);

  return { options, zone, onSelectValue };
}

export default useArea;
