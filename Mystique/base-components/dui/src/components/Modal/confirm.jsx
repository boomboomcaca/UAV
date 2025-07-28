/* eslint-disable no-param-reassign */
import React from 'react';
import ReactDOM from 'react-dom';

import Modal from './index.jsx';

const loop = () => {};

export default function confirm({ title, content, style, renderOverFn = loop, ...restProps }) {
  if (!title && !content) {
    return {
      close: () => {},
    };
  }

  const div = document.createElement('div');
  document.body.appendChild(div);

  const close = () => {
    ReactDOM.unmountComponentAtNode(div);
    if (div && div.parentNode) {
      div.parentNode.removeChild(div);
    }
  };

  if (restProps.onOk) {
    const snapOk = restProps.onOk;
    const okf = () => {
      snapOk();
      close();
    };
    restProps.onOk = okf;
  }

  if (restProps.onCancel) {
    const snapCancel = restProps.onCancel;
    const cancel = () => {
      snapCancel();
      close();
    };
    restProps.onCancel = cancel;
  }

  ReactDOM.render(
    <Modal visible title={title} style={{ width: '420px', ...style }} onCancel={close} usePortal={false} {...restProps}>
      <div style={{ textAlign: 'center', fontSize: '14px', fontWeight: '700' }}>{content}</div>
    </Modal>,
    div,
  );

  renderOverFn && renderOverFn();

  return {
    close,
  };
}
