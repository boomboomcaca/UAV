import React, { useState } from 'react';
import SlidesShow from '../index';
import Select from '../../Select/index';
import styles from './Demo.module.less';
import banner1png from '../images/1.png';
import banner2png from '../images/2.png';
import banner3png from '../images/3.png';
import banner4png from '../images/4.png';
import banner5png from '../images/5.png';
import banner6png from '../images/6.png';
import banner7png from '../images/7.png';
import banner8png from '../images/8.png';
import banner9png from '../images/9.png';
import banner10png from '../images/10.png';

const { Option } = Select;

export default function Demo() {
  const [pattern, setPattern] = useState('effect');
  const imgList = [
    // 'https://swiperjs.com/demos/images/nature-1.jpg',
    // 'https://swiperjs.com/demos/images/nature-2.jpg',
    // 'https://swiperjs.com/demos/images/nature-3.jpg',
    // 'https://swiperjs.com/demos/images/nature-4.jpg',
    // 'https://swiperjs.com/demos/images/nature-5.jpg',
    // 'https://swiperjs.com/demos/images/nature-6.jpg',
    banner1png,
    banner2png,
    banner3png,
    banner4png,
    banner5png,
    banner6png,
    banner7png,
    banner8png,
    banner9png,
    banner10png,
  ];
  // 设置为"fade"（淡入）"cube"（方块）"coverflow"（3d流）"flip"（3d翻转）
  const options = [
    {
      label: '常规',
      value: 'normal',
    },
    {
      label: '动效',
      value: 'effect',
    },
  ];
  return (
    <div className={styles.contianer}>
      {/* <Select value={pattern} onChange={(val) => setPattern(val)}>
        {options.map((item) => (
          <Option value={item.value} key={item.value}>
            {item.label}
          </Option>
        ))}
      </Select> */}
      <div className={styles.contianer_slider}>
        <SlidesShow loop interval={2000} pattern={pattern} imgList={imgList} />
      </div>
    </div>
  );
}
