# Empty

## 何时使用

- 数据为空区域占位
- 例子详见 Demo

## API

### Empty props

| 参数      | 说明            | 类型          | 默认值                       |
| --------- | --------------- | ------------- | ---------------------------- |
| className | 顶层 div 样式   | css-class     | null                         |
| message   | 描述信息        | string        | '暂无数据'                   |
| emptype   | 空类型          | Empty：string | Empty.Normal [Empty](#Empty) |
| svg       | 自定义 svg 图片 | any           | null                         |

- <a name="Empty" id="Empty">Empty</a> 类型

```js
Empty.Normal = 'EmptyNormal';
Empty.Feature = 'EmptyFeature';
Empty.Device = 'EmptyDevice';
Empty.Box = 'EmptyBox';
Empty.RunningTask = 'EmptyRunningTask';
Empty.UAV = 'EmptyUAV';
Empty.Station = 'EmptyStation';
```
