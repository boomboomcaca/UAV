/**
 * @typedef {Object} Module
 * @property {String} id 模块id
 * @property {String} deviceId 模块id
 * @property {String} displayName 模块id
 * @property {String} moduleType 模块类型
 * @property {Array<String>} moduleCategory 模块分类
 * @property {String} moduleState 设备列表
 */

/**
 * @typedef {Object} SpectrumMonitorProps
 * @property {boolean} maxView 是否为最大显示
 * @property {Boolean} resizeChart 是否需要调整chart尺寸
 * @property {String} taskStatus 任务状态
 * @property {any} curFeature 当前功能
 * @property {Array<Module>} devices 设备列表
 * @property {Function} onMaxView
 */

/**
 * @typedef {Object} VideoMonitorProps
 * @property {boolean} maxView 是否为最大显示
 * @property {String} taskStatus 任务状态
 * @property {Array<Module>} devices 设备列表
 * @property {Function} onMaxView
 */

/**
 * @typedef {Object} RadarMonitorProps
 * @property {boolean} showList 是否为最大显示
 * @property {String} taskStatus 任务状态
 * @property {Array<Module>} devices 设备列表
 * @property {Function} onMaxView
 */

/**
 * @typedef {Object} SpectrumControlProps
 * @property {String} taskStatus 任务状态
 * @property {Array<Module>} devices 设备列表
 * @property {Function} onMaxView
 */

/**
 *
 */
