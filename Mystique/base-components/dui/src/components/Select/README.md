
下拉选择器。

## 何时使用

- 弹出一个下拉菜单给用户选择操作，用于代替原生的选择器。
- 当选项少时（少于 5 项），建议直接将选项平铺，使用 `Radio` 是更好的选择。

## API

```jsx
<Select>
  <Option value="lucy">lucy</Option>
</Select>
```

### Select props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| style | 自定义内联样式 | - | - |
| value | 指定当前选中的条目 | - | - |
| onChange | 选中 option时，调用此函数 | function(value) | - |
| name | 内置input `:type=search` 标签的name属性值 | string | - |


### Option props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| title | 选中该 Option 后，Select 的 title | string | - |
| value | 默认根据此属性值进行筛选 | - | - |