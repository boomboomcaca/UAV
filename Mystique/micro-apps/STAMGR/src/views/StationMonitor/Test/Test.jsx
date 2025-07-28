import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Bubble from '@/components/Bubble';
import styles from './index.module.less';

const Test = (props) => {
  const { className } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <Bubble>
        <div style={{ marginTop: 24 }}>???</div>
      </Bubble>
      <Bubble className={styles.bubble1} baseColor="#FF0000">
        <div style={{ marginTop: 24 }}>???</div>
      </Bubble>
      <Bubble className={styles.bubble1} baseColor="#FF00FF">
        <div style={{ marginTop: 24 }}>???</div>
      </Bubble>
      <Bubble className={styles.bubble1} baseColor="#FFFFFF">
        <div style={{ marginTop: 24 }}>???</div>
      </Bubble>
      <Bubble className={styles.bubble1} baseColor="#00FFFF">
        <div style={{ marginTop: 24 }}>???</div>
      </Bubble>
      <Bubble className={styles.bubble2} bubble={Bubble.Normal}>
        <div style={{ marginTop: 24, color: '#35E065' }}>正常</div>
      </Bubble>
      <Bubble className={styles.bubble2} bubble={Bubble.Exception}>
        <div style={{ marginTop: 24, color: '#FF4C2B' }}>异常</div>
      </Bubble>
    </div>
  );
};

Test.defaultProps = {
  className: null,
};

Test.propTypes = {
  className: PropTypes.any,
};

export default Test;
