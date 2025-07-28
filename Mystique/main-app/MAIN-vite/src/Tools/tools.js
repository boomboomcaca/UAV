// import { defaultSettings } from "searchlight-settings";

/**
 * 生成全局唯一标识符GUId
 * @returns {string}
 */
export const createGUID = () => {
  function S4() {
    return Math.floor((1 + Math.random()) * 0x10000)
      .toString(16)
      .substring(1);
  }
  return `${S4() + S4()}-${S4()}-${S4()}-${S4()}-${S4()}${S4()}${S4()}`;
};

export const MrkToken = createGUID();
export const SelMrkToken = createGUID();
export const ChannelToken = createGUID();
export const CompassToken = createGUID();
export const SignalToken = createGUID();
export const RawToken = createGUID();
export const chartDomId = createGUID();

export const deepClone = (obj) => {
  if (!obj || typeof obj !== "object") {
    return obj;
  }
  let newObj = {};
  if (Array.isArray(obj)) {
    newObj = obj.map((item) => deepClone(item));
  } else {
    Object.keys(obj).forEach((key) => {
      // eslint-disable-next-line no-return-assign
      return (newObj[key] = deepClone(obj[key]));
    });
  }
  return newObj;
};

export const getOneSetting = (key) => {
  // const settings = window.localStorage.getItem('scan_settings');
  // const snap = settings ? JSON.parse(settings) : defaultSettings();
  // const needOne = snap?.find((item) => item.name === key);
  // return needOne || {};
  return {};
};

export const getSegKey = (segment) => {
  const { id, name, startFrequency, stopFrequency, stepFrequency } = segment;
  const key = window.btoa(
    encodeURIComponent(
      JSON.stringify({ id, name, startFrequency, stopFrequency, stepFrequency })
    )
  );
  return key;
};

export const lowFormatIn = (segs, params) => {
  const lowSnap = [];
  segs.forEach((seg) => {
    const cloneParmam = deepClone(params);
    cloneParmam.forEach((par) => {
      // eslint-disable-next-line no-param-reassign
      par.value = seg[par.name] || par.default;
    });
    cloneParmam.push(
      {
        name: "id",
        browsable: false,
        readonly: true,
        value: seg.id || createGUID(),
      },
      { name: "name", browsable: false, readonly: true, value: seg.name || "" }
    );
    lowSnap.push(cloneParmam);
  });
  return lowSnap;
};

export const lowFormatOut = (params) => {
  const currentSeg = [];
  params.forEach((item) => {
    const oneseg = {};
    item.forEach((ONE) => {
      oneseg[ONE.name] = ONE.value;
    });
    currentSeg.push(oneseg);
  });
  return currentSeg;
};
