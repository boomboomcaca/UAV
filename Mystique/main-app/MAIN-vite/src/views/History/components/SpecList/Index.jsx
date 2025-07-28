import React, { useEffect, useState } from "react";
import { Loading, Empty } from "dui";

import styles from "./style.module.less";

const SpecList = (props) => {
  const [loading, setLoading] = useState(true);
  const [dataList, setDataList] = useState();
  useEffect(() => {
    setTimeout(() => {
      setLoading(false);
    }, 2000);
  }, []);
  return (
    <div className={styles.root}>
      <div className={styles.loading}>
        {!loading && !dataList && <Empty emptype={Empty.UAV} />}
        {loading && <Loading loadingMsg="数据加载中..." />}
      </div>
      {dataList && <div className={styles.content}>datas</div>}
    </div>
  );
};

export default SpecList;
