'use strict';

const path = require('path');
const fs = require('fs');
const distname = './src/Icons/index.jsx';
const svgDir = './src/Icons/assets/svg/';

const listpath = path.join(__dirname, distname);
const list = {};
// 纯色SVG

fs.stat(listpath, (error, stats) => {
  console.log('listpath:::', listpath);
  fs.readdir(svgDir, (error, files) => {
    const file = fs.createWriteStream(listpath, 'utf8');
    file.write('import React from "react";\n', 'utf8');
    file.write('import "./style.css";\n', 'utf8');

    files.forEach((item) => {
      console.log(item);
      fs.readFile(path.join(__dirname, svgDir + item), 'utf8', (error, content) => {
        let fileName = item.replace('.svg', '');
        fileName = fileName = fileName.replace(fileName[0], fileName[0].toUpperCase());
        let trueContent;
        if (fileName.indexOf('Colored') >= 0) {
          trueContent = templatecolor(fileName, content);
        } else {
          trueContent = template(fileName, content);
        }

        list[fileName] = trueContent; //content.split("\n")[0].replace(/\/+/, "").trim();
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

function templatecolor(iconName, content) {
  let transedEontent = content.replace(/(svg[^>]+)/, function (x) {
    return (
      x +
      `  className={'dc-icon '+ className}
    ref={ref}
    onClick={onClick}
    style={{
     
      width: iconSize,
      height: iconSize,
      ...style,
    }}`
    );
  });

  transedEontent = transedEontent.replace(/-\w/g, function (x) {
    return x.slice(1).toUpperCase();
  });

  return `
    export const ${iconName}Icon = React.forwardRef((props, ref) => {
      const { style, onClick, color = "#fff", iconSize, className } = props;
      return (
        
          ${transedEontent}
     
      );
    });
    
    `;
}

function template(iconName, content) {
  // let transedEontent = content.replace(/<svg[^>]+>/g, "");
  //
  let transedEontent = content.replace(/(svg[^>]+)/, function (x) {
    return (
      x +
      `  className={'dc-icon '+ className}
    ref={ref}
    onClick={onClick}
    style={{
     
      width: iconSize,
      height: iconSize,
      ...style,
    }}`
    );
  });

  transedEontent = transedEontent.replace(
    // /fill="[#]{0,1}[a-z,A-Z,0-9]{1,}"/g,
    // /fill="(?:(?!none).)*"/g,
    /fill="(?:(?!none).)*"/g,
    'fill={color}',
  );
  transedEontent = transedEontent.replace(/stroke="[#]{0,1}[a-z,A-Z,0-9]{1,}"/g, 'stroke={color}');
  transedEontent = transedEontent.replace(/-\w/g, function (x) {
    return x.slice(1).toUpperCase();
  });
  return `
    export const ${iconName}Icon = React.forwardRef((props, ref) => {
      const { style, onClick, color = "#fff", iconSize , className} = props;
      return (
        
          ${transedEontent}
     
      );
    });
    
    `;
}
