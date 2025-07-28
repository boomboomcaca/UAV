# Mystique 项目结构说明

## 🏗️ 分层架构概述

本项目采用**5层分层架构**，按照依赖调用关系和功能职责进行组织，确保清晰的层次结构和低耦合设计。

## 📁 目录结构

```
Mystique/
├── main-app/              # 🎯 主应用层
│   └── MAIN-vite/         # React + Vite 主应用容器
├── micro-apps/            # 🔄 微前端应用层
│   ├── STAMGR/            # 状态管理应用
│   ├── spectrumcharts/    # 频谱图表应用
│   └── map/               # 地图应用
├── base-components/       # 🧱 基础组件库层
│   ├── dui/               # 基础UI组件库
│   └── searchlight-commons/ # 通用业务组件库
├── themes-tools/          # 🎨 主题工具层
│   ├── theme/             # 主题系统
│   ├── dciconfont/        # 图标字体库
│   └── gpujs/             # GPU计算工具
├── services/              # ⚙️ 服务支撑层
│   ├── MapServer/         # 地图服务
│   ├── MonitorNodeSelector/ # 监控节点选择器
│   └── QianKunHelper/     # 微前端助手
└── PROJECT-STRUCTURE.md  # 📋 项目结构说明
```

## 🔗 层级调用关系

### 调用层次 (自上而下)

1. **main-app** → 调用所有其他层
2. **micro-apps** → 调用 base-components、themes-tools、services 层
3. **base-components** → 调用 themes-tools 层
4. **themes-tools** → 独立层，被其他层调用
5. **services** → 为上层提供服务支持

### 具体依赖关系

#### 🎯 第1层：主应用层 (main-app)
- **MAIN-vite**: React 18 + Vite 4 + qiankun
- **功能**: 
  - 微前端容器管理
  - 全局路由和状态
  - 用户认证和权限
  - 子应用注册和生命周期管理

#### 🔄 第2层：微前端应用层 (micro-apps)
- **STAMGR**: 设备状态管理系统
- **spectrumcharts**: 频谱数据可视化
- **map**: 地理信息和空间数据展示
- **特点**: 独立开发、独立部署、运行时集成

#### 🧱 第3层：基础组件库层 (base-components)
- **dui**: 基础UI组件库 (Button, Modal, Input, Table等)
- **searchlight-commons**: 业务通用组件 (SegEditor, PropertyList等)
- **职责**: 提供统一的UI规范和业务组件

#### 🎨 第4层：主题工具层 (themes-tools)
- **theme**: CSS主题系统和样式变量
- **dciconfont**: 图标字体库
- **gpujs**: GPU加速计算工具
- **职责**: 样式主题、工具函数、性能优化

#### ⚙️ 第5层：服务支撑层 (services)
- **MapServer**: 地图服务后端
- **MonitorNodeSelector**: 监控节点选择逻辑
- **QianKunHelper**: 微前端集成助手
- **职责**: 底层服务和工具支持

## 🚀 开发流程

### 推荐开发顺序：

1. **第4层** (themes-tools) → 建立样式基础
2. **第3层** (base-components) → 构建组件库
3. **第5层** (services) → 配置服务支持
4. **第2层** (micro-apps) → 开发业务应用
5. **第1层** (main-app) → 集成主应用

### 构建部署顺序：

```bash
# 1. 构建主题工具层
cd themes-tools/theme && npm run build
cd ../dciconfont && npm run build
cd ../gpujs && npm run build

# 2. 构建基础组件库层
cd ../../base-components/dui && npm run build
cd ../searchlight-commons && npm run build

# 3. 构建服务支撑层
cd ../../services/MonitorNodeSelector && npm run build
cd ../QianKunHelper && npm run build

# 4. 构建微前端应用层
cd ../../micro-apps/STAMGR && npm run build
cd ../spectrumcharts && npm run build
cd ../map && npm run build

# 5. 构建主应用
cd ../../main-app/MAIN-vite && npm run build
```

## 📋 架构优势

1. **清晰的分层结构**: 按照依赖关系和功能职责分层
2. **低耦合高内聚**: 每层职责单一，依赖关系清晰
3. **便于团队协作**: 不同团队可负责不同层的开发
4. **支持渐进式开发**: 可按层逐步开发和部署
5. **易于维护扩展**: 新功能按层次添加，不影响现有结构
6. **测试友好**: 各层可独立测试，提高代码质量

## 🔧 技术栈总览

| 层级 | 主要技术栈 | 构建工具 |
|------|------------|----------|
| main-app | React 18, Vite 4, qiankun | Vite |
| micro-apps | React, Vite, 各种图表库 | Vite/Rollup |
| base-components | React, Less, TypeScript | Rollup |
| themes-tools | CSS, TypeScript, GPU.js | Rollup |
| services | Node.js, Express | Webpack/Rollup |

这种分层架构确保了项目的可维护性、可扩展性和团队协作效率！ 