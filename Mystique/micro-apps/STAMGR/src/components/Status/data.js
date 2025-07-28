import BusyStationPng from '@/assets/images/station-busy.png';
import DisabledStationPng from '@/assets/images/station-disabled.png';
import FaultStationPng from '@/assets/images/station-fault.png';
import NormalStationPng from '@/assets/images/station-normal.png';
import OfflineStationPng from '@/assets/images/station-offline.png';
import UnknownStationPng from '@/assets/images/station-unknown.png';

// TODO 不要写字典数据
const status = [
  { key: 'unknown', tag: '未知', png: UnknownStationPng },
  { key: 'disabled', tag: '禁用', png: DisabledStationPng },
  { key: 'idle', tag: '空闲', png: NormalStationPng, visible: false },
  { key: 'online', tag: '在线', png: NormalStationPng },
  { key: 'busy', tag: '忙碌', png: BusyStationPng, visible: false },
  { key: 'fault', tag: '故障', png: FaultStationPng },
  { key: 'offline', tag: '离线', png: OfflineStationPng },
];

export default status;
