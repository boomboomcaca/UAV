# Expand

## 何时使用

- 具有两层结构的树级展开
- 例子详见 Demo

## API

### Expand props

| 参数           | 说明         | 类型    | 默认值             |
| -------------- | ------------ | ------- | ------------------ |
| dataSource     | 数据源       | array   | null [data](#data) |
| itemTemplate   | 子项模板     | element | null               |
| onLoadMore     | 加载更多     | func    |                    |
| onSelectChange | 选择改变事件 | func    |                    |

- <a id="data" name="data">data</a>结构

```js
[
  {
    id,
    name,
    children: [{ id, name },...],
  },...
];
```
