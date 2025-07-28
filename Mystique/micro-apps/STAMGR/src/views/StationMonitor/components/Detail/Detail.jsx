import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message, Modal } from 'dui';
import { RefreshIcon } from 'dc-icon';
import { restartEdge } from '@/api/cloud';
import Fields, { Field } from '@/components/Fileds';
import Button from '@/components/Button';
import { fields } from './detail';
import styles from './index.module.less';

const Detail = (props) => {
  const { className, data } = props;

  const onRestart = () => {
    Modal.confirm({
      title: '提示',
      closable: false,
      content: '确定要重启边缘服务？',
      onOk: async () => {
        const p = { edgeId: data.id };
        // 此处不重启环境监控
        // const find = data.modules.find((r) => {
        //   return r.moduleCategory[0] === 'control';
        // });
        // if (find) {
        //   p.deviceId = find.id;
        // }
        const res = await restartEdge(p);
        if (res) {
          message.success({ key: 'tip', content: '发送重启指令成功' });
        }
      },
    });
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.form}>
        <Fields data={data} labelStyle={{ width: 120, opacity: 0.5 }}>
          {fields.map((i) => {
            return (
              <Field label={i.label} name={i.name} style={i.name === 'stateStr' ? { color: data?.stateColor } : null} />
            );
          })}
        </Fields>
      </div>
      <div className={styles.more}>
        <Button onClick={onRestart} className={styles.restart} primary={false}>
          <>
            <RefreshIcon color="#ffffff80" />
            重启边缘服务
          </>
        </Button>
      </div>
    </div>
  );
};

Detail.defaultProps = {
  className: null,
  data: null,
};

Detail.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
};

export default Detail;
