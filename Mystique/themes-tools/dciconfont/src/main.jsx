import React from 'react';
import ReactDOM from 'react-dom';
import App from '@/example';
import { name } from '../package.json';

document.title = `${name} Dev`;

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root'),
);
