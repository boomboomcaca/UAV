import React from "react";
import { useHistory } from "react-router-dom";
import Header from "../Header/Index.jsx";
import styles from "./style.module.less";

const Template = (props) => {
  const history = useHistory();
  const { title, children } = props;
  return (
    <div className={styles.root}>
      <Header
        title={title}
        onBack={() => {
          history.goBack();
        }}
      />
      <div className={styles.content}>
        <div className={styles.content1}>{children}</div>
      </div>
    </div>
  );
};

export default Template;
