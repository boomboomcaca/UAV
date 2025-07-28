# IconButton

## 何时使用

- 类似 checkbox 的扩展按钮
- 例子详见 Demo

## API

### IconButton props

| 参数      | 说明          | 类型              | 默认值        |
| --------- | ------------- | ----------------- | ------------- |
| tag       | 标记          | string            | ''            |
| loading   | 加载中        | bool              | false         |
| children  | 子组件        | any               | null          |
| className | 顶层 div 样式 | css               | null          |
| checked   | 选择状态      | bool or undefined | undefined     |
| disabled  | 可用状态      | bool              | false         |
| visible   | 可见状态      | bool              | true          |
| onClick   | 点击事件      | func              | (checked,tag) |
