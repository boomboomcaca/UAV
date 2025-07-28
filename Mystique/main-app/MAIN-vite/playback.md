# 1.频谱取证数据信息

```json
{
  // 数据文件名称 频谱数据只存储一帧统计数据，无此字段信息
  "fileName": null,
  // 数据类型
  "fileType": 3,
  // 文件路径 频谱数据只存储一帧统计数据，无此字段信息
  "filePath": null,
  // 存储时间 ISO8601
  "createdAt": "2023-04-19T15:51:16.413649Z",
  // 数据时长
  "duration": "00:00:00",
  // 最后一次更新时间
  "upDatedAt": "2023-04-19T15:51:16.41365Z",
  // 频段信息
  "segments": [
    {
      "startFrequency": 88,
      "stopFrequency": 108,
      "stepFrequency": 25
    },
    {
      "startFrequency": 88,
      "stopFrequency": 108,
      "stepFrequency": 25
    }
  ],
  // 频谱统计数据（多段需拼接）
  "data": [-1, 0, -2, 55, 45, "..."]
}
```

# 2. 光电取证数据信息

```json
{
  // 数据文件名称
  "fileName": "xxxxx.mp4",
  // 数据类型
  "fileType": 2,
  // 文件路径
  "filePath": "/videos",
  // 存储时间 ISO8601
  "createdAt": "2023-04-19T15:51:16.413649Z",
  // 数据时长
  "duration": "00:00:00",
  // 最后一次更新时间
  "upDatedAt": "2023-04-19T15:51:16.41365Z",
  // 频段信息 光电无此数据信息
  "segments": null,
  // 频谱统计数据（多段需拼接成完整帧） 光电无此数据信息
  "data": null
}
```

# 3.频谱数据以文件形式存在

> 注意：服务端需考虑如何压缩帧率和数据拼接

### 3.1 请求到的频谱数据回放信息

```json
{
  // 数据文件名称
  "fileName": "xxxx.dat",
  // 数据类型
  "fileType": 3,
  // 文件路径 频谱数据只存储一帧统计数据，无此字段信息
  "filePath": null,
  // 存储时间 ISO8601
  "createdAt": "2023-04-19T15:51:16.413649Z",
  // 数据时长
  "duration": "00:00:00",
  // 最后一次更新时间
  "upDatedAt": "2023-04-19T15:51:16.41365Z",
  // 频段信息
  "segments": [
    {
      "startFrequency": 88,
      "stopFrequency": 108,
      "stepFrequency": 25
    },
    {
      "startFrequency": 88,
      "stopFrequency": 108,
      "stepFrequency": 25
    }
  ],
  // 数据帧数
  "frames": 10
}
```

### 3.2 客户端轮询请求数据

客户端美间隔一定时间(暂定 1s)轮询每帧数据

- 前端请求单帧频谱数据参数

```json
{
  // 回放文件名
  "fileName": "xxxxxx.dat",
  // 当前数据帧索引
  "frameIndex": 0
}
```

- api 请求参考如下：

```javascript
getFrame("/spectrum?fileName=xxxxxx.dat&frameIndex=0");
```

- 每次查询返回数据信息（多段需每段单独拼接成完整帧，数据单位 dBμV,数据为正常值）：

```json
{
  "result": "ok",
  "data": [
    [0, 1, 5, 7, 4, "..."],
    [57, 0, 4, 4, 2, "..."]
  ]
}
```

> 备注

- 20230419 讨论决定采用 【3】 进行频谱数据存储和轮询
