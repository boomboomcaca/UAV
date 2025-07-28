import React from 'react';
import ReactDOM from 'react-dom';
import App from '@/app'; // 单应用
// import App from '@/layout/Base.jsx'; // 路由模式
import '@dc/theme';
import Store from '@/store';
import '@/styles/global.less';

function getElement(container) {
  return (container || document).querySelector('#root');
}

export function render(props) {
  const { container, master, options } = props;
  ReactDOM.render(
    <React.StrictMode>
      <Store actions={{ master, options }}>
        <App />
      </Store>
    </React.StrictMode>,
    getElement(container),
  );
}

export function destroy(props) {
  ReactDOM.unmountComponentAtNode(getElement(props.container));
}
