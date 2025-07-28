# Cascader

## 何时使用

- 当用户需要选择一个级联数据时，如省市区
- 例子详见 Demo

## API

### Cascader props

| 参数          | 说明                  | 类型                                        | 默认值                    |
| ------------- | --------------------- | ------------------------------------------- | ------------------------- |
| values        | 当前选择的值          | array                                       | null                      |
| options       | 所有选项              | tree-array                                  | null                      |
| className     | 顶层 div 样式         | css-class                                   | null                      |
| keyMap        | 自定义 key-value 映射 | {KEY,LABEL,CHILDREN}:{string,string,string} | {'id','label','children'} |
| splitter      | 显示分割符            | char                                        | '/'                       |
| onSelectValue | 选择改变事件          | func                                        | (res:同 values 结构)      |
