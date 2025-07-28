/* eslint-disable no-await-in-loop */
import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message, Modal, Input, Button } from 'dui';
import { del, add } from '@/api/cloud';
import Fields, { Field } from '@/components/Fileds';
import ElectButton from '@/components/ElectButton';
import useTemplate from '@/hooks/useTemplate';
import useWindowSize from '@/hooks/useWindowSize';
import { createGUID } from '@/utils/random';
import { templateUrl } from '@/api/path';
import Item from './Item.jsx';
import styles from './index.module.less';

const Template = (props) => {
  const { className } = props;

  const size = useWindowSize();

  let isMouseOver = useRef(false).current;
  const refDiv = useRef();
  const logRef = useRef([]);
  const [logx, setLogx] = useState([]);
  const addLog = (msg, type = 'info') => {
    const id = createGUID();
    window.console.log(id);
    const log = { id, msg, type };
    const lx = [...logRef.current, log];
    logRef.current = lx;
    setLogx(lx);
    setTimeout(() => {
      scrollToEnd();
    }, 20);
  };
  useEffect(() => {
    if (refDiv.current) {
      refDiv.current.onmouseenter = () => {
        isMouseOver = true;
      };
      refDiv.current.onmouseleave = () => {
        isMouseOver = false;
      };
    }
  }, [refDiv]);
  const scrollToEnd = () => {
    if (!isMouseOver) {
      refDiv.current.scrollTop = refDiv.current.scrollHeight;
    }
  };

  const fileObjs = useRef([]).current;
  const [trigger, setTrigger] = useState(false);

  useEffect(() => {
    if (trigger) {
      window.console.log(fileObjs);
      uploadFiles();
    }
  }, [trigger]);

  const uploadFiles = async () => {
    for (let i = 0; i < fileObjs.length; i += 1) {
      const tempObj = fileObjs[i];
      addLog(`${i + 1}.上传 ${tempObj.displayName} 中...`);
      const record = {
        name: tempObj.displayName,
        remark: tempObj.description,
        template: tempObj,
      };

      const d = {
        template: JSON.stringify(tempObj),
        version: tempObj.version,
        tempType: tempObj.moduleType,
        name: tempObj.displayName,
        remark: tempObj.description,
      };
      setData(d);

      try {
        const res = await add(templateUrl, record);
        window.console.log(res);

        if (res.result) {
          window.console.log(res.result);
          addLog(`上传 ${tempObj.displayName} 成功`, 'success');
        }
      } catch (error) {
        if (error) {
          window.console.log(error);
          addLog(`上传 ${tempObj.displayName} :${error}`, 'error');
        }
      }
    }
    refresh();
    setTrigger(false);
    fileObjs.splice(0, fileObjs.length);
  };

  const [type, setType] = useState('device'); // driver

  const [data, setData] = useState(null); // driver

  const { refresh, templates } = useTemplate(type, true);

  const onDeleteTemplate = (item) => {
    Modal.confirm({
      title: '模板删除',
      closable: false,
      content: (
        <div className={styles.mcont}>
          确定要删除
          <Item className={styles.mitem} item={item} closable={false} />
          吗?
        </div>
      ),
      onOk: () => {
        del(templateUrl, { id: item.id }).then((res) => {
          if (res.result) {
            message.success({ key: 'tip', content: '模板删除成功' });
            refresh();
          }
        });
      },
    });
  };

  const onFieldChange = (name, value) => {
    data[name] = value;
  };

  const onSelectFile = () => {
    addLog('');
    addLog('--------- 选择模板');
    document.getElementById('files').click();
  };

  const isRightFile = (file) => {
    return file.version && file.moduleType && file.displayName;
  };

  function fileImport() {
    const selectedFile = document.getElementById('files').files[0];
    if (selectedFile) {
      addLog('读取文件...');
      const reader = new FileReader();
      reader.readAsText(selectedFile);
      reader.onload = (e) => {
        try {
          const tempObj = JSON.parse(e.currentTarget.result);
          if (isRightFile(tempObj)) {
            const d = {
              template: e.currentTarget.result,
              version: tempObj.version,
              tempType: tempObj.moduleType,
              name: tempObj.displayName,
              remark: tempObj.description,
            };
            setData(d);
            addLog('读取文件完成');
          } else {
            addLog('文件格式错误', 'error');
          }
        } catch (error) {
          addLog('读取文件失败', 'error');
        }
      };
    } else {
      addLog('选择取消');
    }
  }

  const uploadFile = () => {
    const tp = {
      name: data.name,
      remark: data.remark,
      template: JSON.parse(data.template),
    };
    add(templateUrl, tp)
      .then((res) => {
        if (res.result) {
          message.success({ key: 'tip', content: '上传成功！' });
          addLog(`上传 ${data.name} 成功`, 'success');
          refresh();
        }
      })
      .catch((rej) => {
        if (rej) {
          window.console.log(rej);
          addLog(`上传 ${data.name} :${rej}`, 'error');
        }
      });
  };

  const onAddList = () => {
    addLog('');
    addLog('--------- 批量上传模板');
    document.getElementById('files2').click();
  };
  const fileImport2 = () => {
    const selectedFiles = document.getElementById('files2').files;
    if (selectedFiles) {
      addLog('读取文件...');
      for (let i = 0; i < selectedFiles.length; i += 1) {
        const selectedFile = selectedFiles[i];
        const reader = new FileReader();
        reader.readAsText(selectedFile);
        reader.onload = (e) => {
          try {
            const tempObj = JSON.parse(e.currentTarget.result);
            if (isRightFile(tempObj)) {
              fileObjs.push(tempObj);
              if (fileObjs.length === selectedFiles.length) {
                setTrigger(true);
                addLog('读取文件完成');
              }
            } else {
              addLog('文件格式错误', 'error');
            }
          } catch (error) {
            addLog('读取文件失败', 'error');
          }
        };
      }
    } else {
      addLog('选择取消');
    }
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.list}>
        <ElectButton
          options={[
            { key: 'device', value: '设备模板' },
            { key: 'driver', value: '功能模板' },
          ]}
          value={type}
          onChange={(e) => {
            setType(e.key);
          }}
        />
        <div className={styles.temps}>
          {templates?.map((t) => {
            return (
              <>
                <div className={styles.title}>{t.value}</div>
                {t.templates.length > 0 ? (
                  <div className={styles.subtemps}>
                    {t.templates
                      ?.sort((s1, s2) => {
                        if (s1.name === s2.name) {
                          return s1.version > s2.version ? -1 : 1;
                        }
                        return s1.name > s2.name ? -1 : 1;
                      })
                      .map((tt) => {
                        return <Item className={styles.item} item={tt} onClick={onDeleteTemplate} />;
                      })}
                  </div>
                ) : null}
              </>
            );
          })}
        </div>
      </div>
      <div className={styles.upld}>
        <div>模板上传</div>
        <div className={styles.form}>
          <Fields data={data} labelStyle={{ width: 90 }} onChange={onFieldChange}>
            <Field label="模板名称" name="name">
              <Input style={{ width: Math.max(size.innerWidth / 2 - 320, 200) }} placeholder="请输入" />
            </Field>
            <Field label="备注" name="remark">
              <Input style={{ width: Math.max(size.innerWidth / 2 - 240, 200) }} placeholder="请输入" />
            </Field>
            <Field label="版本号" name="version" />
            <Field label="模板类型" name="tempType" />
            <Field label="模板" name="template">
              <textarea
                className={styles.textarea}
                style={{
                  width: Math.max(size.innerWidth / 2 - 240, 200),
                  height: Math.max(size.innerHeight - 380, 300),
                }}
              />
            </Field>
          </Fields>
        </div>
        <div className={styles.operate}>
          <div className={styles.choose}>
            <input id="files" type="file" accept=".json" style={{ display: 'none' }} onChange={fileImport} />
            <Button disabled={trigger} onClick={onSelectFile}>
              选择模板文件
            </Button>
          </div>
          <Button disabled={trigger || data === null} onClick={uploadFile}>
            上传当前模板
          </Button>
          <div className={styles.choose}>
            <input
              id="files2"
              type="file"
              accept=".json"
              multiple
              key={trigger}
              style={{ display: 'none' }}
              onChange={fileImport2}
            />
            <Button disabled={trigger} onClick={onAddList}>
              批量上传
            </Button>
            <Button
              onClick={() => {
                logRef.current = [];
                setLogx([]);
              }}
            >
              清除日志
            </Button>
          </div>
        </div>

        <div className={styles.log} ref={refDiv}>
          {logx.map((l) => {
            return (
              <div
                key={l.id}
                className={l.type === 'error' ? styles.error : l.type === 'success' ? styles.success : null}
                style={{ minHeight: 10 }}
              >
                {l.msg}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

Template.defaultProps = {
  className: null,
};

Template.propTypes = {
  className: PropTypes.any,
};

export default Template;
