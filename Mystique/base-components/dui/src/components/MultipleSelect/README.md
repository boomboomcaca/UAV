下拉选择器。

## 何时使用

- 更优雅的、多选的、下拉选择器

## API

### MultipleSelect props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| style | 自定义内联样式 | - | - |
| value | 指定当前选中的条目 | string\[] | - |
| onChange | 选中 option时，调用此函数 | function(value) | - |
| name | 内置input `:type=search` 标签的name属性值 | string | - |


### Option props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| title | 选中该 Option 后，Select 的 title | string | - |
| value | 默认根据此属性值进行筛选 | string | - |