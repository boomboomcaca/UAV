/*
 * @Author: wangXueDong
 * @Date: 2022-06-22 16:59:47
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-08-04 15:58:35
 */
const testData = [
  {
    caption: '开路电视',
    items: undefined,
    remark:
      '电视信号带宽为8MHz,图像信号带宽7.25MHz，伴音载频比图像高6.5MHz。模拟电视信号采用PAL、NTSC、SECAM制式，数字电视采用DTMB和DVB-C制式。',
    id: 4,
  },
  {
    caption: '调频广播',
    items: [
      {
        value: 88,
        name: '',
        display: '88MHz',
      },
      {
        value: 89,
        name: '',
        display: '89MHz',
      },
      {
        value: 90,
        name: '',
        display: '90MHz',
      },
      {
        value: 95,
        name: '',
        display: '95MHz',
      },
      {
        value: 100,
        name: '',
        display: '100MHz',
      },
      {
        value: 101,
        name: '',
        display: '101MHz',
      },
      {
        value: 105,
        name: '',
        display: '105MHz',
      },
    ],
    remark:
      'FM广播以调频方式进行音频传输，信号频率跟随音频变化。范围在87~108MHz之间。信道带宽为200kHz，信道间隔为100kHz。',
    id: 2,
  },
  {
    caption: '民用航空',
    items: undefined,
    remark:
      '民用航空通讯专用频段，包括航空固定业务、航空移动业务和航空广播业务。主要使用108~137MHz和235~328.6MHz这两个频段。',
    id: 6,
  },
  {
    caption: '对讲机',
    items: undefined,
    remark:
      '对讲机在不需要任何网络支持的情况下就可以通话，适用于相对固定且频繁通话的场合。对讲机分为：模拟对讲机、数字对讲机、IP对讲机。',
    id: 3,
  },
  {
    caption: '集群系统',
    items: undefined,
    remark:
      '集群系统是指具有一组无线电信道，供多个部门共用的专用调度系统。该系统使用400MHz频段和800MHz频段，具有接续时间短、频率利用率高等特点。',
    id: 5,
  },
  {
    caption: 'WLAN',
    items: undefined,
    remark:
      'WLAN是指应用无线通信技术将计算机设备互联起来，构成可以互相通信和实现资源共享的网络体系。频率范围为2400~2483.5MHz。',
    id: 7,
  },
];
export default testData;
