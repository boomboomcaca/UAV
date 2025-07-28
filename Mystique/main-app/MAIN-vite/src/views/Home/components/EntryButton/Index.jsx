import React from "react";
// import { ReactComponent as Background } from "../../icons/entry_button_backgroung.svg";
import { useHistory, useLocation } from "react-router-dom";
import styles from "./style.module.less";
import { useCallback } from "react";

const EntryButton = (props) => {
  const history = useHistory();
  const location = useLocation();
  const { children, title, hash, key, openNew } = props;

  const itemClick = useCallback(() => {
    console.log("locationlocationlocationlocation:::", location);
    if (hash) {
      if (openNew) {
        const locationInfo = window.location.href.split("#");

        window.open(`${locationInfo[0]}#${hash}`, "_blank");
        // 20230512 electron下不行
        // window.open(`${window.location.origin}/#${hash}`, "_blank");

        // const href = document.createElement("a");
        // href.href = `${locationInfo[0]}#${hash}`;
        // console.log("open new :::", hash);
        // href.target = "_blank";
        // document.body.appendChild(href);
        // href.click();
        // setTimeout(() => {
        //   document.body.removeChild(href);
        // }, 500);
      } else {
        history.push(hash);
      }
    }
  }, [hash, openNew]);

  return (
    // <Link className={styles.link} to={hash}>
    <div key={key} className={styles.entrybtnRoot} onClick={itemClick}>
      <div className={styles.btncon}>
        <div className={styles.iconcon}>
          <div className={styles.icon}>{children}</div>
        </div>
      </div>
      <div>{title}</div>
    </div>
    // </Link>
  );
};

export default EntryButton;
