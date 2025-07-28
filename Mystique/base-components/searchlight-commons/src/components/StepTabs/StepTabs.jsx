import React, { useEffect, useState, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import step1Png from './assets/step1-icon.png';
import step2Png from './assets/step2-icon.png';
import step3Png from './assets/step3-icon.png';
import step4Png from './assets/step4-icon.png';
import step5Png from './assets/step5-icon.png';
import stepBgPng from './assets/step-bg.png';
import emptyPng from './assets/empty-icon.png';
import styles from './StepTabs.module.less';

const STEPIMGS = {
  step1Png,
  step2Png,
  step3Png,
  step4Png,
  step5Png,
};
const StepTabs = (props) => {
  const { currentTabNum, children, className, onTabChange, hideMenu } = props;
  const [activePaneNum, setActivePaneNum] = useState(currentTabNum);
  const [tabPanes, setTabPanes] = useState([]);
  const [isShowMenu, setIsShowMenu] = useState(hideMenu);
  const hasChildrenRef = useRef([]);
  useEffect(() => {
    // 步骤对应的标题和图标
    const hasChildren = children && Array.isArray(children) ? children : [children];
    hasChildrenRef.current = hasChildren;
    // console.log('stepTabPanes--->hasChildren', hasChildren);
    if (children && hasChildren.length > 0) {
      const stepTabPanes = [];
      for (let i = 0; i < hasChildren.length; i += 1) {
        const { title, describe, imgBox } = hasChildren[i].props;
        stepTabPanes.push({ title, describe, imgBox });
      }
      // console.log('stepTabPanes--->', stepTabPanes);
      setTabPanes([...stepTabPanes]);
    }
    return () => {};
  }, []);
  const checkTabPane = (index) => {
    // 切换tab项
    if (index !== activePaneNum) {
      setActivePaneNum(index);
      onTabChange(index);
    }
  };
  useMemo(() => {
    setActivePaneNum(currentTabNum);
  }, [currentTabNum]);
  useMemo(() => {
    setIsShowMenu(hideMenu);
  }, [hideMenu]);
  return (
    <div className={classnames(styles.tabBox, className)}>
      <div className={styles.tabBox_content}>
        <div
          className={!isShowMenu ? styles.content_check : [styles.content_check, styles.content_check_hide].join(' ')}
        >
          {tabPanes.slice(0, 5).map((item, index) => (
            <div
              key={`tabPanes-title-${index + 1}`}
              className={[styles.check_option, activePaneNum === index ? styles.check_option_check : ''].join(' ')}
              onClick={() => checkTabPane(index)}
            >
              <div className={styles.check_optionL}>
                <img alt="" src={STEPIMGS[`step${index + 1}Png`]} />
              </div>
              <div className={styles.check_optionR}>
                <img alt="" src={stepBgPng} />
                <div className={styles.optionR_info}>
                  {item.imgBox || ''}
                  <div className={styles.optionR_info_txt}>{item.title || ''}</div>
                </div>
              </div>
              <div className={styles.check_tringle} />
            </div>
          ))}
        </div>
        <div className={styles.content_pane}>
          <div
            className={styles.pane_des}
            title={tabPanes[activePaneNum] && tabPanes[activePaneNum].describe ? tabPanes[activePaneNum].describe : ''}
          >
            {tabPanes[activePaneNum] && tabPanes[activePaneNum].describe ? tabPanes[activePaneNum].describe : ''}
          </div>
          <div className={styles.pane_display}>
            {children && activePaneNum > -1
              ? hasChildrenRef.current.map((child, idx) => {
                  if (child) {
                    return React.cloneElement(child, {
                      className: activePaneNum === idx ? styles.pane_display_check : '',
                      key: child.key === undefined ? child.key : `tabPane-${idx + 1}`,
                      style: {
                        left: `${(activePaneNum - idx) * 100}%`,
                      },
                      hideMenu,
                    });
                  }
                  return null;
                })
              : null}
          </div>
        </div>
      </div>
      {children && activePaneNum > -1 ? null : (
        <div
          className={styles.content_display_empty}
          style={{
            paddingTop: tabPanes.length > 0 && tabPanes.length <= 5 ? `${(tabPanes.length / 2) * 80}px` : '50%',
          }}
        >
          <img alt="" src={emptyPng} />
          <span>请在左侧选择操作</span>
        </div>
      )}
    </div>
  );
};
StepTabs.defaultProps = {
  className: null,
  currentTabNum: 0,
  children: null,
  hideMenu: false,
  onTabChange: () => {},
};

StepTabs.propTypes = {
  className: PropTypes.any,
  currentTabNum: PropTypes.number,
  children: PropTypes.any,
  hideMenu: PropTypes.bool,
  onTabChange: PropTypes.func,
};

export default StepTabs;
