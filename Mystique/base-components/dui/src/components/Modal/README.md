
## 何时使用

- 需要用户处理事务，又不希望跳转页面以致打断工作流程时，可以使用 Modal 在当前页面正中打开一个浮层，承载相应的操作

## API

### Modal props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| maskclosable | 点击蒙层是否允许关闭 | boolean | false |
| usePortal | 是否渲染在body | boolean | true |
| visible | 对话框是否可见 | boolean | false |
| closable | 是否显示右上角的关闭按钮 | boolean | false |
| onCancel | 点击遮罩层或右上角叉或取消按钮的回调 | function(e) | - |
| onOk | 点击确定回调 | function(e) | - |
| title | 标题 | string | - |
| style | 可用于设置浮层的样式，调整浮层位置等 | - | - |
| bodyStyle | 可用于设置内容层的样式 | - | - |
| footerStyle | 可用于设置footer的样式 | - | - |
| footer | 底部内容，当不需要默认底部按钮时，可以设为 footer={null} | ReactNode | - |



