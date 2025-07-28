/* eslint-disable */
const os = require('os');
// todo：可以进一步优化把算法文件名写到配置文件，然后进行通用的操作系统判断 windwos 和 linux.
const ffi = require('ffi-napi');
const ref = require('ref-napi');
const Struct = require('ref-struct-napi');
const RefArray = require('ref-array-napi');
// ref-struct
const Data = Struct({
  // 可以考虑返回二维数组
  // 考虑使用指针返回？指针 指针 指针
  pointLen: ref.types.int,
  azimuthNLength: ref.types.int,
  mindistance: ref.types.double,
  angleindexLength: ref.types.int,
  lonLength: ref.types.int,
  latLength: ref.types.int,
  angleindex: RefArray(ref.types.int, 100),
  azimuthN: RefArray(ref.types.float, 1000),
  lat: RefArray(ref.types.double, 100),
  lon: RefArray(ref.types.double, 100),
  result: RefArray(ref.types.double, 100),
  resultLength: ref.types.double,
  leftUpLon: ref.types.double,
  leftUpLat: ref.types.double,
  rightDownLon: ref.types.double,
  rightDownLat: ref.types.double,
  width: ref.types.double,
  hight: ref.types.double,
  optimalLon: ref.types.double,
  optimalLat: ref.types.double,
  // 外层参数传递进去？
  heatMapData: RefArray(RefArray(ref.types.double, 3), 100),
});

// `ffi.Library`用于注册函数，第一个入参为 DLL 路径，最好为文件绝对路径
const dll = ffi.Library('IntersectionPositioning/IntersectionPositioning', {
  intersectionPositioning: [ref.types.void, [ref.refType(Data), 'string']],
});

exports.intersectionPositioningTest = function () {
  try {
    const data = new Data();
    const jsonData = require('../lib/algorithm/IntersectionPositioningData.json');
    const testData = jsonData[3];
    data.pointLen = testData.pointLen;
    data.azimuthNLength = testData.azimuthNLength;
    data.mindistance = testData.mindistance;
    data.angleindexLength = testData.angleindex.length;
    data.lonLength = testData.lon.length;
    data.latLength = testData.lat.length;
    data.angleindex = testData.angleindex;
    data.azimuthN = testData.azimuthN;
    data.lat = testData.lat;
    data.lon = testData.lon;
    data.result = [1, 2, 3, 4];
    data.resultLength = 0;
    data.leftUpLon = 103.741621574929;
    data.leftUpLat = 31.1352335869034;
    data.rightDownLon = 107.035465860026;
    data.rightDownLat = 29.4135506088319;
    data.width = 2000;
    data.hight = 1000;
    dll.intersectionPositioning(data.ref(), 'heatmap.png');
    // console.log('方法执行完成............ ');
    // console.log(`最优经度:${data.optimalLon} 最优纬度:${data.optimalLat}`);
    // console.log(data.result.toArray());
    for (let i = 0; i < data.resultLength; i++) {
      // console.log(data.heatMapData[i].toArray());
    }
  } catch (error) {
    // console.log(error);
  }
};
// exports.intersectionPositioningTest = function () {

//     try {
//         const certInfo = new CertGroud({ P1: 14, P2: 32 });
//         certInfo.P4 = [23333, 33312333, 1, 2, 3, 4, 9, 5];
//         console.log("sssssssssssssss11");
//         // certInfo.P5=testNumFirst;
//         dll.AddMethod(certInfo.ref());
//         console.log("sssssssssssssss");
//         //一定要输出数组长度
//         console.log(certInfo.P4.toArray());
//         //console.log(certInfo.P4);
//         //长度必须定义好
//         console.log("sssssssssssssss");
//         for (let i = 0; i < 600; i++) {
//             console.log(certInfo.Data[i].toArray());
//         }

//         // console.log(certInfo.Data[1].toArray());
//         // console.log(certInfo.Data[2].toArray());
//         // console.log(certInfo.Data[3].toArray());
//         // console.log(certInfo.Data[4].toArray());
//         console.log("sssssssssssssss");
//     } catch (error) {
//         console.log(error);
//     }

//     //回调同样会阻塞进程！和Node-C++ 插件一样的结果
// }

// 同步调用

// exports.intersectionPositioningTest=function(){
//     //测试数据100组
//     var jsonData=require('../lib/algorithm/IntersectionPositioningData.json')
//     let successCount=0;
//     for(let i=0;i<jsonData.length;i++){
//         let reqData =jsonData[i];
//         let data=algorithm.intersectionPositioning(reqData);
//         if(reqData.resultLen==data.resultLen){
//             successCount+=1;
//         }
//     }

//     console.log(`算法测试完成 总测试数据 ${jsonData.length} 成功数 ${successCount}`);
// }
