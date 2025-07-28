
## 何时使用

- 作为一个弹层基础容器组件使用

## API

### Modal props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| visible | 是否可见 | boolean | false |
| popupTransition | 动画效果 | string | rtg-slide-right |
| maskclosable | 点击蒙层是否允许关闭 | boolean | false |
| destroyOnClose | 关闭时销毁 Drawer 里的子元素 | boolean | false |
| mask | 是否展示遮罩 | boolean | true |
| onCancel | 点击遮罩层的回调 | function(e) | - |
| usePortal | 是否渲染在body, 为fasle时渲染在父元素，为true时渲染在body，为字符串时，为document.querySelector的参数 | string \| boolean | true |



