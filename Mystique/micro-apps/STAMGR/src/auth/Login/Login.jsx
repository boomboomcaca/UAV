import React, { useState, useRef } from 'react';
import ReactDOM from 'react-dom';
import { getToken, setToken } from '@/auth/tools';
import request from '@/utils/request';
import styles from './styles.module.less';

function login(data) {
  return request({
    url: '/auth/user/login',
    method: 'post',
    data,
  });
}

const Login = () => {
  const isDevelopment = import.meta.env.MODE === 'development';
  const timer = useRef();
  const [form, saveForm] = useState(
    isDevelopment
      ? {
          account: 'admin',
          password: '123456',
        }
      : {},
  );

  const onChange = (e, key) => {
    saveForm({
      ...form,
      [key]: e?.target?.value,
    });
  };

  const onClick = () => {
    login(form).then(({ result }) => {
      if (result) {
        setToken(result);
        window.location.reload();
      }
    });
  };

  return (
    <div
      className={styles.login}
      onClick={() => {
        clearTimeout(timer.current);
      }}
    >
      <div className={styles.form}>
        <div className={styles.tips}>{document.title}</div>
        <input
          value={form.account}
          placeholder="账号"
          type="text"
          name="account"
          onChange={(e) => onChange(e, 'account')}
          autoComplete="off"
        />
        <input
          value={form.password}
          placeholder="密码"
          type="password"
          name="password"
          onChange={(e) => onChange(e, 'password')}
          autoComplete="off"
        />
        <div className={styles.button} onClick={() => onClick()}>
          登录
        </div>
      </div>
    </div>
  );
};

export default function renderLogin(renderApp) {
  if (!getToken()) {
    const id = `LOGIN_TARGET${new Date().getTime()}`;
    const e = document.createElement('div');
    e.id = id;
    document.body.append(e);

    ReactDOM.render(<Login />, document.getElementById(id));
  } else {
    renderApp();
  }
}
