import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { CSSTransition } from 'react-transition-group';
import icons from './icons.jsx';
import styles from './index.module.less';
import './animate.less';

const Item = (props) => {
  const { item } = props;
  return (
    <CSSTransition in={item?.active} timeout={300} classNames="rtgmsg-fade" unmountOnExit appear>
      <div className={classnames(styles.item, styles[`item_${item?.type}`])}>
        <div className={styles.icon}>{icons[item?.type]}</div>
        {item.icon ? <div style={{ paddingTop: '37px' }}>{item.icon}</div> : null}
        <div className={styles.msg}>{item?.msg}</div>
      </div>
    </CSSTransition>
  );
};

Item.defaultProps = {
  item: null,
};

Item.propTypes = {
  item: PropTypes.any,
};

export default Item;
