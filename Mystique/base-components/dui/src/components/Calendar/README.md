## 何时使用

- 当用户需要输入一个日期，可以点击标准输入框，弹出日期面板进行选择。

- 日历组件严格采用dayjs库，如需使用，请确认日期为dayjs初始化

## API

### Calendar props

- 单个日期选择

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| value | 日期 | dayjs | dayjs() |
| disable | 是否禁用 | boolean | false |
| onChange | 选项变化时的回调函数 | function(date: dayjs) | - |
| style | 自定义内联样式 | - | - |
| minDate | 最小可选日期 | dayjs | - |


### Calendar.range props

- 日期区间选择

- 参数同单选


