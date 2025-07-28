const os = require('os');
const fs = require('fs');
const path = require('path');
const ffi = require('ffi-napi');

function getOsType() {
  let osType;
  if (os.type() === 'Windows_NT') {
    osType = 0;
    // windows
  } else if (os.type() === 'Linux') {
    osType = 1;
    // Linux
  } else if (os.type() === 'Darwin') {
    osType = 2;
    // mac
  } else {
    osType = -1;
    // 不支持提示
  }
  return osType;
}

const getMachineCode = () => {
  const osType = getOsType();
  let libName = 'libMachineCodeGenerator';
  if (osType === 0) {
    libName += '.dll';
  } else if (osType === 1) {
    libName += '.so';
  } else {
    return null;
  }

  const dirPath = path.join(process.cwd(), 'lib');
  const libPath = path.join(dirPath, libName);

  if (!fs.existsSync(libPath)) {
    return null;
  }
  try {
    const myLibrary = new ffi.Library(libPath, {
      generateMachineCode: ['string', []],
    });
    const result = myLibrary.generateMachineCode();
    return result;
  } catch (err) {
    return null;
  }
};

module.exports = {
  getMachineCode,
};
