import React, { useState, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import step1Png from './assets/step1-icon.png';
import step2Png from './assets/step2-icon.png';
import step3Png from './assets/step3-icon.png';
import step4Png from './assets/step4-icon.png';
import step5Png from './assets/step5-icon.png';
import stepBgPng from './assets/step-bg.png';
import styles from './StepTabs.module.less';

const STEPIMGS = {
  step1Png,
  step2Png,
  step3Png,
  step4Png,
  step5Png,
};
const StepTabPane = (props) => {
  const { className, style, tab, title, describe, imgBox, hideMenu, children } = props;
  const [showTitle, setShowTitle] = useState(hideMenu);
  useMemo(() => {
    setShowTitle(hideMenu);
  }, [hideMenu]);
  return (
    <div className={classnames(styles.tabPane, className)} style={style}>
      {showTitle ? (
        <div className={styles.tabPane_title}>
          <div className={styles.title_des}>{describe}</div>
          <div className={styles.title_wrap}>
            <div className={styles.wrap_optionL}>
              {STEPIMGS[`step${tab + 1}Png`] ? <img alt="" src={STEPIMGS[`step${tab + 1}Png`]} /> : ''}
            </div>
            <div className={styles.wrap_optionR}>
              <img alt="" src={stepBgPng} />
              <div className={styles.optionR_info}>
                {imgBox || ''}
                <div className={styles.optionR_info_txt}>{title || ''}</div>
              </div>
            </div>
          </div>
        </div>
      ) : (
        ''
      )}
      <div className={styles.tabPane_content}>{children}</div>
    </div>
  );
};
StepTabPane.defaultProps = {
  className: '',
  style: {},
  tab: 0,
  title: '',
  describe: '',
  imgBox: null,
  hideMenu: false,
  children: null,
};

StepTabPane.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  tab: PropTypes.number,
  title: PropTypes.string,
  describe: PropTypes.string,
  imgBox: PropTypes.any,
  hideMenu: PropTypes.bool,
  children: PropTypes.any,
};

export default StepTabPane;
