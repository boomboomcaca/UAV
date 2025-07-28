# SwitchButton

## 何时使用

- 类似 switch 的扩展按钮
- 例子详见 Demo

## API

### SwitchButton props

| 参数             | 说明          | 类型    | 默认值                    |
| ---------------- | ------------- | ------- | ------------------------- |
| tag              | 标记          | string  | ''                        |
| className        | 顶层 div 样式 | css     | null                      |
| contentClassName | 内容样式      | css     | null                      |
| style            | 顶层 div 样式 | object  | null                      |
| contentStyle     | 内容样式      | object  | null                      |
| children         | 子组件        | any     | null                      |
| values           | 子项值        | array   | []                        |
| value            | 当前值        | any     | null                      |
| indicator        | 指示器图标    | element | null                      |
| disabled         | 可用状态      | bool    | false                     |
| visible          | 可见状态      | bool    | true                      |
| onClick          | 点击事件      | func    | (tag,index,values[index]) |
