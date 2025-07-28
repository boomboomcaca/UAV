const ffi = require('ffi-napi');
const ref = require('ref-napi');
const refArray = require('ref-array-napi');
const os = require('os');
const { getHeatmapData } = require('./heatmap');
const config = require('../data/config/config');

const osType = os.type();
const dllName = `${process.cwd()}/lib/algorithm/TDOA`;
const tdoaDLL = ffi.Library(dllName, {
  TDOA_M_fixedstation: [
    ref.types.void,
    [
      ref.refType(ref.types.double),
      ref.types.int,
      ref.refType(ref.types.int),
      ref.types.int,
      ref.types.double,
      ref.refType(ref.types.double),
      ref.refType(ref.types.int),
    ],
  ],
  GridSelect: [
    ref.types.void,
    [
      ref.refType(ref.types.double),
      ref.types.int,
      ref.refType(ref.types.double),
      ref.refType(ref.types.int),
    ],
  ],
});
const lngLatDatas = [];
let calculationCount = 0;

async function computeTDOAResult(calculateDatas) {
  const bsCollection = [];
  const iqCollection = [];
  calculateDatas.forEach((item) => {
    bsCollection.push(...item.lngLat);
    iqCollection.push(...item.iq);
  });
  const bsLength = bsCollection.length;
  const iqLength = iqCollection.length;
  const sampleRate = calculateDatas[0].fs;

  const bsType = refArray(ref.types.double, bsLength);
  const bsPointer = ref.alloc(bsType, bsCollection);

  const iqPointer = Buffer.alloc(iqLength * 4);
  for (let i = 0; i < iqLength; i++) {
    iqPointer.writeInt32LE(iqCollection[i], i * 4);
  }

  const posType = refArray(ref.types.double, 2);
  const posPointer = ref.alloc(posType, []);

  const posLengthPointer = ref.alloc(ref.types.int, 0);
  // 调用 TDOA_M_fixedstation 方法
  await new Promise((resolve, reject) => {
    tdoaDLL.TDOA_M_fixedstation.async(
      bsPointer,
      bsLength,
      iqPointer,
      iqLength,
      sampleRate,
      posPointer,
      posLengthPointer,
      (err) => {
        if (err) {
          reject(err);
        }
        resolve();
      }
    );
  });
  if (
    posLengthPointer.deref() === 2 &&
    posPointer.deref()[0] > 0 &&
    posPointer.deref()[1] > 0
  ) {
    const resultLngLat = [];
    resultLngLat.push(posPointer.deref()[0]);
    resultLngLat.push(posPointer.deref()[1]);
    if (lngLatDatas.length > 10000) {
      lngLatDatas.shift();
    }
    lngLatDatas.push(resultLngLat);
    const lngLatCount = lngLatDatas.length * 2;
    const lngLatInput = [];
    const lngLatOutput = [];
    lngLatDatas.forEach((item) => lngLatInput.push(...item));

    const inputType = refArray(ref.types.double, lngLatInput.length);
    const inputPointer = ref.alloc(inputType, lngLatInput);
    const outputType = refArray(ref.types.double, lngLatInput.length);
    const outputPointer = ref.alloc(outputType, lngLatOutput);
    const outputLengthPointer = ref.alloc(ref.types.int, 0);
    // 调用 GridSelect 方法
    await new Promise((resolve, reject) => {
      tdoaDLL.GridSelect.async(
        inputPointer,
        lngLatCount,
        outputPointer,
        outputLengthPointer,
        (err) => {
          if (err) {
            reject(err);
          }
          resolve();
        }
      );
    });
    if (outputLengthPointer.deref() > 0) {
      const inputLngLat = [];
      for (let i = 0; i < outputLengthPointer.deref(); i += 2) {
        const longitude = outputPointer.deref()[i].toFixed(6);
        const latitude = outputPointer.deref()[i + 1].toFixed(6);
        let exist = false;
        for (let j = 0; j < inputLngLat.length; j += 2) {
          if (inputLngLat[j] === longitude && inputLngLat[j + 1] === latitude) {
            exist = true;
            break;
          }
        }
        if (!exist) {
          inputLngLat.push(longitude);
          inputLngLat.push(latitude);
        }
      }
      const heatmapData = await getHeatmapData(inputLngLat);
      const dataCollection = [];
      dataCollection.push({
        type: 'location',
        longitude: heatmapData.optimalLng,
        latitude: heatmapData.optimalLat,
        data: heatmapData.heatmapLngLat,
      });
      const tdoaResult = {
        jsonrpc: '2.0',
        id: 0,
        result: {
          taskID: '',
          timestamp: new Date().getTime() * 1000000,
          dataCollection,
        },
      };
      return tdoaResult;
    }
  }
  return null;
}

process.on('message', async (calculateDatas) => {
  try {
    const tdoaResult = await computeTDOAResult(calculateDatas);
    if (tdoaResult) {
      process.send(tdoaResult);
    }
    if (!osType.includes('Windows')) {
      calculationCount++;
      try {
        if (calculationCount === config.manualGCInterval) {
          global.gc();
          calculationCount = 0;
        }
        // eslint-disable-next-line no-empty
      } catch {}
    }
  } catch (err) {
    process.send(err.message);
  }
});
