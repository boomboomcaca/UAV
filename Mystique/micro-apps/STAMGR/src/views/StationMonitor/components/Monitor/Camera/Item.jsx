import React, { useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { FullScreenIcon } from 'dc-icon';
import getConfig from '@/config';
import styles from './item.module.less';

const { videoServerUrl } = getConfig();

const Item = (props) => {
  const { className, item } = props;

  const imgRef = useRef(null);

  const onRedirect = () => {
    const reg = new RegExp(/\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}/);
    window.open(`http://${item.url.match(reg)[0]}`);
  };

  useEffect(() => {
    const interval = setInterval(() => {
      fetch(`${videoServerUrl}/videoService/getVideo?moduleId=${item.id}`)
        .then((res) => {
          res.arrayBuffer().then((buffer) => {
            const blob = new Blob([buffer], { type: 'image/jpeg' });
            imgRef.current.src = URL.createObjectURL(blob);
          });
        })
        .catch((err) => window.console.log(err));
    }, 1000);
    return () => {
      clearInterval(interval);
    };
  }, []);

  return (
    <div className={classnames(styles.camera, className)}>
      <div className={styles.title}>{item.name}</div>
      <div className={styles.video}>
        <img alt="" ref={imgRef} width="100%" height="100%" />
      </div>
      <div className={styles.operate}>
        <FullScreenIcon onClick={onRedirect} />
      </div>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
};

export default Item;
