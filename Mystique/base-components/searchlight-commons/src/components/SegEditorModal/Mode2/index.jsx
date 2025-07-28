import React, { memo, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Input, message, Modal, Empty } from 'dui';
import { EDITSVG, GOUGOUSVG, FANHUISVG, CLOSESVG, IPTSVG } from '../svg.jsx';
import styles from './index.module.less';

// 格式化时间文本
export const stampDate = (s = new Date(), format = 'yyyy-MM-dd HH:mm:ss') => {
  const dtime = new Date(s);
  const o = {
    'M+': dtime.getMonth() + 1, // month
    'd+': dtime.getDate(), // day
    'H+': dtime.getHours(), // hour
    'm+': dtime.getMinutes(), // minute
    's+': dtime.getSeconds(), // second
    'q+': Math.floor((dtime.getMonth() + 3) / 3), // quarter
    'f+': dtime.getMilliseconds(), // millisecond
    S: dtime.getMilliseconds(), // millisecond
  };
  if (/(y+)/.test(format)) format = format.replace(RegExp.$1, `${dtime.getFullYear()}`.substr(4 - RegExp.$1.length));
  for (const k in o)
    if (new RegExp(`(${k})`, 'g').test(format))
      format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : `00${o[k]}`.substr(`${o[k]}`.length));
  return format;
};

const Mode2 = (props) => {
  const { axios, limit, onMode2IPT, mode2ReKey } = props;

  const [data, setdata] = useState([]);
  const [editIdx, seteditIdx] = useState(-1);
  const [name, setname] = useState('');

  const onEdit = () => {
    if (name === '') {
      message.info({ key: 'Mode2', content: `请输入列表名称` });
    }
    axios?.({
      url: '/sys/scanInfo/update',
      method: 'post',
      data: { id: data[editIdx].id, name },
    }).then(() => {
      getData();
      seteditIdx(-1);
    });
  };

  const onDelete = (item) => {
    axios?.({
      url: '/sys/scanInfo/del',
      method: 'post',
      data: { id: item.id },
    }).then(() => {
      getData();
    });
  };

  const getData = () => {
    axios?.({
      url: '/sys/scanInfo/getList',
      method: 'get',
    }).then((res) => {
      setdata(res.result);
    });
  };

  const onSelect = (segs) => {
    let isHaveStepNone = false;
    segs.forEach((item) => {
      if (![...limit.stepItems, 63].includes(item.stepFrequency)) {
        isHaveStepNone = true;
      }
    });
    if (isHaveStepNone) {
      message.info({ key: 'Mode2', content: `存在频段有设备不支持的步进` });
      return;
    }
    onMode2IPT(segs);
  };

  useEffect(() => {
    getData();
  }, [mode2ReKey]);

  return (
    <div className={styles.Mode2}>
      {data.length === 0 ? (
        <Empty emptype={Empty.Normal} message="暂无数据" />
      ) : (
        <div className={styles.listArea}>
          {data.map((item, idx) => (
            <div className={classnames(styles.groupitem, { [styles.edit]: idx === editIdx })} key={item.id}>
              {idx === editIdx ? (
                <>
                  <Input
                    style={{ width: 220 }}
                    placeholder="请输入名称"
                    value={name}
                    maxLength={12}
                    onChange={(val) => setname(val)}
                  />
                  <div className={styles.dealsvgheightbox} onClick={onEdit}>
                    {GOUGOUSVG}
                  </div>
                  <div className={styles.dealsvgheightbox} onClick={() => seteditIdx(-1)}>
                    {FANHUISVG}
                  </div>
                </>
              ) : (
                <>
                  <div
                    className={styles.editIcon}
                    onClick={() => {
                      seteditIdx(idx);
                      setname(item.name);
                    }}
                  >
                    {EDITSVG}
                  </div>
                  <div className={styles.infoArea}>
                    <div className={styles.name}>{item.name}</div>
                    <div className={styles.extra}>
                      <div>{stampDate(item.updateTime || item.createTime, 'yyyy-MM-dd')}</div>
                      <div>
                        <span>频段个数：</span>
                        <span style={{ color: '#fff' }}>{item.scanInfo?.length}</span>
                      </div>
                    </div>
                  </div>
                  <div
                    className={classnames(styles.iptBtn, styles.dealsvgheightbox)}
                    onClick={() => onSelect(item.scanInfo)}
                  >
                    {IPTSVG}
                  </div>
                </>
              )}
              <div
                className={styles.closeIcon}
                onClick={() => {
                  Modal.confirm({
                    title: '提示',
                    content: `是否确认删除此频段表？`,
                    onOk: () => {
                      onDelete(item);
                    },
                  });
                }}
              >
                {CLOSESVG}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

Mode2.propTypes = {
  axios: PropTypes.func.isRequired,
  limit: PropTypes.object.isRequired,
  onMode2IPT: PropTypes.func.isRequired,
  mode2ReKey: PropTypes.any.isRequired,
};

export default memo(Mode2);
