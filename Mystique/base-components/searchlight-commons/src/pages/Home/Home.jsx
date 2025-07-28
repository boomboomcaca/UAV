import React, { useState, useEffect, useRef } from 'react';
import { useRequest } from 'ahooks';
import { message, TabButton, Radio } from 'dui';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Resizable } from 're-resizable';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { dark, vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import components from '@/components/demo';
import extensions from '@/extensions/demo';
import { getDefaultDemoKey, saveDefaultDemoKey } from '@/lib/tools';
import { setToken, getToken } from '@/api/auth';
import { login } from '@/api/cloud';
import styles from './styles.module.less';
import 'github-markdown-css';

export default () => {
  useEffect(() => {
    // 判断已登录之后，直接跳转到主页
    const tk = getToken();
    if (tk) {
      // window.console.log(tk);
    } else {
      run({ account: 'admin', password: '123456' });
    }
  }, []);

  const { run } = useRequest((user) => login(user), {
    manual: true,
    onSuccess: (ret) => {
      setToken(ret.result);
      message.success('登录成功！');
    },
    onError: (/* ret */) => {
      message.error('登录失败！');
    },
  });

  // 管理开始时临时的demo页面key
  const defaultValue = getDefaultDemoKey() || { key: 'Example', from: 'components' };
  const [demoKey, saveDemoKey] = useState(defaultValue);

  const [document, setdocument] = useState(null);
  const [democodes, setdemocodes] = useState([]);
  const [democode, setdemocode] = useState(null);
  const [democodeTxt, setdemocodeTxt] = useState(null);
  const [docKey, setDocKey] = useState({ key: 'document', value: 'Document' });
  const docKeyRef = useRef({ key: 'document', value: 'Document' });

  const c = Object.entries({ ...components, ...extensions }).map((i) => {
    const [name, demo] = i;
    return { name, demo };
  });

  /**
   * 切换example
   *
   * @param {*} e
   */
  const onClick = (e, from) => {
    const key = e?.target?.innerText;
    if (key) {
      saveDemoKey({ key, from });
      saveDefaultDemoKey({ key, from });
    }
    setdemocode(null);
  };

  // 计算出demo
  const Demo = c.find((i) => i.name === demoKey?.key)?.demo;

  useEffect(() => {
    if (demoKey) {
      const { key, from } = demoKey;
      fetch(`http://192.168.102.167:8587/${from}/${key}`)
        .then((response) => {
          if (response.ok) {
            return response.text();
          }
          return '';
        })
        .then((text) => {
          const ddd = new DOMParser().parseFromString(text, 'text/html');
          const as = ddd.querySelectorAll('a');
          let find = null;
          for (let i = 0; i < as.length; i += 1) {
            if (as[i].innerHTML.toUpperCase() === 'README.MD') {
              find = as[i].innerHTML;
              break;
            }
          }
          if (find) {
            fetch(`http://192.168.102.167:8587/${from}/${key}/${find}`)
              .then((response) => {
                if (response.ok) {
                  return response.text();
                }
                return '';
              })
              .then((text2) => {
                setdocument(text2);
              });
          } else {
            setdocument('没有相关README文件！');
          }
        });
      fetch(`http://192.168.102.167:8587/${from}/${key}/Demo`)
        .then((response) => {
          if (response.ok) {
            return response.text();
          }
          return '';
        })
        .then((text) => {
          const ddd = new DOMParser().parseFromString(text, 'text/html');
          const as = ddd.querySelectorAll('a');
          const opts = [];
          for (let i = 0; i < as.length; i += 1) {
            if (as[i].innerText.indexOf('.jsx') > -1) {
              opts.push({ value: as[i].innerText, label: as[i].innerText });
            }
          }
          setdemocodes(opts);
          if (docKeyRef.current?.key === 'democode' && opts.length > 0) {
            setdemocode(opts[0].value);
          } else {
            setdemocode(null);
          }
        });
    }
  }, [demoKey]);

  useEffect(() => {
    if (demoKey && democode) {
      const { key, from } = demoKey;
      fetch(`http://192.168.102.167:8587/${from}/${key}/Demo/${democode}`)
        .then((response) => {
          if (response.ok) {
            return response.text();
          }
          return '';
        })
        .then((text) => {
          setdemocodeTxt(`\`\`\` jsx ${text} \`\`\` `);
        });
    } else {
      setdemocodeTxt(null);
    }
  }, [demoKey, democode]);

  return (
    <div className={styles.home}>
      <div className={styles.tabs}>
        <div className={styles.tips}>组件列表：</div>
        <div className={styles.item}>
          {Object.keys(components).map((i) => (
            <div
              value={i}
              key={i}
              className={i === demoKey?.key ? styles.radioActive : styles.radio}
              onClick={(e) => {
                onClick(e, 'components');
              }}
            >
              {i}
            </div>
          ))}
          {Object.keys(extensions).map((i) => (
            <div
              value={i}
              key={i}
              className={i === demoKey?.key ? styles.radioActive : styles.radio}
              onClick={(e) => {
                onClick(e, 'extensions');
              }}
            >
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
            onChange={(d) => {
              setDocKey(d);
              docKeyRef.current = d;
              if (democodes.length > 0) {
                setdemocode(democodes[0].value);
              }
            }}
            option={[
              { key: 'document', value: 'Document' },
              { key: 'democode', value: 'Demo code' },
            ]}
          />
          <div className={styles.md}>
            {docKey.key === 'document' && document && (
              <ReactMarkdown className="markdown-body" remarkPlugins={[remarkGfm]}>
                {document}
              </ReactMarkdown>
            )}
            {docKey.key === 'democode' && democodes && (
              <div className={styles.code}>
                <Radio options={democodes} value={democode} onChange={(value) => setdemocode(value)} />
                <ReactMarkdown
                  components={{
                    code({ node, inline, className, children, ...props }) {
                      const match = /language-(\w+)/.exec(className || '');
                      return !inline && match ? (
                        <SyntaxHighlighter style={vscDarkPlus} language={match[1]} PreTag="div" {...props}>
                          {children}
                        </SyntaxHighlighter>
                      ) : (
                        <code className={className} {...props}>
                          {children}
                        </code>
                      );
                    },
                  }}
                >
                  {democodeTxt}
                </ReactMarkdown>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
