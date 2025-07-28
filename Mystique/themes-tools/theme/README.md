# @dc/theme

基于 css var && typescript 设计的主题系统

> 一段时间之后，此插件会在[vite-micro-template](http://gitlab.decentest.com/sw-front-team/vite-micro-template)中被 CDN 化

> 默认情况下，主题系统随操作系统自动切换 dark 或 light 模式

## 安装

```sh
nrm use dc(npm private)
npm install @dc/theme
# or
yarn add @dc/theme
# or
pnpm add @dc/theme
```

## 使用

1. 子应用只添加在dev环境，不要打包到生产环境！！！
2. 主应用入口文件引入theme和相应位置接入控制逻辑
3. [配置对应表请参考](http://gitlab.decentest.com/sw-front-team/theme/-/blob/master/src/config.js)

src/index.* 入口文件中
```javascript
import '@dc/theme';
```

样式使用
```css
#root {
  color: var(--theme-primary);
}
```
```javascript
const style = { color: 'var(--theme-primary)' }
```

主题操作工具
``` javascript
import Theme , { Mode } from '@dc/theme/dist/tools.js';

const t = new Theme(Mode.auto);
t.getMode();
t.getTheme();
t.setTheme();
t.switchTheme();
```

调试使用👇（限子应用，开发完时请删除）
```javascript
// 入口文件
import Theme, { Mode } from '@dc/theme/dist/tools.js';
import '@dc/theme';

window.theme = new Theme(Mode.light);
// 控制台快捷调试
theme.switchTheme();
```


## 文件结构
```shell
.
├── dist               // 生产环境代码
├── src                 
│   ├── config.js       // 配置表
│   ├── create.js       // css生成脚本
│   ├── theme.css       // 无作用，老版本备份
│   ├── tools.ts        // 主题控制库
│   └── config          // 应用配置文件
└── README.md           // 使用文档
```


---
## DEV任务分工
1. [脚本工具🦧](http://gitlab.decentest.com/liuhongyu)：负责主题控制库、生成脚本、打包脚本编写
2. [配置🦧](http://gitlab.decentest.com/wuhongjun)：负责与编码调试猿、UI协调完善配置表(src/config.js)
3. [编码调试🦧](http://gitlab.decentest.com/wanglinghui)：负责调试主题功能，测试和收集所遇到的视觉差异、技术难点
4. [UI](http://gitlab.decentest.com/lixue)：提供包含高保真低文件大小的图片等材料、示例如黑白色样式、配合完善配置表