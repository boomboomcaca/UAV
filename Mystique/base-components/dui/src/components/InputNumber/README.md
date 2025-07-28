
## 何时使用

用于创建一个实体或收集信息。
需要对输入的数据类型进行校验时。

## API

### InputNumber props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| style | 自定义内联样式 | - | - |
| size | 控件大小。 | `large` \| `normal` | normal |
| value | 输入框内容 | number | - |
| defaultValue | 初始值 | number | - |
| onChange | 输入框内容变化时的回调 | function(value) | - |
| min | 最小值 | number | - |
| max | 最大值 | number | - |
| step | 每次改变步数，可以为小数 | number | - |
| digits | 小数位数 | number | 2 |
| suffix | 带有后缀内容的 input | ReactNode | - |