import { renderWithQiankun, qiankunWindow } from 'vite-plugin-qiankun/dist/helper';
import login from '@/auth/Login';
import { render, destroy } from './index.jsx';

// 独立运行
// eslint-disable-next-line no-underscore-dangle
if (!qiankunWindow.__POWERED_BY_QIANKUN__) {
  // 渲染登录模块，已登录则render
  login(() => {
    render({});
  });
}

// 在主应用中运行
renderWithQiankun({
  mount(props) {
    render(props);
  },
  bootstrap() {
    // console.log('micro app bootstraped');
  },
  unmount(props) {
    destroy(props);
  },
});
