# DCFE UI 组件库

[![npm version](https://img.shields.io/badge/npm-v0.0.1-blue)](http://192.168.102.29:8000/#browse/search/npm=name.raw%3Ddui) [![gitlab](https://img.shields.io/badge/gitlab-dui-yellow)](http://gitlab.decentest.com/sw-front-team/dui)

## 开发

```shell
yarn install

yarn dev
```

---

## 注意

1. 组件开发人员非特殊情况不要修改 src/components 以外任何文件！！！
2. 新增或修改的组件在 components 根目录中操作：

   A. 一个组件只允许在该组件的文件夹中开发

   B. 新增或修改组件需在 components/index.js 和 components/demo.js 中新增相关的引用：让 build 脚本能够读取到组件模块和 demo 模块

   C. 新增文件中需要包含类似 components/Example/Demo 的【测试文件目录 】

   D. 新增文件中需要包含类似 components/Example/README.md 的组件【说明文档】

   E. 所负责的组件需要在 components/README.md 中标记

   F. less 中图片文件请转成 base64 并独立为 xxx.less 文件再引入使用

3. 由于基础工程还在调整，开发完之后先不 build 或上传到私有 npm
4. 仓库初始化的 Button 组件不是示例代码，只是用来占用目录的，开发时可以删除和覆盖

---

## 全局样式变量

JavaScript

```JavaScript
import modifyVars from '@/config/modifyVars';

console.log(modifyVars);
// modifyVars['@primary-color']
```

Less

```Less
.div-h1{
  font-size:@font-h1-size
}
```

## build

```shell
rollup -c //打包组件

cd dist
npm version patch //修复bug，较小修改
npm version minor //新增功能，兼容老版本
npm version major //新的架构调整，不兼容老版本

npm publish //发布

npm run build //打包+更新patch+发布
```

### 开发人

Select:邓云升
Radio:邓云升
Checkbox:邓云升
Input:邓云升
InputNumber:邓云升
Form:邓云升
MultipleSelect:邓云升
Button:吴虹均
Switch:吴虹均
IconSwitch:吴虹均
SelectButton:吴虹均
StationButton:吴虹均
ToggleButton:宋秋宇
ToggleButton2:宋秋宇
SwitchButton:宋秋宇
IconButton:宋秋宇
Modal:邓云升
PopUp:邓云升
Drawer:邓云升
Calendar:邓云升
Loading:宋秋宇
Empty:宋秋宇
message:宋秋宇
Cascader:宋秋宇
Table:宋秋宇
Table2:宋秋宇
ListView:宋秋宇
Expand:宋秋宇
Pagination:宋秋宇
TimeSelect:吴虹均
Progress:吴虹均
TimeScroll:吴虹均
Menu:邓云升
StickyTable:宋秋宇
ButtonRadio:吴虹均
SlidesShow:梁超
ButtonSwitch:吴虹均
TabButton:吴虹均
MultipleSwitch:梁超
CalendarYear:吴虹均
CalendarMouth:吴虹均
TimeScroll2:夏银樯
ExceedButton:宋秋宇
