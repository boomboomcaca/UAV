
## 何时使用

- 抽屉从父窗体边缘滑入，覆盖住部分父窗体内容。用户在抽屉内操作时不必离开当前任务，操作完成后，可以平滑地回到原任务。

## API

### Modal props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| maskclosable | 点击蒙层是否允许关闭 | boolean | false |
| visible | 对话框是否可见 | boolean | false |
| width | 抽屉宽度 | string | '40%' |
| headerIcon | 自定义关闭位置dom | ReactNode | - |
| onCancel | 点击遮罩层回调 | function(e) | - |
| title | 标题 | string | - |
| bodyStyle | 可用于设置内容层的样式 | - | - |



