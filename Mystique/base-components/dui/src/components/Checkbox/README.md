
多选框。

## 何时使用

- 在一组可选项中进行多项选择时。
- 单独使用可以表示两种状态之间的切换，和 switch 类似。区别在于切换 switch 会直接触发状态改变，而 checkbox 一般用于状态标记，需要和提交操作配合。

## API

### Checkbox props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| checked | 指定当前是否选中 | boolean | false |
| onChange | 选项变化时的回调函数 | function(checked) | - |
| name | 内置input `:type=checkbox` 标签的name属性值 | string | - |
| className | 类名 | string | - |


### Checkbox.Group props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| value | 指定选中的选项 | string[] | [] |
| onChange | 选项变化时的回调函数 | function(checkedValue) | - |
| name | 内置input `:type=checkbox` 标签的name属性值 | string | - |
| options | 以配置形式设置子元素 | string\[] \| Array&lt;{ label: string value: string }> | - |  |
| valueKey | 以options数组配置时，指定value项的键值 `eg:id` | string | value |
| labelKey | 以options数组配置时，指定label项的键值 `eg:name` | string | label |
| className | 类名 | string | - |
| optionCls | option类名 | string | - |

