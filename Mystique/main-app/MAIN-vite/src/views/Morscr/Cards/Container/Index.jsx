import React from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";

const CardContainer = (props) => {
  const { children, title, style, fit } = props;

  return (
    <div className={styles.conRoot} style={style}>
      <div className={styles.titlecon}>
        <div className={styles.title}>{title}</div>
      </div>
      <div className={styles.child} style={{ top: fit ? 0 : "40px" }}>
        {children}
      </div>
      <div className={styles.mask} />
    </div>
  );
};

CardContainer.defaultProps = {
  fit: false,
};

CardContainer.propTypes = {
  fit: PropTypes.bool,
};

export default CardContainer;
