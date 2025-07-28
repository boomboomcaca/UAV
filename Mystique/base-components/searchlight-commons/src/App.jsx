/*
 * @Author: wangXueDong
 * @Date: 2021-09-08 15:28:42
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-07-01 10:29:35
 */
import React from 'react';
import { HashRouter, Route, Switch, Redirect } from 'react-router-dom';
import routes from '@/routes';
import './global.less';

export default () => (
  <HashRouter>
    <Switch>
      <Route path="/" exact render={() => <Redirect to="/home" />} />
      {routes.map((item) => {
        return (
          <Route
            key={item.path}
            path={item.path}
            exact={item.exact}
            render={(props) => {
              return <item.component {...props} />;
            }}
          />
        );
      })}
      <Redirect to="/404" />
    </Switch>
  </HashRouter>
);
