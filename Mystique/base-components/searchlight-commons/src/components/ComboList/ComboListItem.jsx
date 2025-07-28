import React, { useRef } from 'react';
import PropTypes from 'prop-types';
import icons, { info, colors1, colors2 } from './icons.jsx';
import styles from './item.module.less';

const ComboListItem = (props) => {
  const { item } = props;
  const divh = useRef(null);
  return (
    <div
      className={styles.dropdownItem}
      onClick={(e) => {
        e.stopPropagation();
      }}
    >
      <div className={styles.icon}>{icons[item.type] || info}</div>
      <div
        className={styles.plate2}
        onMouseEnter={() => {
          if (divh.current) divh.current.style.color = colors2[item.type] || colors2.info;
        }}
        onMouseLeave={() => {
          if (divh.current) divh.current.style.color = colors1[item.type] || colors1.info;
        }}
      >
        <div ref={divh} className={styles.content} style={{ color: colors1[item.type] || colors1.info }}>
          {item.msg}
        </div>
      </div>
    </div>
  );
};

ComboListItem.defaultProps = {
  item: null,
};

ComboListItem.propTypes = {
  item: PropTypes.any,
};

export default ComboListItem;
