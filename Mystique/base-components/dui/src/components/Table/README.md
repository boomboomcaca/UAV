# Table

## 何时使用

- 简单表格
- 例子详见 Demo

## API

### Table props

| 参数               | 说明           | 类型   | 默认值                                                                         |
| ------------------ | -------------- | ------ | ------------------------------------------------------------------------------ |
| columns            | 列模板         | array  | null                                                                           |
| data               | 数据           | array  | null                                                                           |
| showSelection      | 显示选择列     | bool   | true                                                                           |
| currentSelections  | 当前选择行数据 | array  | [ ...dataItem ]                                                                |
| onSelectionChanged | 选择变化       | func   | (data 子集)                                                                    |
| selectRowIndex     | 选择行号       | number | -1                                                                             |
| onRowSelected      | 选择行         | func   | (item,index,toSelect:bool)                                                     |
| onColumnSort       | 列排序         | func   | ({ key: column.key, state: oneof(['none', 'asc', 'desc']) })                   |
| options            | 更多选项       | any    | { canRowSelect: false, rowHover: undefined, rowHeight: null, bordered: false } |
| loadMore           | 加载更多       | func   |                                                                                |
| canLoadMore        | 加载更多可用   | bool   | false                                                                          |
