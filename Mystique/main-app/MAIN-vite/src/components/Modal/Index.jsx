import React from "react";
import classnames from "classnames";

import { ReactComponent as CloseIcon } from "../../assets/icons/modal_close.svg";
import styles from "./style.module.less";

const Modal = (props) => {
  const { title, className, children, onClose, headChild } = props;
  return (
    <div className={classnames(styles.modalRoot, className)}>
      <div className={styles.header}>
        <div className={styles.title}>{title}</div>
        <CloseIcon
          className={styles.close}
          onClick={() => {
            if (onClose) onClose();
          }}
        />
        <div className={styles.headChild}>{headChild}</div>
      </div>
      <div className={styles.content}>{children}</div>
    </div>
  );
};

export default Modal;
