# ListView

## 何时使用

- 自动尺寸的列表组件
- 例子详见 Demo

## API

### ListView props

| 参数         | 说明          | 类型           | 默认值 |
| ------------ | ------------- | -------------- | ------ |
| className    | 顶层 div 样式 | css            | null   |
| children     | 子组件        | any            | null   |
| virtualized  | 虚拟化        | bool           | false  |
| baseSize     | 基于尺寸      | {width,height} | null   |
| loadMore     | 加载更多      | func           |        |
| dataSource   | 数据源        | array          | null   |
| itemTemplate | 数据子项模板  | func           | null   |
