# ToggleButton

## 何时使用

- 类似 toggle 的扩展按钮
- 例子详见 Demo

## API

### ToggleButton props

| 参数          | 说明          | 类型   | 默认值        |
| ------------- | ------------- | ------ | ------------- |
| tag           | 标记          | string | ''            |
| className     | 顶层 div 样式 | css    | null          |
| style         | 顶层 div 样式 | object | null          |
| children      | 子组件        | any    | null          |
| checked       | 子项值        | array  | []            |
| twinkling     | 指示器闪烁    | bool   | false         |
| disabled      | 可用状态      | bool   | false         |
| visible       | 可见状态      | bool   | true          |
| onClick       | 点击事件      | func   | (checked,tag) |
| onDoubleClick | 双击事件      | func   | (tag)         |
| onPress       | 长按事件      | func   | (tag)         |
