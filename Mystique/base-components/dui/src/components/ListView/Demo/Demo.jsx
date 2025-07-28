import React, { useState } from 'react';
import ListView from '../index';
import styles from './index.module.less';

export default function Demo() {
  const [dataSource, setDataSource] = useState([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
  const loadMore = () => {
    if (dataSource.length < 100) {
      const arr = [...dataSource];
      for (let i = 0; i < 20; i += 1) {
        arr.push(`ik-${dataSource.length + i}`);
      }
      setTimeout(() => {
        setDataSource(arr);
      }, 200);
    }
  };

  const [dataSource2, setDataSource2] = useState([0, 1, 2, 3, 4, 5]);
  const loadMore2 = () => {
    window.console.log('??????????');
    const arr = [...dataSource2];
    for (let i = 0; i < 5; i += 1) {
      arr.push(`ik-${dataSource2.length + i}`);
    }
    setTimeout(() => {
      setDataSource2(arr);
    }, 200);
  };

  const strs = `大前端时代，最近在面试前端工程师的过程中，有感而发，技术更新迭代快，学习成本高。浏览了各大博客论坛，千差万别，比较混乱。最终决定参考
  Element UI 的设计风格，主题色选择紫色（受到 MaterialDesignInXamlToolkit 的影响），写一套基于 Vue3 的 UI
  框架库和对应的官方网站，方便后期在项目中快速使用，也算是对 Vue3 新特性的学习和总结。然而，当我们开始构建越来越大型的应用时，
  需要处理的 JavaScript 代码量也呈指数级增长。包含数千个模块的大型项目相当普遍。我们开始遇到性能瓶颈 —— 使用 JavaScript 开发
  的工具通常需要很长时间（甚至是几分钟！）才能启动开发服务器，即使使用 HMR，文件修改后的效果也需要几秒钟才能在浏览器中反映出来。
  如此循环往复，迟钝的反馈会极大地影响开发者的开发效率和幸福感。
  Vite 旨在利用生态系统中的新进展解决上述问题：浏览器开始原生支持 ES 模块，且越来越多 JavaScript 工具使用编译型语言编写。`;

  return (
    <div className={styles.root}>
      <ListView className={styles.list} baseSize={{ width: 240 }} loadMore={loadMore}>
        {dataSource.map((m) => {
          return <ListView.Item key={m} content={<div className={styles.item1}>{m}</div>} />;
        })}
      </ListView>
      <ListView.Virtualized
        className={styles.list}
        baseSize={{ width: 240 }}
        dataSource={dataSource}
        itemTemplate={(m) => <div className={styles.item1}>{m}</div>}
        loadMore={loadMore}
      />
      {/* <ListView
        virtualized
        className={styles.list}
        // baseSize={{ width: 240, height: 120 }}
        baseSize={{ width: '50%', height: 120 }}
        loadMore={loadMore}
        dataSource={dataSource}
        itemTemplate={(item) => {
          return <div className={styles.item2}>{item}</div>;
        }}
      /> */}
      <ListView.Flow className={styles.list} loadMore={loadMore2}>
        {dataSource2.map((item, idx) => {
          return (
            // eslint-disable-next-line react/no-array-index-key
            <div key={idx} className={styles.item3} /* style={{ height: Math.abs(200 * Math.sin(idx)) }} */>
              {item}
              {strs.substring(0, Math.abs(400 * Math.sin(idx)))}
            </div>
          );
        })}
      </ListView.Flow>
    </div>
  );
}
