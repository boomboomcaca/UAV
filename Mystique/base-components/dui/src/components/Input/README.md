
## 何时使用

- 需要用户输入表单域内容时。
- 提供组合型输入框，带搜索的输入框，还可以进行大小选择。

## API

### Input props

| 参数 | 说明 | 类型 | 默认值 |
| --- | --- | --- | --- |
| className | 类名 | string | - |
| style | 自定义内联样式 | - | - |
| name | input标签的name属性值 | string | - |
| value | 输入框内容 | string \| number | - |
| defaultValue | 初始值 | string \| number | - |
| onChange | 输入框内容变化时的回调 | function(value,error) | - |
| onPressEnter | 按下回车的回调 | function(value) | - |
| size | 控件大小。 | `large` \| `normal` | normal |
| maxLength | 最大长度 | number | - |
| suffix | 带有后缀内容的 input | ReactNode | - |
| allowClear | 可以点击清除图标删除内容 | boolean | false |
| showSearch | 显示搜索图标 | boolean | false |
| onSearch | 点击搜索图标或按下回车键时的回调 | function(value) | - |
| rules | 校验规则，设置字段的校验逻辑。 | Rule\[] | - |

rules校验插件文档 [async-validator](https://github.com/yiminghe/async-validator) 

Input 的其他属性和 React 自带的 [input](https://reactjs.org/docs/dom-elements.html#all-supported-html-attributes) 一致。


