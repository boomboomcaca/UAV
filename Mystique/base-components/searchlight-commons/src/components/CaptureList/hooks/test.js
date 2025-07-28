const downloadUseEleA = (content, fileName) => {
  // a标签下载
  const a = document.createElement('a');
  a.href = content;
  a.download = fileName;
  a.style.display = 'none';
  document.body.appendChild(a);
  a.click();
  setTimeout(() => {
    document.body.removeChild(a);
    // 2022-1-24 liujian 释放资源
    window.URL.revokeObjectURL(content);
  }, 1000);
};

const saveFileOnWeb = (fileName, dataString, utf8) => {
  return new Promise((resolve, reject) => {
    fetch(dataString)
      .then((res) => {
        res.blob().then((b) => {
          downloadUseEleA(URL.createObjectURL(b), fileName);
          resolve();
        });
      })
      .catch((er) => {
        console.log('download error', er);
        reject(er);
      });
  });
};

const downloadAndSaveFile = (url, type, name) => {
  return new Promise((resolve, reject) => {
    if (!url.trim().startsWith('http')) {
      reject(new Error('Invalid URL'));
    } else {
      const lastPoint = url.lastIndexOf('.');
      const fileExt = url.substring(lastPoint);
      const lastSymbol = url.lastIndexOf('/');
      const fileName = name || url.substring(lastSymbol + 1).replace(fileExt, '');
      saveFileOnWeb(`${fileName}${fileExt}`, url)
        .then((res) => {
          resolve();
        })
        .catch((er) => {
          reject(er);
        });
    }
  });
};

export default downloadAndSaveFile;
