/* eslint-disable import/no-unresolved */
// 用户1编写区域

exports.edge = require('./edge/edgeController');

exports.channelController = require('./channel/channelController');

// 系统权限
exports.sys_user = require('./sys/user');
exports.sys_permission = require('./sys/permission');
exports.sys_role = require('./sys/role');
exports.sys_role_permission = require('./sys/rolePermission');
exports.sys_user_role = require('./sys/userRole');
exports.sys_dic = require('./sys/dictionary');
exports.sys_dic_list = require('./sys/dictionaryList');
exports.sys_group = require('./sys/group');
exports.sys_user_group = require('./sys/userGroup');
exports.sys_role_group = require('./sys/roleGroup');
exports.sys_permission_module = require('./sys/permissionModule');

// 数据权限测试
exports.test_device = require('./test/testDevice');
exports.test_project = require('./test/testProject');

// 站点 设备 功能管理
// 是否可以直接用反射直接赋值
exports.rmbt_edge = require('./rmbt/edge');
exports.rmbt_device = require('./rmbt/device');
exports.rmbt_template = require('./rmbt/template');
exports.rmbt_plan = require('./rmbt/plan');

exports.rmbt_planning_business = require('./rmbt/planningBusiness');
exports.rmbt_planning_segment_type = require('./rmbt/planningSegmentType');
exports.rmbt_planning_segment = require('./rmbt/planningSegment');
exports.rmbt_channel_division = require('./rmbt/channelDivision');

exports.rmbt_resource_threshold = require('./rmbt/resourceThreshold');
exports.rmbt_resource_alarm = require('./rmbt/resourceAlarm');

// 新信号模板
exports.rmbt_new_signal_template = require('./rmbt/newSignalTemplate');
exports.rmbt_new_signal_data = require('./rmbt/newSignalData');
exports.rmbt_new_signal_ignore = require('./rmbt/newSignalIgnore');

// 设备状态管理、
exports.log_device_status = require('./log/logDeviceStatus');
exports.log_edge_status = require('./log/logEdgeStatus');
exports.log_edge_track = require('./log/logEdgeTrack');
exports.log_plan_running = require('./log/logPlanRunning');
exports.log_envi_info = require('./log/logEnviInfo');
exports.log_remote_control = require('./log/logRemoteControl');
exports.log_file_info = require('./log/logFileInfo');
exports.log_task_info = require('./log/logTaskInfo');
exports.task_controller = require('./task/taskController');
exports.log_business = require('./log/logBusiness');

// 模板
exports.template_frequency = require('./template/frequencyTemplate');
exports.template_threshold = require('./template/thresholdTemplate');

// 频段扫描信号
exports.rmbt_scan_signal = require('./rmbt/scanSignal');

// 考试保障
exports.rmbt_exam_signal_template = require('./exam/examSignalTemplate');
exports.rmbt_exam_signal_data = require('./exam/examSignalData');
exports.rmbt_exam_signal_ignore = require('./exam/examSignalIgnore');

// 系统运行时长
exports.sys_runtime = require('./sys/runtime');
// 授权
exports.license = require('./license/licenseOp');
// 无人机信息
exports.uav = require('./rmbt/uav');
