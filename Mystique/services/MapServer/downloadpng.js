const {
  getTileWidthHttps1,
  getTileWidthHttp1,
} = require("./utils/tileDownloader");
const fs = require("fs");

const urls = [
  "http://127.0.0.1:8182/tile?x=6459&y=3363&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6459&y=3364&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6459&y=3365&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6459&y=3366&z=13&ms=complex&mt=roades",

  "http://127.0.0.1:8182/tile?x=6460&y=3363&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6460&y=3364&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6460&y=3365&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6460&y=3366&z=13&ms=complex&mt=roades",

  "http://127.0.0.1:8182/tile?x=6461&y=3363&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6461&y=3364&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6461&y=3365&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6461&y=3366&z=13&ms=complex&mt=roades",

  "http://127.0.0.1:8182/tile?x=6462&y=3363&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6462&y=3364&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6462&y=3365&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6462&y=3366&z=13&ms=complex&mt=roades",

  "http://127.0.0.1:8182/tile?x=6463&y=3363&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6463&y=3364&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6463&y=3365&z=13&ms=complex&mt=roades",
  "http://127.0.0.1:8182/tile?x=6463&y=3366&z=13&ms=complex&mt=roades",
];
const urls1 = [
  "http://127.0.0.1:8182/tile?x=6459&y=3363&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6459&y=3364&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6459&y=3365&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6459&y=3366&z=13&ms=complex&mt=statelite",

  "http://127.0.0.1:8182/tile?x=6460&y=3363&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6460&y=3364&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6460&y=3365&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6460&y=3366&z=13&ms=complex&mt=statelite",

  "http://127.0.0.1:8182/tile?x=6461&y=3363&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6461&y=3364&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6461&y=3365&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6461&y=3366&z=13&ms=complex&mt=statelite",

  "http://127.0.0.1:8182/tile?x=6462&y=3363&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6462&y=3364&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6462&y=3365&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6462&y=3366&z=13&ms=complex&mt=statelite",

  "http://127.0.0.1:8182/tile?x=6463&y=3363&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6463&y=3364&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6463&y=3365&z=13&ms=complex&mt=statelite",
  "http://127.0.0.1:8182/tile?x=6463&y=3366&z=13&ms=complex&mt=statelite",
];
// urls.forEach((url) => {
//   const arr = url.split("?")[1].split(["&"]);
//   const name = arr[0].replace("=", "") + arr[1].replace("=", "") + ".png";
//   console.log(arr);
//   getTileWidthHttp1(url, (e) => {
//     fs.writeFileSync(name, e);
//   });
// });

urls1.forEach((url, index) => {
  const arr = url.split("?")[1].split(["&"]);
  const dirName = `c${Math.floor(index / 4) + 1}`;
  if (!fs.existsSync(dirName)) {
    fs.mkdirSync(dirName);
  }
  const name =
    dirName + "/" + arr[0].replace("=", "") + arr[1].replace("=", "") + ".png";

  getTileWidthHttp1(url, (e) => {
    console.log(url, e);
    fs.writeFileSync(name, e);
  });
});
