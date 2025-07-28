const ffi = require('ffi-napi');
const ref = require('ref-napi');
const Struct = require('ref-struct-napi');
const RefArray = require('ref-array-napi');
const config = require('../data/config/config');
const { getEdge } = require('../manager/edgeManager');
const { getLogger } = require('../helper/log4jsHelper');
const jointMeasure = require('./jointMeasure');
const { getHeatmapData } = require('./heatmap');

const logger = getLogger('intersectionPositioning');
const dllName = `${process.cwd()}/lib/algorithm/IntersectionPositioning`;

// 暂时用不上
// var AsyncLock = require('async-lock');
// var lock = new AsyncLock();
const Param = Struct({
  pointLen: ref.types.int,
  azimuthNLength: ref.types.int,
  mindistance: ref.types.double,
  angleindexLength: ref.types.int,
  lonLength: ref.types.int,
  latLength: ref.types.int,
  angleindex: RefArray(ref.types.int, 1000),
  azimuthN: RefArray(ref.types.float, 1000),
  lat: RefArray(ref.types.double, 1000),
  lon: RefArray(ref.types.double, 1000),
  result: RefArray(ref.types.double, 100),
  resultLength: ref.types.double,
});

// `ffi.Library`用于注册函数，第一个入参为 DLL 路径，最好为文件绝对路径
const dll = ffi.Library(dllName, {
  intersectionPositioning: [ref.types.void, [ref.refType(Param)]],
});

class intersectionPositioning extends jointMeasure {
  lock = false;

  lockArrayOperator = false;

  lon = [];

  lat = [];

  azimuthN = [];

  angleindex = [];

  // 处理数据
  // 所有固定字符串写入配置文件
  async processData(data) {
    try {
      this.sendDataToClient(data);
      // 乐观锁
      if (this.lock) {
        return;
      }
      const { edgeID } = data.result;
      const edgeInfo = await getEdge(edgeID);
      this.lon.push(edgeInfo.longitude);
      this.lat.push(edgeInfo.latitude);
      // todo:angleIndex 测试1也可以工作，具体待优化
      this.angleindex.push(1);
      const item = data.result.dataCollection.find((s) => s.type === 'dfind');
      if (item && item.azimuth) {
        this.azimuthN.push(item.azimuth);
      }
      // 超过最大单站定位点 移除前100
      if (
        this.lon.length >
        config.intersectionPositioning.signleLocateDataMaxCount
      ) {
        this.lon.splice(0, 100);
        this.lat.splice(0, 100);
        this.angleindex.splice(0, 100);
        this.azimuthN.splice(0, 100);
      }
      // 乐观锁防止再次触发方法
      if (this.lockArrayOperator) {
        return;
      }
      if (this.angleindex.length > config.intersectionPositioning.pointLen) {
        // 如果数据长度大于10，则开始进行准备数据,同时该方式
        // 并发高了乐观锁锁不住
        this.lock = true;
        this.lockArrayOperator = true;
        const self = this;
        if (config.intersectionPositioning.useTestData) {
          // eslint-disable-next-line global-require
          const jsonData = require('../lib/algorithm/IntersectionPositioningData.json');
          const testData = jsonData[3];
          this.lon = testData.lon.slice();
          this.lat = testData.lat.slice();
          this.azimuthN = testData.azimuthN.slice();
          // 测试全是1也能计算出结果  205,304 ,203,298 //经过测试angleIndex 影响因素不大
          this.angleindex = testData.angleindex.slice();
        }
        const tempLon = this.lon.slice();
        const tempLat = this.lat.slice();
        const tempAzimuthN = this.azimuthN.slice();
        const tempAngleindex = this.angleindex.slice();
        this.lon = [];
        this.lat = [];
        this.azimuthN = [];
        this.angleindex = [];
        await self.calcIntersectionPositioning(
          tempLon,
          tempLat,
          tempAzimuthN,
          tempAngleindex
        );
        this.lock = false;
        setTimeout(() => {
          self.lockArrayOperator = false;
        }, config.intersectionPositioning.signleLocateDrawtimeInterval);
      }
    } catch (error) {
      logger.error(error);
    }
  }

  async calcIntersectionPositioning(lon, lat, azimuthN, angleindex) {
    try {
      const param = new Param();
      param.pointLen = lon.length;
      param.azimuthNLength = azimuthN.length;
      param.mindistance = config.intersectionPositioning.mindistance;
      param.angleindexLength = angleindex.length;
      param.lonLength = lon.length;
      param.latLength = lat.length;
      param.angleindex = angleindex;
      param.azimuthN = azimuthN;
      param.lat = lat;
      param.lon = lon;
      param.result = [];
      param.resultLength = 0;

      await new Promise((resolve, reject) => {
        dll.intersectionPositioning.async(param.ref(), (err) => {
          if (err) {
            reject(err);
          }
          resolve();
        });
      });
      if (param.resultLength > 0) {
        const inputLngLat = [];
        for (let i = 0; i < param.resultLength && i < 100; i += 2) {
          const longitude = param.result[i].toFixed(6);
          const latitude = param.result[i + 1].toFixed(6);
          let exist = false;
          for (let j = 0; j < inputLngLat.length; j += 2) {
            if (
              inputLngLat[j] === longitude &&
              inputLngLat[j + 1] === latitude
            ) {
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
        const resMessage = {
          jsonrpc: '2.0',
          id: 0,
          result: {
            taskID: this.cloudTaskID,
            timestamp: Math.round(new Date().getTime() * 1000000),
            dataCollection: [],
          },
        };
        resMessage.result.dataCollection.push({
          type: 'location',
          longitude: heatmapData.optimalLng,
          latitude: heatmapData.optimalLat,
          data: heatmapData.heatmapLngLat,
        });
        this.sendDataToClient(resMessage);
        logger.debug('交汇定位计算完成');
      } else {
        logger.warn('交汇定位计算无结果');
      }
    } catch (error) {
      logger.error('交汇定位计算出错');
      logger.error(error);
    }
  }
}

module.exports = intersectionPositioning;
