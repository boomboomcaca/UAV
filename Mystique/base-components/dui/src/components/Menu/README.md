
## 何时使用

- 为页面和功能提供导航的菜单列表。

## API

### Menu props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| value | 当前选择的value | any | - |
| onClick | 点击菜单时的回调 | function(value) | - |


### Menu.SubMenu props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| title | 含子菜单的父级节点 | reactNode | - |
| defaultOpen | 初始化展开的菜单，切换一次后失效 | bool | false |


### Menu.Item props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| key | value值，用作select判断及返回 | string | - |
| transit | 中转数据，传递值会作为menu的第二个参数原封不动返还 | any | - |