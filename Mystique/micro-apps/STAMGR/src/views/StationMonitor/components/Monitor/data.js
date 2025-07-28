export const testMain = {
  type: 'switchState',
  moduleID: 'ad',
  name: 'main',
  display: '开关',
  state: 'on',
  info: [
    {
      name: 'voltage',
      display: '电压',
      unit: 'V',
      value: 220.5,
      message: '',
    },
    {
      name: 'current',
      display: '电流',
      unit: 'A',
      value: 1.2,
      message: '',
    },
  ],
};

export const testPower = [
  {
    type: 'switchState',
    moduleID: 'adfasdafadsf',
    name: 'switch3',
    display: '开关3',
    state: 'on',
    info: [
      {
        name: 'voltage',
        display: '电压',
        unit: 'V',
        value: 220.5,
        message: '',
      },
      {
        name: 'current',
        display: '电流',
        unit: 'A',
        value: 1.2,
        message: '',
      },
    ],
  },
  {
    type: 'switchState',
    moduleID: 'adfasdafadsf',
    name: 'switch4',
    display: '开关4',
    state: 'on',
    info: [
      {
        name: 'voltage',
        display: '电压',
        unit: 'V',
        value: 220.4,
        message: '',
      },
      {
        name: 'current',
        display: '电流',
        unit: 'A',
        value: 1.0,
        message: '',
      },
    ],
  },
];

export const testSensor = [
  // {
  //   name: 'temperature1',
  //   display: '顶部温度',
  //   unit: '℃',
  //   value: 45.5,
  //   message: '',
  // },
  // {
  //   name: 'temperature2',
  //   display: '底部温度',
  //   unit: '℃',
  //   value: 38.5,
  //   message: '',
  // },
  // {
  //   name: 'temperature3',
  //   display: '中部温度',
  //   unit: '℃',
  //   value: 40.5,
  //   message: '',
  // },
  {
    name: 'temperature',
    display: '温度',
    unit: '℃',
    // value: 40.5,
    message: '',
  },
  {
    name: 'humidity',
    display: '湿度',
    unit: '%',
    // value: 67,
    message: '',
  },
  {
    name: 'airPressure',
    display: '气压',
    unit: 'hPa',
    // value: 1013.25,
    message: '',
  },
];

export const testAlarm = [
  {
    name: 'acs',
    display: '门禁告警',
    unit: '',
    // value: true,
    message: '警报详细信息',
  },
  {
    name: 'smoke',
    display: '烟雾报警',
    unit: '',
    // value: true,
    message: '警报详细信息',
  },
  {
    name: 'fire',
    display: '进水告警',
    unit: '',
    // value: true,
    message: '警报详细信息',
  },
  // {
  //   name: 'fire',
  //   display: '火灾报警',
  //   unit: '',
  //   // value: true,
  //   message: '警报详细信息',
  // },
];

export const testCamera = [
  { name: '区域1', url: '' },
  { name: '区域2', url: '' },
  { name: '区域3', url: '' },
  { name: '区域4', url: '' },
  { name: '区域5', url: '' },
];
