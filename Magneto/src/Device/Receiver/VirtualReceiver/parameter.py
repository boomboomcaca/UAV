# 启停任务/功能
TASK_JSON = {
    "method": "task",
    "params": {
        "operation": "on",  # on——开启任务；off——关闭任务
        "feature":
        "ssoa"  # 单频测量-ffm；单频测向-ffdf；频段扫描/新信号截获-scan；单车场强定位-ssoa；多站任务-multi
    }
}
# 边缘端——设置参数
SET_PARAMETER_JSON = {
    "method": "setParameter",
    "params": {
        # 中心频率
        "frequency": 2017000000,
        # 中频带宽
        "bandwidth": 500000,
        # 开始频率
        "start_freq": 108300000,
        # 截止频率
        "stop_freq": 125500000,
        # 截止频率
        "step": 25000,
        # 电平门限
        "level_threshold": 40,
        # 质量门限
        "quality_threshold": 20,
    }
}
# 自动测试用例——设置虚拟设备调用的信号环境文件
SINGLE_DATA_JSON = {
    "method": "setData",
    "params": {
        "path": "E:\test\Cloud_Interface\anglog\doc\single.json"
    }
}
SERVER_JSON = {
    "method": "server",
    "params": {
        "ip": None,  # ip地址
        "port": None  # 端口号
    }
}