import React from 'react';
import { HashRouter, Route, Switch, Redirect } from 'react-router-dom';
import '@dc/theme';
// import Theme, { Mode } from '@dc/theme/dist/tools';
import routes from '@/routes';
import './global.less';

// window.GlobalTheme = new Theme(Mode.light);

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
