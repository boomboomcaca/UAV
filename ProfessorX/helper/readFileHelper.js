const fs = require('fs');

function getView(bytes) {
  const view = new DataView(new ArrayBuffer(bytes.length));
  for (let i = 0; i < bytes.length; i++) {
    view.setUint8(i, bytes[i]);
  }
  return view;
}

exports.getFileDescriptor = (fileName) => {
  return new Promise((resolve, reject) => {
    fs.open(fileName, 'r', (err, fd) => {
      if (err) {
        reject(err);
      }
      resolve(fd);
    });
  });
};

exports.getIndexData = (fileName) => {
  return new Promise((resolve, reject) => {
    fs.readFile(fileName, async (err, data) => {
      if (err) {
        reject(err);
      }
      const indexData = [];
      const arrByte = new Uint8Array(data);
      for (let i = 0; i < arrByte.length; i += 24) {
        const indexArray = arrByte.subarray(i, i + 8);
        const index = Number(getView(indexArray.reverse()).getBigInt64());
        const offsetArray = arrByte.subarray(i + 8, i + 16);
        const offset = Number(getView(offsetArray.reverse()).getBigInt64());
        const timestampArray = arrByte.subarray(i + 16, i + 24);
        const timestamp = Number(
          getView(timestampArray.reverse()).getBigUint64()
        );
        const indexItem = {
          index,
          offset,
          timestamp,
        };
        indexData.push(indexItem);
      }
      resolve(indexData);
    });
  });
};
exports.getFileByteLength = (fileName) => {
  return new Promise((resolve, reject) => {
    fs.readFile(fileName, async (err, data) => {
      if (err) {
        reject(err);
      }
      const arrByte = new Uint8Array(data);
      resolve(arrByte.length);
    });
  });
};

exports.getContentData = (fd, contentIndex, contentLength) => {
  return new Promise((resolve, reject) => {
    /* eslint-disable new-cap */
    const buf = new Buffer.alloc(contentLength);
    fs.read(
      fd,
      buf,
      0,
      contentLength,
      contentIndex.offset,
      (err, bytesRead, buffer) => {
        if (err) {
          reject(err);
        }
        resolve(buffer);
      }
    );
  });
};
