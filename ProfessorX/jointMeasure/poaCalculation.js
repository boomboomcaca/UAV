const ffi = require('ffi-napi');
const ref = require('ref-napi');
const refArray = require('ref-array-napi');
const refStruct = require('ref-struct-napi');
const { getHeatmapData } = require('./heatmap');

const dllName = `${process.cwd()}/lib/algorithm/POA`;
const LocationStruct = refStruct({
  Long: ref.types.double,
  Lat: ref.types.double,
});
const poaDLL = ffi.Library(dllName, {
  POA_location: [
    ref.types.void,
    [
      ref.refType(ref.types.double),
      ref.types.int,
      ref.refType(ref.types.double),
      ref.types.double,
      ref.refType(LocationStruct),
    ],
  ],
});

async function compute(calculateDatas) {
  const longLatDatas = [];
  const levelDatas = [];
  for (let i = 0; i < calculateDatas.length; i++) {
    longLatDatas.push(...calculateDatas[i].lngLat);
    levelDatas.push(calculateDatas[i].data);
  }
  const baseCount = calculateDatas.length;
  const currentFrequency = calculateDatas[0].frequency;

  const longLatType = refArray(ref.types.double, longLatDatas.length);
  const longLatPointer = ref.alloc(longLatType, longLatDatas);

  const levelType = refArray(ref.types.double, levelDatas.length);
  const levelPointer = ref.alloc(levelType, levelDatas);

  const location = new LocationStruct();
  const locationPointer = ref.alloc(LocationStruct, location);

  await new Promise((resolve, reject) => {
    poaDLL.POA_location.async(
      longLatPointer,
      baseCount,
      levelPointer,
      currentFrequency,
      locationPointer,
      (err) => {
        if (err) {
          reject(err);
        }
        resolve();
      }
    );
  });

  if (locationPointer.deref().Long > 0 && locationPointer.deref().Lat > 0) {
    const inputLngLat = [];
    const longitude = locationPointer.deref().Long.toFixed(6);
    const latitude = locationPointer.deref().Lat.toFixed(6);
    inputLngLat.push(longitude);
    inputLngLat.push(latitude);
    const heatmapData = await getHeatmapData(inputLngLat);
    const dataCollection = [];
    dataCollection.push({
      type: 'location',
      longitude: heatmapData.optimalLng,
      latitude: heatmapData.optimalLat,
      data: heatmapData.heatmapLngLat,
    });
    const result = {
      jsonrpc: '2.0',
      id: 0,
      result: {
        taskID: '',
        timestamp: new Date().getTime() * 1000000,
        dataCollection,
      },
    };
    return result;
  }
  return null;
}

process.on('message', async (calculateDatas) => {
  try {
    const result = await compute(calculateDatas);
    if (result) {
      process.send(result);
    }
  } catch (err) {
    process.send(err.message);
  }
});
