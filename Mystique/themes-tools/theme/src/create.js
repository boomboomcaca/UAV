'use strict';

const fs = require('fs');
const config = require('./config.js');

/**
 * 生成css字符串
 * @returns css string
 */
const createThemeVarCSS = () => {
  const space = ' ';

  const themes = Object.keys(config[0]).filter((f) => f !== 'name');

  return themes
    .map((t) => {
      // dark == root
      const root = t === 'dark' ? ':root,\n' : '';
      // add head
      let css = `${root}:root[theme='${t}'] {`;
      // add css var
      config.forEach((c) => {
        css += `\n${space}${c.name}: ${c[t]};`;
      });
      // add foot
      css += '\n}';
      return css;
    })
    .join(`\n\n`);
};

try {
  fs.writeFileSync('./dist/theme.css', createThemeVarCSS());
  console.log('\r\n build theme.css succeed :)\r\n');
} catch (error) {
  console.error('\r\n build theme.css failed :(\r\n');
}
