# Pagination

## 何时使用

- 分页组件
- 例子详见 Demo

## API

### Pagination props

| 参数      | 说明          | 类型   | 默认值          |
| --------- | ------------- | ------ | --------------- |
| current   | 当前页        | number | 0               |
| pageSize  | 每页条数      | number | 20              |
| total     | 总条数        | number | 0               |
| className | 顶层 div 样式 | css    | null            |
| onChange  | 页数改变      | func   | (page,pageSize) |
