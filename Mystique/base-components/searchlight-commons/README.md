<!--
 * @Author: XYQ
 * @Date: 2022-01-24 18:09:27
 * @LastEditors: XYQ
 * @LastEditTime: 2022-04-13 11:22:36
 * @Description: file content
-->
# Commons 组件库

[![npm version](https://img.shields.io/badge/npm-v0.0.1-blue)](http://192.168.102.29:8000/#browse/search/npm=name.raw%3Dsearchlight-commons) [![gitlab](https://img.shields.io/badge/gitlab-commons-yellow)](http://gitlab.decentest.com/sw-front-team/searchlight-commons)

## 说明

<a href="readme.man.md">所有组件</a>

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

## build发布

```shell
rollup -c //打包组件

cd dist
npm version patch //修复bug，较小修改
npm version minor //新增功能，兼容老版本
npm version major //新的架构调整，不兼容老版本

npm publish //发布

npm run build //打包+更新patch+发布

```
## components 拆包

### 1. 配置需要拆离的组件

> 在 pub.config.json 文件的pubcomponents 中添加对应的组件名称（名称为 ~/src/components/[name]）
> 拆包发布的组件如下
- StationSelectorLite
- StatisticDataChart
- DcBearingChart

### 2. 手动拆离发布

> 所有拆离组件一起发布
```shell
npm run all
```
> 发布指定拆离组件
```shell
npm run lite
```

# 安装使用

## 安装合并发布部分
```shell
yarn add searchlight-commons
```


## 安装拆包发布组件，以StationSelectorLite为例
```shell
yarn add @searchlight-commons/statisticdatachart
```

