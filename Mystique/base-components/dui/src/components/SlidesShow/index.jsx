/* eslint-disable no-undef */
import React, { useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './style.module.less';

/**
 * 轮播图
 */
const SlidesShow = (props) => {
  const { imgList, recall, wrapClassName, interval } = props;
  const imgClass = ['cubeRandom', 'block', 'cubeStop', 'horizontal', 'showBars'];
  const boxDomRef = useRef(null);
  useEffect(() => {
    let resizeTimer = null;
    const resizeBanner = () => {
      const slidesShowDom = document.getElementsByClassName('slidesShow')[0];
      $('.box_skitter_large')
        .css({ width: slidesShowDom.clientWidth, height: slidesShowDom.clientHeight })
        .skitter({
          label: false,
          thumb_width: slidesShowDom.clientWidth,
          thumb_height: slidesShowDom.clientHeight,
          numbers: false,
          interval: 5000,
          hideTools: true,
          navigation: false,
          show_randomly: true,
          onLoad: () => {
            console.log('boxDomRef', boxDomRef.current);
            $('.box_skitter img').css({ width: slidesShowDom.clientWidth, height: slidesShowDom.clientHeight });
            $('.box_skitter image img').css({ width: slidesShowDom.clientWidth, height: slidesShowDom.clientHeight });
            $('.container_skitter .box_clone img').css({
              width: slidesShowDom.clientWidth,
              height: slidesShowDom.clientHeight,
            });
            $('.box_skitter .preview_slide ul li img').css({
              width: slidesShowDom.clientWidth,
              height: slidesShowDom.clientHeight,
            });
          },
        });
    };
    const resizeFunc = () => {
      // 生成图表
      if (resizeTimer) {
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }
      resizeTimer = setTimeout(() => {
        resizeBanner();
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }, 300);
    };
    $(document).ready(function () {
      resizeBanner();
      window.addEventListener('resize', resizeFunc, false);
    });
    return () => {
      window.removeEventListener('resize', resizeFunc, false);
    };
  }, []);
  return (
    <div className={classnames('slidesShow', styles.container, wrapClassName)}>
      <div className="box_skitter box_skitter_large" ref={boxDomRef}>
        <ul>
          {imgList.map((item, index) => (
            <li key={`SwiperSlide-${index + 1}`}>
              <img alt="swiper-img" src={item || ''} className={imgClass[Math.abs(imgClass.length - index)]} />
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
SlidesShow.defaultProps = {
  imgList: [],
  recall: null,
  interval: 5000,
  wrapClassName: '',
};

SlidesShow.propTypes = {
  imgList: PropTypes.array,
  recall: PropTypes.func,
  interval: PropTypes.number,
  wrapClassName: PropTypes.any,
};
export default SlidesShow;
