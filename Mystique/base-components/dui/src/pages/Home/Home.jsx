import React, { useEffect, useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Resizable } from 're-resizable';

import components from '@/components/demo';
import TabButton from '../../components/TabButton';
import { getDefaultDemoKey, saveDefaultDemoKey } from '@/lib/tools';

import styles from './styles.module.less';
import 'github-markdown-css';

export default () => {
  // 管理开始时临时的demo页面key
  const defaultValue = getDefaultDemoKey() || 'Example';
  const [demoKey, saveDemoKey] = useState(defaultValue);

  const [document, setdocument] = useState();
  const [democode, setdemocode] = useState();

  const [docKey, setDocKey] = useState({ key: 'document', value: 'Document' });
  const c = Object.entries(components).map((i) => {
    const [name, demo] = i;
    return { name, demo };
  });

  /**
   * 切换example
   *
   * @param {*} e
   */
  const onClick = (e) => {
    const key = e?.target?.innerText;
    if (key) {
      saveDemoKey(key);
      saveDefaultDemoKey(key);
    }
  };

  useEffect(() => {
    if (demoKey) {
      // TODO 拉取文件
      fetch(`http://192.168.102.167:8586/components/${demoKey}/README.md`)
        .then((response) => {
          if (response.ok) {
            return response.text();
          }
          return '';
        })
        .then((text) => setdocument(text));

      fetch(`http://192.168.102.167:8586/components/${demoKey}/Demo/Demo.jsx`)
        .then((response) => {
          if (response.ok) {
            return response.text();
          }
          return '';
        })
        .then((text) => setdemocode(`\`\`\`\`JavaScript${text}\`\`\`\``));
    }
  }, [demoKey]);

  // 计算出demo
  const Demo = c.find((i) => i.name === demoKey)?.demo;

  return (
    <div className={styles.home}>
      <div className={styles.tabs}>
        <div className={styles.tips}>组件列表：</div>
        <div className={styles.item}>
          {Object.keys(components).map((i) => (
            <div value={i} key={i} className={i === demoKey ? styles.radioActive : styles.radio} onClick={onClick}>
              {i}
            </div>
          ))}
        </div>
      </div>
      <div className={styles.demo}>
        <Resizable minWidth="30%" maxWidth="90%" defaultSize={{ width: '60%' }}>
          <div className={styles.demoview}>
            <Demo />
          </div>
        </Resizable>
        <div className={styles.demodoc}>
          <TabButton
            state={docKey}
            onChange={(d) => setDocKey(d)}
            option={[
              { key: 'document', value: 'Document' },
              { key: 'democode', value: 'Demo code' },
            ]}
          />
          {docKey.key === 'document' && document && (
            <ReactMarkdown className="markdown-body" children={document} remarkPlugins={[remarkGfm]} />
          )}
          {docKey.key === 'democode' && democode && (
            <ReactMarkdown className="markdown-body" children={democode} remarkPlugins={[remarkGfm]} />
          )}
        </div>
      </div>
    </div>
  );
};
