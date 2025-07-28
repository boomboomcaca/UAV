import React from 'react';
import message from '../index';
import Button from '../../Button';
import icons from '../icons.jsx';
import styles from './index.module.less';

export default function Demo() {
  return (
    <div className={styles.root}>
      <Button onClick={() => message.success(`message.success('mess...')`)}>message.success</Button>
      <Button onClick={() => message.info(`message.info('mess...')`)}>message.info</Button>
      <Button onClick={() => message.warning(`message.warning('mess...')`)}>message.warning</Button>
      <Button onClick={() => message.warn(`message.warn('mess...')`)}>message.warn</Button>
      <Button onClick={() => message.error(`message.error('mess...')`)}>message.error</Button>
      <Button onClick={() => message.loading(`message.loading('mess...')`)}>message.loading</Button>

      <Button
        onClick={() =>
          message.success({
            key: 'keyID_142857',
            content: `message.success({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 5,
          })
        }
      >
        message.success(key)
      </Button>
      <Button
        onClick={() =>
          message.info({
            key: 'keyID_142857',
            content: `message.info({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 5,
          })
        }
      >
        message.info(key)
      </Button>
      <Button
        onClick={() =>
          message.warning({
            key: 'keyID_142857',
            content: `message.warning({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 5,
          })
        }
      >
        message.warning(key)
      </Button>
      <Button
        onClick={() =>
          message.warn({
            key: 'keyID_142857',
            content: `message.warn({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 5,
          })
        }
      >
        message.warn(key)
      </Button>
      <Button
        onClick={() =>
          message.error({
            key: 'keyID_142857',
            content: `message.error({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 5,
          })
        }
      >
        message.error(key)
      </Button>
      <Button
        onClick={() =>
          message.loading({
            key: 'keyID_142857',
            content: `message.loading({ key: 'keyID_142857', content: 'mess...', duration: 5 })`,
            duration: 50,
          })
        }
      >
        message.loading(key)
      </Button>
      <Button onClick={() => message.Toast(`成功显示`)}>message.Toast(key)</Button>
      <Button
        onClick={() =>
          message.Toast({
            icon: icons.success,
            content: `成功显示`,
          })
        }
      >
        Toast(自定义icon)
      </Button>
    </div>
  );
}
