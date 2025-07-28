
单选框。

## 何时使用

- 用于在多个备选项中选中单个状态。
- 和 Select 的区别是，Radio 所有选项默认可见，方便用户在比较中选择，因此选项不宜过多。

## API

### Radio props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| value | 根据 value 进行比较，判断是否选中 | - | - |
| onChange | 选项变化时的回调函数 | function(value) | - |
| name | 内置input `:type=radio` 标签的name属性值 | string | - |
| options | 以配置形式设置子元素 | string\[] \| Array&lt;{ label: string, value: string, disabled?: bool }> | - |  |
| valueKey | 以options数组配置时，指定value项的键值 `eg:id` | string | value |
| labelKey | 以options数组配置时，指定label项的键值 `eg:name` | string | label |

