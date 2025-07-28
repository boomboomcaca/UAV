const path = require('path');
const fs = require('fs');

const distname = './index.d.ts';
const svgDir = './src/Icons/assets/svg/';

const listpath = path.join(__dirname, distname);
const list = {};

fs.stat(listpath, (error, stats) => {
  fs.readdir(svgDir, (error, files) => {
    const file = fs.createWriteStream(listpath, 'utf8');
    file.write(
      `import React from "react";\n
        interface Iprops {
        style: any;
        onClick: () => void;
        color: string;
        iconSize: number;
      };\n`,
      'utf8',
    );

    files.forEach((item) => {
      fs.readFile(path.join(__dirname, svgDir + item), 'utf8', (error, content) => {
        let fileName = item.replace('.svg', '');
        fileName = fileName = fileName.replace(fileName[0], fileName[0].toUpperCase());

        list[fileName] = `export class ${fileName}Icon extends React.PureComponent<Iprops, any> {}`;
        if (Object.keys(list).length === files.length) {
          // 执行完成
          Object.keys(list)
            .sort()
            .forEach((key) => {
              let element = list[key];
              file.write(`${element}\n`, 'utf8');
            });
        }
      });
    });
  });
});
