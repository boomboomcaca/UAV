# searchlight-commons

## <a name="Contents" id="Contents">目录</a>

[AudioPlayer : 音频播放器(PCMPlayer,MP3Player)](#AudioPlayer)  
[ComboList : 用于消息展示(可能会移入 dui !慎用)](#ComboList)  
[DataGenerator : 数据重采样与门限算法](#DataGenerator)  
[EnumSelector : 组合式数据选择器](#EnumSelector)  
[FrameView : 原始数据回放组件](#FrameView)  
[FrequencyInput : 频率输入组件](#FrequencyInput)  
[Header : 应用统一头部组件](#Header)  
[ImageCapture : 应用统一截图界面(无自带截图使用 main 截图)](#ImageCapture)  
[NumberInput : 数字输入组件(来自 FrequencyInput)](#NumberInput)  
[Popup : 弹出组件(PopupDrawer,PopupModal !慎用,请使用 dui 中 popup)](#Popup)  
[PropertyList : 参数列表](#PropertyList)  
[ReplayList : 回放列表](#ReplayList)  
[SegmentsEditor : 频段编辑组件](#SegmentsEditor)  
[SegmentViewX : 频段显示组件](#SegmentViewX)  
[SliderBar : 电平质量指示器](#SliderBar)
[ScaleSliderBar : 衰减控制](#ScaleSliderBar)  
[ColumnScaleSlider :衰减控制（竖版）](#ColumnScaleSlider)
[BubbleSelector:按钮选择气泡弹框](#BubbleSelector)
[Popover:自定义气泡弹框](#Popover)
[SpectrumTemplate : 频谱模板选择](#SpectrumTemplate)  
[SpectrumUnitConverter : 频谱单位转换](#SpectrumUnitConverter)  
[StationSelectorLite : 站点功能选择](#StationSelectorLite)  
[StatusControlBar : 应用统一底部工具条](#StatusControlBar)  
[TaskInfoModal : 应用统一任务状态弹窗](#TaskInfoModal)  
[TemplateManager : 应用统一任务状态弹窗](#TemplateManager)  
[WBDFPolar : 宽带测向示向度圆盘图](#WBDFPolar)
[PathAxis: "射电天文电测"路径轴](#PathAxis)
[PathColumnAxis: "射电天文电测"竖版路径轴](#PathColumnAxis)
[PathDetailAxis: "射电天文电测"详情路径轴](#PathDetailAxis)
[PathSettingAxis: "射电天文电测"任务设置路径轴](#PathSettingAxis)
[CircuitChange: "射电天文电测"电路切换](#CircuitChange)
[PathColumnSetAxis: "射电天文电测"竖版可设置轴](#PathColumnSetAxis)

### <a name="AudioPlayer" id="AudioPlayer">AudioPlayer</a>

> [返回目录](#Contents)

- class 音频解调数据播放器，内部已集成 PCMPlayer 和 MP3Player 自动切换
- API ：[Demo](#AudioPlayer_Demo)
- PCMPler & MP3Plyer 见[拓展](#AudioPlayer_More)

|        Name         | Member      | Description                  | Remark                             |
| :-----------------: | :---------- | :--------------------------- | :--------------------------------- |
| constructor(option) | constructor | 构造函数                     | [option](#AudioPlayer_option) 可选 |
|     feed(data)      | function    | Uint8Array 设置 MP3 裸流数据 | [data](#AudioPlayer_data)          |
|      destroy()      | function    | 销毁播放器                   |                                    |
|      suspend()      | function    | 暂停播放                     |                                    |
|      resume()       | function    | 继续播放                     |                                    |

- <a name="AudioPlayer_option" id="AudioPlayer_option">option</a> 构造参数

```js
{
    streamType: 'demodstream',//string ['demodstream', 'filestream'] default 'demodstream'
}
```

- <a name="AudioPlayer_data" id="AudioPlayer_data">data</a> 播放数据

```js
data:{
  data,// 音频数据
  format,// 音频格式
  samplingRate,// 采样率
  channelNumber,// 声道数
  bitsPerSample, // 采样数据位数
}
```

- <a name="AudioPlayer_Demo" id="AudioPlayer_Demo">Demo</a>

```js
const audioPlayer = new AudioPlayer();
// 设置播放数据
audioPlayer.feed(data);
// 销毁播放器
audioPlayer.destroy();
```

- <a name="AudioPlayer_More" id="AudioPlayer_More">拓展</a>

  1.  PCMPlayer

  - class PCM 裸流播放器
  - API

  | Name                | Member      | Description                  | Remark |
  | ------------------- | ----------- | ---------------------------- | ------ |
  | constructor(option) | constructor | 构造函数                     |        |
  | feed(data)          | function    | Uint8Array 设置 PCM 裸流数据 |        |
  | destroy()           | function    | 销毁播放器                   |        |
  | suspend()           | function    | 暂停播放                     |        |
  | resume()            | function    | 继续播放                     |        |
  | setVolume()         | function    | 设置播放器音量               |        |

  - 构造函数参数

  ```js
  option:{
      encoding: '16bitInt', // 码流位数
      channels: 1, // 声道数
      sampleRate: 22050, // 音频采样率
      flushingTime: 1000, // 数据播放处理时间间隔，可选参数
      }
  ```

  - Demo

  ```js
  const audioPlayer = new PCMPlayer({
    encoding: '16bitInt',
    channels: 1,
    sampleRate: 22050,
    flushingTime: 1000,
  });
  // 设置播放数据
  audioPlayer.feed(data);
  // 销毁播放器
  audioPlayer.destroy();
  ```

  2.  MP3Player

  - class MP3 及其它音频流播放器
  - API

  | Name                | Member      | Description                  | Remark |
  | ------------------- | ----------- | ---------------------------- | ------ |
  | constructor(option) | constructor | 构造函数                     |        |
  | feed(data)          | function    | Uint8Array 设置 MP3 裸流数据 |        |
  | destroy()           | function    | 销毁播放器                   |        |
  | suspend()           | function    | 暂停播放                     |        |
  | resume()            | function    | 继续播放                     |        |

  - Demo

  ```js
  const audioPlayer = new MP3Player({
    encoding: '16bitInt',
    channels: 1,
    sampleRate: 22050,
    flushingTime: 1000,
  });
  // 设置播放数据
  audioPlayer.feed(data);
  // 销毁播放器
  audioPlayer.destroy();
  ```

### <a name="ComboList" id="ComboList">ComboList</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#ComboList_Demo)

|     Name     | MemberType | Description       | Remark                                |
| :----------: | :--------- | :---------------- | :------------------------------------ |
|   mainIcon   | any        | 主图标            | 可选                                  |
|   dropIcon   | any        | 下拉图标          | 可选                                  |
|    values    | array      | 所有要展示的值    | obsolete                              |
|    value     | any        | 新增的一个值      | string 或 [object](#ComboList_object) |
| attachValues | array      | 附加的多个值      | 多个 value                            |
|  maxLength   | number     | 最多缓存条数      |                                       |
|   disabled   | bool       | 是否可用          |                                       |
|   visible    | bool       | 是否可见          |                                       |
|  className   | any        | 组件顶层 div 样式 |                                       |

- <a name="ComboList_object" id="ComboList_object">object</a>

```js
var val0 = '默认信息';
var val1 = { type: 'info', msg: '提示信息' };
var val1 = { type: 'error', msg: '错误信息' };
var val1 = { type: 'warning', msg: '警告信息' };
```

- <a name="ComboList_Demo" id="ComboList_Demo">Demo</a>

```jsx
<ComboList
  // mainIcon={<div />}
  // dropIcon={<div />}
  className={styles.list}
  values={[]}
  value={{ type: 'error', msg: '提示信息' }}
  attachValues={[{ type: 'error', msg: '提示信息' }]}
  maxLength={20}
/>
```

### <a name="DataGenerator" id="DataGenerator">DataGenerator</a>

> [返回目录](#Contents)

- js function
- API ：[Demo](#DataGenerator_Demo)

|                      Name                       | MemberType                        | Description  | Remark |
| :---------------------------------------------: | :-------------------------------- | :----------- | :----- |
|            dataResample(data,point)             | function(array,number)            | 数据重采样   |        |
| getMultiThreshold(refThreshold, thresholdCount) | function([{frequency}...],number) | 获取多值门限 |        |

- <a name="DataGenerator_Demo" id="DataGenerator_Demo">Demo</a>

```js
dataResample([1, 2], 10);
getMultiThreshold([{ frequency: 100 }, { frequency: 200 }], 20);
```

### <a name="EnumSelector" id="EnumSelector">EnumSelector</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#EnumSelector_Demo)

|      Name      | MemberType | Description  | Remark               |
| :------------: | :--------- | :----------- | :------------------- |
|    caption     | string     | 标题         |                      |
|     value      | any        | 当前选择的值 |                      |
|     items      | array      | 一层的数据   | 与 levelItems 冲突   |
|   levelItems   | array      | 两层的数据   | 与 items 冲突        |
|  popContainer  | any        | 弹出位置     |                      |
| onValueChanged | func       | 值改变事件   | (index,value)        |
|    options     | object     | 图标         | {leftIcon,rightIcon} |
|    disable     | bool       | 可用状态     |                      |

- <a name="EnumSelector_Demo" id="EnumSelector_Demo">Demo</a>

```jsx
const data1 = [
  { value: 50, display: '50 kHz' },
  { value: 100, display: '100 kHz' },
  { value: 150, display: '150 kHz' },
  { value: 500, display: '500 kHz' },
  { value: 200, display: '200 kHz' },
];
const data2 = [
  {
    caption: '广播',
    items: [
      { value: 50, display: 'ch1|50 kHz' },
      { value: 100, display: 'ch2|100 kHz' },
      { value: 150, display: 'ch3|150 kHz' },
    ],
  },
  {
    caption: '电视',
    items: [
      { value: 55, display: 'ch1|50 kHz' },
      { value: 110, display: 'ch1|100 kHz' },
      { value: 150, display: 'ch1|150 kHz' },
      { value: 205, display: 'ch1|200 kHz' },
      { value: 505, display: 'ch1|500 kHz' },
    ],
  },
];
<>
  <EnumSelector
    caption="带宽"
    items={data1}
    value={selValue1}
    onValueChanged={(index, value) => {
      console.log('bandwidth changed', value);
      setSelValue1(value);
    }}
  />
  <EnumSelector
    levelItems={data2}
    caption="信道"
    value={selValue2}
    onValueChanged={(index, value) => {
      console.log('frequency changed', index, value);
      setSelValue2(value);
    }}
  />
</>;
```

### <a name="FrameView" id="FrameView">FrameView</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#FrameView_Demo)

|      Name      | MemberType | Description            | Remark                                           |
| :------------: | :--------- | :--------------------- | :----------------------------------------------- |
|     wsurl      | string     | 回放服务的 Socket 地址 |                                                  |
|  replayParam   | any        | 回放数据               | 从数据库来 [replayParam](#FrameView_replayParam) |
|    children    | element    | 数据展示区域           | 数据可能来源于数据回调                           |
|      menu      | element    | 左下角菜单             | 可选                                             |
| onDataCallback | func       | 数据回调               | 数据回调                                         |
|   onRunState   | func       | 播放状态               | (bool)                                           |

- <a name="FrameView_object" id="FrameView_replayParam">replayParam</a>

```js
var replayParam = { id: 50810904, edgeID: '10002', taskID, params, path, recordCount, sourceFile };
```

- <a name="FrameView_Demo" id="FrameView_Demo">Demo</a>

```jsx
<FrameView
  wsurl={wsurl}
  replayParam={replayParam}
  onDataCallback={(res) => {
    setData(JSON.stringify(res));
  }}
  menu={<Button> hello world! </Button>}
>
  <div
    style={{
      wordBreak: 'break-all',
      width: '30%',
      height: '30%',
      position: 'absolute',
      right: 200,
      bottom: 200,
      overflowY: 'auto',
      overflowX: 'hidden',
      color: 'white',
      fontSize: 10,
    }}
  >
    {data}
  </div>
</FrameView>
```

### <a name="FrequencyInput" id="FrequencyInput">FrequencyInput</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#FrequencyInput_Demo)

|      Name       | MemberType | Description  | Remark                       |
| :-------------: | :--------- | :----------- | :--------------------------- |
|      value      | number     | 当前值       |                              |
|  onValueChange  | func       | 值改变事件   |                              |
|    className    | string     | 样式         |                              |
|    decimals     | number     | 小数点数     |                              |
|    miniValue    | number     | 最小值       |                              |
|    maxValue     | number     | 最大值       |                              |
|     disable     | bool       | 是否可用     |                              |
|    hideKeys     | array      | 隐藏某些按键 | [keys](#FrequencyInput_keys) |
| unavailableKeys | array      | 禁用某些按键 |                              |

- <a name="FrequencyInput_keys" id="FrequencyInput_keys">keys</a>

```js
var keys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-', '+/-'];
```

- <a name="FrequencyInput_Demo" id="FrequencyInput_Demo">Demo</a>

```jsx
    <FrequencyInput value={value} onValueChange={(v) => setValue(v)} unavailableKeys={['+/-']} />
    <FrequencyInput.Modal value={value} onValueChange={(v) => setValue(v)} unavailableKeys={['+/-']} />
```

### <a name="Header" id="Header">Header</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#Header_Demo)

|      Name       | MemberType | Description        | Remark                                       |
| :-------------: | :--------- | :----------------- | :------------------------------------------- |
|      title      | any        | 标题               | 可选                                         |
|    showIcon     | array      | 可见图标           | 优先于 hideIcon [MenuType](#Header_MenuType) |
|    hideIcon     | array      | 隐藏图标           | ~showIcon [MenuType](#Header_MenuType)       |
|  showRightLogo  | bool       | 显示右侧图标       |                                              |
|    children     | any        | 自定义图标放于右侧 |                                              |
|  disabledState  | any        | 禁用图标           | [MenuType](#Header_MenuType)                 |
|     visible     | bool       | 是否可见           |                                              |
|    className    | any        | 组件顶层 div 样式  |                                              |
|      style      | any        | 组件顶层 div 样式  |                                              |
| onMenuItemClick | func       | 默认图标点击事件   | [MenuType](#Header_MenuType)                 |

- <a name="Header_MenuType" id="Header_MenuType">MenuType</a>

```js
const MenuType = {
  RETURN: 0x00,
  HOME: 0x01,
  MESSAGE: 0x02,
  MORE: 0x03,
  INFO: 0x04,
};
```

- <a name="Header_Demo" id="Header_Demo">Demo</a>

```jsx
const onMenuItemClick = (type) => {
  window.console.log(type);
  switch (type) {
    case MenuType.RETURN:
      break;
    case MenuType.HOME:
      break;
    case MenuType.MESSAGE:
      break;
    case MenuType.MORE:
      break;
    case MenuType.INFO:
      break;
    default:
      break;
  }
};
<Header
  title={title}
  showIcon={[MenuType.HOME, MenuType.MESSAGE]}
  hideIcon={[MenuType.MESSAGE]}
  onMenuItemClick={onMenuItemClick}
>
  <EyeOpenIcon
    key={1}
    onClick={() => {
      window.console.log('click');
    }}
  />
  <EyeOpenIcon key={2} />
</Header>;
```

### <a name="ImageCapture" id="ImageCapture">ImageCapture</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#ImageCapture_Demo)

|  Name   | MemberType | Description        | Remark                         |
| :-----: | :--------- | :----------------- | :----------------------------- |
| imgURL  | any        | 主图标             | [imgURL](#ImageCapture_imgURL) |
|  title  | string     | 文件下载时名称前缀 | 可选                           |
| timeout | number     | 下载时延           |                                |

- <a name="ImageCapture_imgURL" id="ImageCapture_imgURL">imgURL</a>

```js
const imgURL = { url: data.uri, timestamp: new Date().getTime() };
```

- <a name="ImageCapture_Demo" id="ImageCapture_Demo">Demo</a>

```jsx
<ImageCapture imgURL={uri} />
```

### <a name="NumberInput" id="NumberInput">NumberInput</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#NumberInput_Demo)

|      Name       | MemberType | Description  | Remark                    |
| :-------------: | :--------- | :----------- | :------------------------ |
|      value      | number     | 当前值       |                           |
|     suffix      | any        | 水印         |                           |
|  onValueChange  | func       | 值改变事件   |                           |
|    className    | string     | 样式         |                           |
|    decimals     | number     | 小数点数     |                           |
|    miniValue    | number     | 最小值       |                           |
|    maxValue     | number     | 最大值       |                           |
|     disable     | bool       | 是否可用     |                           |
| unavailableKeys | array      | 禁用某些按键 | [keys](#NumberInput_keys) |

- <a name="NumberInput_keys" id="NumberInput_keys">keys</a>

```js
var keys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-', '+/-'];
```

- <a name="NumberInput_Demo" id="NumberInput_Demo">Demo</a>

```jsx
<NumberInput
  value={value}
  suffix="xHz/μV"
  onValueChange={(v) => setValue(v)}
  unavailableKeys={['+/-']}
  className={styles.input}
/>
```

### <a name="Popup" id="Popup">Popup</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#Popup_Demo)

|      Name      | MemberType | Description     | Remark             |
| :------------: | :--------- | :-------------- | :----------------- |
|    visible     | bool       | 可见状态        |                    |
|    children    | any        | 内容            |                    |
|    popStyle    | any        | 弹出样式        |                    |
|  popClassName  | any        | 弹出样式        |                    |
|  closeOnMask   | bool       | 通过 mask 关闭  |                    |
|     ghost      | bool       | mask 是否响应等 |                    |
|    onClose     | func       | 关闭事件        |                    |
|  getContainer  | any        | 弹出位置        | bool or element_ID |
| destoryOnClose | bool       | 关闭是否销毁    |                    |

- <a name="Popup_Demo" id="Popup_Demo">Demo</a>

```jsx
<Popup
  style={{ backgroundColor: 'transparent' }}
  popStyle={{
    width: '30%',
    height: '100%',
    backgroundColor: '#80802080',
  }}
  visible={showPopup1}
  onClose={() => {
    setShowPopup1(false);
  }}
>
  hello wolrd!
</Popup>
```

- <a name="Popup_More" id="Popup_More">其它</a>

```js
PopupDrawer.propTypes = {
  ...Popup.propTypes,
  title: PropTypes.string, // 标题
  children: PropTypes.any, // 内容
  contentStyle: PropTypes.any,
  contentClassName: PropTypes.any,
  titleAttach: PropTypes.any, // 副标题
  onTitleAttachClick: PropTypes.func, // obsolete
  showClose: PropTypes.bool,
};
PopupModal.propTypes = {
  ...Popup.propTypes,
  title: PropTypes.string,
  children: PropTypes.any,
};
```

### <a name="PropertyList" id="PropertyList">PropertyList</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#PropertyList_Demo)

|      Name       | MemberType | Description | Remark                    |
| :-------------: | :--------- | :---------- | :------------------------ |
|     enable      | bool       | 可用状态    |                           |
|     params      | array      | 参数        |                           |
|     filter      | any        | 过滤器      | "notinstall" or "install" |
|    hideKeys     | array      | 隐藏参数    | 根据实际参数的 name 数组  |
| OnParamsChanged | func       | 参数值修改  | (params,name,old,value)   |

- <a name="PropertyList_Demo" id="PropertyList_Demo">Demo</a>

```jsx
const params=[{name,value,}] // 详见边缘端参数说明
<PropertyList
  enable
  params={params}
  hideKeys={['polarization', 'test2']}
  OnParamsChanged={(ps, name, old, val) => {
    // 参数值变更
    window.console.log({ ps, name, old, val });
  }}
/>
```

### <a name="ReplayList" id="ReplayList">ReplayList</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#ReplayList_Demo)

|      Name       | MemberType | Description    | Remark                   |
| :-------------: | :--------- | :------------- | :----------------------- |
|      data       | any        | 回放数据       | [data](#ReplayList_data) |
|    syncData     | any        | 正在同步数据   |                          |
|      type       | any        | 类型           | single or segment        |
|    pageSize     | number     | 每次新增数据量 |                          |
|    sortLocal    | bool       | 本地排序       | false                    |
|  onPageChange   | func       | 新增数据       |                          |
|  onSortChange   | func       | 排序数据       |                          |
|  onDeleteItems  | func       | 删除数据       |                          |
|   onPlayback    | func       | 回放           |                          |
|   onPlaysync    | func       | 同步           |                          |
|   refreshKey    | any        | 组件刷新       |                          |
|   showSearch    | bool       | 显示搜索       | false                    |
| onSearchChanged | func       | 搜索数据       |                          |

- <a name="ReplayList_data" id="ReplayList_data">data</a>

```js
var data = { result: [{ id, edgeID, params }], total };
```

- <a name="ReplayList_Demo" id="ReplayList_Demo">Demo</a>

```jsx
<ReplayList
  pageSize={10}
  data={{ result: [] }}
  syncData={syncData}
  onDeleteItems={(items) => {
    message.info({
      content: `删除 ${items.map((i) => {
        return `${i.id},`;
      })}`,
      key: 'info',
      duration: 1,
    });
    ids = [
      ...ids,
      ...items.map((i) => {
        return i.id;
      }),
    ];
    setListData({
      result: data.result
        .filter((dr) => {
          return !ids.includes(dr.id);
        })
        .slice((pageRef.current - 1) * pageSizeRef.current, pageRef.current * pageSizeRef.current),
      total: data.total,
    });
  }}
  onPageChange={(page, pagesize) => {
    message.info({
      content: `翻页 ${page} ${pagesize}`,
      key: 'info',
      duration: 1,
    });
    pageRef.current = page;
    pageSizeRef.current = pagesize;
    setListData({
      result: data.result.slice((page - 1) * pagesize, page * pagesize),
      total: data.total,
    });
  }}
  onPlayback={(item) => {
    if (item.percentage)
      message.info({
        content: item.id,
        key: 'info',
        duration: 1,
      });
  }}
  onPlaysync={(item, setSync) => {
    if (item.percentage)
      message.info({
        content: item.id,
        key: 'info',
        duration: 1,
      });
    if (setSync) {
      // setSync(true);
      // setTimeout(() => {
      //   setSync(false);
      // }, 2000);
      setTimeout(() => {
        setSyncData({ rate: '20%', sourceFile: item.sourceFile });
      }, 200);
      setTimeout(() => {
        setSyncData({ rate: '30%', sourceFile: item.sourceFile });
      }, 500);
      setTimeout(() => {
        setSyncData({ rate: '50%', sourceFile: item.sourceFile });
      }, 800);
      setTimeout(() => {
        setSyncData({ rate: '75%', sourceFile: item.sourceFile });
      }, 800);
      // setTimeout(() => {
      //   setSyncData({ rate: '0%', sourceFile: item.sourceFile });
      // }, 0);
      setTimeout(() => {
        setSyncData({ rate: '100%', sourceFile: item.sourceFile });
      }, 2000);
    }
  }}
  onSearchChanged={(str) => {
    window.console.log(str);
  }}
/>
```

### <a name="SegmentsEditor" id="SegmentsEditor">SegmentsEditor</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#SegmentsEditor_Demo)

|      Name       | MemberType | Description        | Remark                                             |
| :-------------: | :--------- | :----------------- | :------------------------------------------------- |
|  onSegChanged   | func       | 频段变化事件       | ({segment})                                        |
|  onViewChanged  | func       | 下拉图标           | (data)                                             |
|    treeData     | array      | 频段池的树结构数据 | [treeData](#SegmentsEditor_treeData)               |
|    tableData    | array      | 新增的一个值       | [tableData](#SegmentsEditor_tableData)             |
|  onTreeSelect   | func       | 频段选择           | ([key], data)                                      |
| initSegmentData | array      | 初始化频段数据     | [initSegmentData](#SegmentsEditor_initSegmentData) |
|    editable     | bool       | 是否可编辑         |                                                    |
|    disabled     | bool       | 是否可用           |                                                    |
|    maxCount     | number     | 最大段数           |                                                    |
|    leftWidth    | number     | 左侧宽度           |                                                    |
|   rightWidth    | number     | 右侧宽度           |                                                    |

- <a name="SegmentsEditor_initSegmentData" id="SegmentsEditor_initSegmentData">initSegmentData</a>

```js
var initSegmentData = [
  {
    id: 'dfault_1',
    name: '广播',
    startFrequency: 87,
    stopFrequency: 108,
    stepFrequency: 25,
  },
  {
    id: 'dfault_2',
    name: '广播',
    startFrequency: 87,
    stopFrequency: 108,
    stepFrequency: 25,
  },
];
```

- <a name="SegmentsEditor_treeData" id="SegmentsEditor_treeData">treeData</a>

```js
var treeData = [
  {
    id: '02541eff-7d61-4956-a33d-a3256cfc0d58',
    name: '固定',
    isUserDefine: 0,
    segmentType: [
      {
        id: '1131ae9c-292d-426f-97b2-60fa04b86d61',
        name: '次要业务',
        level: 1,
        businessID: '02541eff-7d61-4956-a33d-a3256cfc0d58',
        isUserDefine: 0,
      },
      {
        id: '42b216ca-840b-4a6f-abca-eae9cdc6dfbe',
        name: '主要业务',
        level: 0,
        businessID: '02541eff-7d61-4956-a33d-a3256cfc0d58',
        isUserDefine: 0,
      },
    ],
  },
];
```

- <a name="SegmentsEditor_tableData" id="SegmentsEditor_tableData">tableData</a>

```js
var tableData = [
  {
    id: '43db1c6e-45ac-4ce5-9413-96fd1dd39df6',
    segmentTypeID: '9fc2d735-ff7d-4d23-a239-3d47e53210e0',
    name: '频段4',
    startFreq: 11700,
    stopFreq: 12200,
    freqStep: 25,
    bandwidth: 25,
    isUserDefine: 0,
    remark: '',
    segmentID: '',
    mode: 0,
  },
];
```

- <a name="SegmentsEditor_Demo" id="SegmentsEditor_Demo">Demo</a>

```jsx
<SegmentsEditor
  treeData={TreeData}
  tableData={TableData}
  editable={segEditable}
  onTreeSelect={([key], data) => {
    window.window.console.log(key, data);
  }}
  onViewChanged={(data) => {
    window.window.console.log(data);
  }}
  onSegChanged={(d) => {
    window.window.console.log(d);
    setSegments(JSON.parse(JSON.stringify(d.segment)));
    if (d.segment.length > 2) {
      setEditable(false);
    }
  }}
/>
```

### <a name="SegmentViewX" id="SegmentViewX">SegmentViewX</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#SegmentViewX_Demo)

|      Name       | MemberType | Description    | Remark                              |
| :-------------: | :--------- | :------------- | :---------------------------------- |
|  minFrequency   | number     | 主图标         | 20                                  |
|  maxFrequency   | number     | 下拉图标       | 8000                                |
|    stepItems    | array      | 所有要展示的值 | [12.5, 25, 50, 100, 200, 500, 1000] |
|    segments     | array      | 新增的一个值   | [segments](#SegmentViewX_segments)  |
|  onValueChange  | func       | 附加的多个值   | 多个 value                          |
| visibleSegments | array      | 最多缓存条数   |                                     |
|    disabled     | bool       | 是否可用       | false                               |
| alternateColor  | string     | 是否可见       | '#1a263e'                           |
|   leftOffset    | number     | 左侧偏移       | 100                                 |
|   rightOffset   | number     | 右侧偏移       | 0                                   |
|    editable     | bool       | 是否可编辑     | true                                |

- <a name="SegmentViewX_segments" id="SegmentViewX_segments">segments</a>

```js
var segments = [
  {
    id: 'eb85e0d2-7844-6ef6-6141-95228d96b22b',
    name: '频段-1',
    startFrequency: 87,
    stopFrequency: 108,
    stepFrequency: 25,
  },
];
```

- <a name="SegmentViewX_Demo" id="SegmentViewX_Demo">Demo</a>

```jsx
<SegmentViewX
  onValueChange={(e) => {
    window.console.log(e);
    const newSeg = { ...segments[e.index], ...e.segment };
    const newSegs = [newSeg];
    setSegments(newSegs);
  }}
  segments={segments}
  minFrequency={20}
  maxFrequency={8000}
  // 显示频段索引数组
  // visibleSegments={[0]}
  // stepItems
/>
```

### <a name="SliderBar" id="SliderBar">SliderBar</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#SliderBar_Demo)

|        Name         | MemberType | Description       | Remark                                  |
| :-----------------: | :--------- | :---------------- | :-------------------------------------- |
|       minimum       | any        | 最小值            | -20                                     |
|       maximum       | any        | 最大值            | 120                                     |
|        value        | array      | 实时值            |                                         |
|     sliderValue     | any        | slider 游标值     | 门限                                    |
|      unitName       | array      | 单位              |                                         |
| onSliderValueChange | number     | slider 游标值改变 |                                         |
|    colorOptions     | bool       | 是否可用          | [colorOptions](#SliderBar_colorOptions) |

- <a name="SliderBar_object" id="SliderBar_object">object</a>

```js
var colorOptions = {
  tickColor: '#3E4269',
  tickLabel: '#F5F5F5',
  sliderLabel: '#4CD5C7',
  backgroundFill: '#181B38',
  backColor: '#0C0D26',
  gradientColors: ['#D1324C', '#214CF2'],
};
```

- <a name="SliderBar_Demo" id="SliderBar_Demo">Demo</a>

```jsx
<SliderBar
  value={sliderVal}
  unitName="°"
  minimum={sliderMin}
  maximum={100}
  sliderValue={50}
  colorOptions={{
    gradientColors: ['#29CCBE'],
  }}
  onSliderValueChange={(e) => console.log(e)}
/>
```

### <a name="SpectrumTemplate" id="SpectrumTemplate">SpectrumTemplate</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#SpectrumTemplate_Demo)

|      Name      | MemberType | Description    | Remark                                   |
| :------------: | :--------- | :------------- | :--------------------------------------- |
|   templates    | any        | 模板数据       | [templates](#SpectrumTemplate_templates) |
|    baseUrl     | any        | 缩略图服务地址 |                                          |
| onSelectChange | array      | 选择事件       |                                          |
|   onLoadMore   | any        | 加载更多       |                                          |

- <a name="SpectrumTemplate_templates" id="SpectrumTemplate_templates">templates</a>

```js
var templates = [
  {
    id: '0d90aa50-f660-11eb-bc93-5d96e59a560c',
    name: 'nmuioi',
    bandwidth: 500,
    remark: null,
    status: 1,
    updateTime: '2021-08-06 10:47:46',
    useId: 1,
  },
];
```

- <a name="SpectrumTemplate_Demo" id="SpectrumTemplate_Demo">Demo</a>

```jsx
<SpectrumTemplate
  baseUrl="http://192.168.102.191:10000"
  templates={moreTemplates}
  onCancel={() => window.console.log('canel select template')}
  onConfirm={(e) => window.console.log('template selected', e)}
  onSelectChange={(e) => {
    window.console.log(e);
    setTimeout(() => {
      e?.callback({});
    }, 2000);
  }}
  onLoadMore={() => {
    pageRef.current += 1;
    setMoreTemplates(templates.slice(0, (pageRef.current + 1) * 20));
  }}
/>
```

### <a name="SpectrumUnitConverter" id="SpectrumUnitConverter">SpectrumUnitConverter</a>

> [返回目录](#Contents)

- js function
- API ：[Demo](#SpectrumUnitConverter_Demo)

|    Name    | MemberType | Description | Remark                                  |
| :--------: | :--------- | :---------- | :-------------------------------------- |
| converters | any        | 单位转换    | [object](#SpectrumUnitConverter_object) |

- <a name="SpectrumUnitConverter_object" id="SpectrumUnitConverter_object">object</a>

```js
const converters = {
  dBm2dBuV,
  dBm2V,
  dBm2mW,
  dBuV2dBm,
  dBuV2V,
  dBuV2uV,
  dBuV2mW,
  V2dBm,
  V2dBuV,
  V2mW,
  mW2dBm,
  mW2dBuV,
  mW2V,
  spectrum2dBm,
};
```

- <a name="SpectrumUnitConverter_Demo" id="SpectrumUnitConverter_Demo">Demo</a>

```js
const { spectrum2dBm } = converters;
const test = () => {
  // 构造测试数据
  const dataCount = 1000000;
  const data = [];
  for (let i = 0; i < dataCount; i += 1) {
    data[i] = Math.random() * 65;
  }
  const startTime = new Date().getTime();
  const datas = spectrum2dBm(data);
  const endTime = new Date().getTime();
  const gap = endTime - startTime;
  setTestTip(`本次测试数据量：${dataCount}，运算耗时：${gap}ms`);
};
```

### <a name="StationSelectorLite" id="StationSelectorLite">StationSelectorLite</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#StationSelectorLite_Demo)

|     Name     | MemberType       | Description   | Remark                                    |
| :----------: | :--------------- | :------------ | :---------------------------------------- |
|   stations   | array            | 站点信息      | [stations](#StationSelectorLite_stations) |
| selectEdgeId | string or number | 选择的站点 ID | edge_id                                   |
|   onSelect   | func             | 选择事件      |                                           |
|   disabled   | bool             | 是否可用      |                                           |
|   visible    | bool             | 是否可见      |                                           |

- <a name="StationSelectorLite_stations" id="StationSelectorLite_stations">stations</a>

```js
var modules = {
  result: [
    {
      edgeID: '142857',
      type: 'stationary',
      category: '1',
      mfid: '142857',
      name: '142857',
      longitude: 104,
      latitude: 30,
      altitude: 100,
      address: '匹配经纬度',
      ip: '192.168.102.94',
      port: null,
      version: '1340860940865765376',
      remark: null,
      modules: [],
      isActive: false,
      lastUpdateTime: null,
      centerAddress: '192.168.1.1',
      zone: 'a',
      magangle: 1,
      fmaddrtype: 'a',
    },
  ],
};
```

- <a name="StationSelectorLite_Demo" id="StationSelectorLite_Demo">Demo</a>

```jsx
<StationSelectorLite
  visible={showSelector}
  stations={modules}
  onSelect={(x) => {
    window.console.log(x);
    setShowSelector(false);
  }}
/>
```

### <a name="StatusControlBar" id="StatusControlBar">StatusControlBar</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#StatusControlBar_Demo)

|   Name    | MemberType | Description               | Remark |
| :-------: | :--------- | :------------------------ | :----- |
| className | any        | 样式                      |        |
| children  | any        | Main or Message or Action |        |

- <a name="StatusControlBar_Demo" id="StatusControlBar_Demo">Demo</a>

```jsx
<StatusControlBar className={styles.bottom}>
  <StatusControlBar.Main>
    <div />
  </StatusControlBar.Main>
  <StatusControlBar.Message>
    <div />
  </StatusControlBar.Message>
  <StatusControlBar.Action>
    <div />
  </StatusControlBar.Action>
</StatusControlBar>
```

### <a name="TaskInfoModal" id="TaskInfoModal">TaskInfoModal</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#TaskInfoModal_Demo)

|   Name    | MemberType | Description | Remark                      |
| :-------: | :--------- | :---------- | :-------------------------- |
|   info    | any        | 任务信息    | [info](#TaskInfoModal_info) |
| moreClick | func       | 更多        |                             |
|  visible  | bool       | 是否可见    |                             |
| onCancel  | func       | 关闭        |                             |

- <a name="TaskInfoModal_info" id="TaskInfoModal_info">info</a>

```js
const info = {
  taskCreator: 'admin',
  creatTime: '2021.03.22 17:47:11',
  runTime: '00:23:10',
  edgeName: 'D0.7移动站01',
  edgeID: '10001',
  type: 'mobile',
  moduleState: 'deviceBusy',
  modulesName: 'DF0001A',
  latitude: 30.550624,
  longitude: 104.072941,
};
```

- <a name="TaskInfoModal_Demo" id="TaskInfoModal_Demo">Demo</a>

```jsx
<TaskInfoModal visible={visible} onCancel={() => setVisible(false)} info={info} />
```

### <a name="TemplateManager" id="TemplateManager">TemplateManager</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#TemplateManager_Demo)

|      Name       | MemberType | Description    | Remark                                  |
| :-------------: | :--------- | :------------- | :-------------------------------------- |
|     baseUrl     | string     | 缩略图服务地址 |                                         |
|   dataSource    | any        | 模板数据       | [templates](#TemplateManager_templates) |
|   onLoadMore    | func       | 加载更多       |                                         |
| onSearchChanged | func       | 搜索           |                                         |
|  onNameChanged  | func       | 修改模板名称   |                                         |
|    onDelete     | func       | 删除           |                                         |
|    onRefresh    | func       | 刷新           |                                         |

- <a name="TemplateManager_templates" id="TemplateManager_templates">templates</a>

```js
var templates = [
  {
    id: '0d90aa50-f660-11eb-bc93-5d96e59a560c',
    name: 'nmuioi',
    bandwidth: 500,
    remark: null,
    status: 1,
    updateTime: '2021-08-06 10:47:46',
    useId: 1,
  },
];
```

- <a name="TemplateManager_Demo" id="TemplateManager_Demo">Demo</a>

```jsx
<TemplateManager
  baseUrl="http://192.168.102.191:10000"
  dataSource={moreTemplates}
  onLoadMore={() => {
    pageRef.current += 1;
    setMoreTemplates(templates.slice(0, (pageRef.current + 1) * 20));
  }}
  onSearchChanged={(str) => {
    window.console.log(str);
  }}
  onNameChanged={(item, callback) => {
    window.console.log(item);
    callback('?????');
  }}
  onDelete={(items) => {
    window.console.log(items);
  }}
  onRefresh={() => {
    window.console.log('!!!!!');
  }}
/>
```

### <a name="WBDFPolar" id="WBDFPolar">WBDFPolar</a>

> [返回目录](#Contents)

- react function component
- API ：[Demo](#WBDFPolar_Demo)

|   Name   | MemberType      | Description        |
| :------: | :-------------- | :----------------- |
| bearings | Array\<Object\> | 宽带测向示向度数据 |
| quality  | number          | 质量门限(0-100)    |

```js
const bearings = [
  {
    frequency: 4, // 频率
    azimuth: 183, // 示向度
    quality: 89, // 测向质量
    color: '#D42AFF', // 颜色
    select: true, // 选中
  },
  {
    frequency: 5,
    azimuth: 252,
    quality: 73,
    color: '#5CFB7F',
  },
  {
    frequency: 6,
    azimuth: 316,
    quality: 52,
    color: '#F545D9',
  },
];

const quality = 50;
```

- <a name="WBDFPolar_Demo" id="WBDFPolar_Demo">Demo</a>

```jsx
<WBDFPolar quality={quality} bearings={bearings} />
```
