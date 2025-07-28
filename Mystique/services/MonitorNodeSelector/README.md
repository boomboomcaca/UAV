# 安装

```bash
npm install monitornodeselector
```

or

```bash
yarn add monitornodeselector
```

# 引入

```javascript
import MonitorNodeSelector from "monitornodeselector";
```

# 接口说明

> **构造函数**

```javascript
  /**
   * 当前功能可用站点加载回调
   * @callback onLoadModules
   * @param {Arrary} e
   */
  /**
   * 进入功能选择默认模块加载完成回调
   * @callback onModuleInitialized
   * @param {{edgeId: String,featureId: String, edgeName: String,featureName: String,deviceName: String,address: String,moduleState: String,isActive:boolean,type:String,longitude: Number,latitude: Number,category:Number}} e
   */
  /**
   * 参数变更回调
   * @callback onParameterChanged
   * @param {{parameters:Arrary,newModule:boolean}} e
   */
  /**
   * http网络请求错误回调
   * @callback onError
   * @param {{code:Number,message:String}} e
   */
  /**
   * 构造函数
   * @param {{features:Arrary<String>, appKey:String, axios:Object, triggerParams:Object,onLoadModules:onLoadModules,onModuleInitialized:onModuleInitialized,onParameterChanged:onParameterChanged,onError:onError,onlyIdle:boolean}} options
   */
  constructor(options) {
    this.#state = options;
    this.#filterCloudNodes();
    console.log("MonitorNodeSelector instance");
  }
```

> **从服务器更新站点列表**

```javascript
/**
 * 从服务器更新站点列表，在onLoadModules中接收
 */
updateCloudNodes();
```

<font style=background:red>每次显示选站列表时调用此方法更新状态，调用此方法后会触发 onLoadModules</font>

> **更新选站信息接口**

```javascript
/**
 * 更新选站信息，更新后触发 onModuleInitialized，onParameterChanged
 * @param {Object} feature 功能信息
 * @param {Object} station 站点信息
 */
updateModuleSelect(feature, station);
```

|  Name   |  Type  | Description | Default |             Remark             |
| :-----: | :----: | :---------: | :-----: | :----------------------------: |
| feature | Object |  功能信息   |         | 选站组件中得到的选择的功能信息 |
| station | Object |  站点信息   |         | 选站组件中得到的选择的站点信息 |

<font style=background:red>调用此方法后会触发 onParameterChanged</font>

> **更新单个参数接口**

```javascript
/**
 * 更新参数，更新后触发onParameterChanged
 * 争对单独处理参数
 * @param {String} name 参数名称
 * @param {object} value 参数值
 */
updateParameter = (name, value);
```

| Name  |  Type  | Description |  Default  | Remark |
| :---: | :----: | :---------: | :-------: | :----: |
| name  | String |  参数名称   | undefined |        |
| value | String |   参数值    | undefined |        |

<font style=background:red>调用此方法后会触发 onParameterChanged</font>

> **批量更新参数接口**

```javascript
/**
 * 更新参数，更新后触发onParameterChanged
 * 争对参数批量绑定变更
 * @param {Arrary} params
 */
updateParameters(params);
```

|  Name  | Type  | Description | Default | Remark |
| :----: | :---: | :---------: | :-----: | :----: |
| params | Arary |             |         |        |

<font style=background:red>调用此方法后会触发 onParameterChanged</font>

> **保存历史信息接口**

```javascript
/**
 * 保存历史信息
 * 一般在任务启动和停止的时候调用
 */
saveCache();
```

# Demo

```javascript
useEffect(() => {
  nodeSelectorRef.current = new MonitorNodeSelector({
    features: ["scan"],
    appKey: "scan",
    axios,
    triggerParams: options, // 功能跳转带入参数
    onLoadModules: (e) => {
      // 选站组件备选列表
      // [
      //   {
      //     ...,
      //     modules:[
      //       {
      //         ...,
      //         frequency:{minimum, maximum},
      //         ifBandwidth
      //       },
      //       ...
      //     ]
      //   },
      //   ...
      // ]
      setFeatureProviders(e);
    },
    onModuleInitialized: (e) => {
      // 选择站点变更
      setSelectedMoudle(e);
    },
    onParameterChanged: (e) => {
      // 功能参数
      setModuleParams(e.parameters);
      // TODO 过滤绑定参数
      // TODO 绑定 propertylist
      }
      // 下发参数到边缘端
      socketInstance.setParameters(e.parameters);
    },
  });
}, []);
```
