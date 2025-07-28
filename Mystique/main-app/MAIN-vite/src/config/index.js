const initConfig = () => {
  const config = { ...window.projConfig.syncconfiguration };
  config.appid = "c1b38349-c9bb-4767-9711-97c5d14089ca";
  return config;
};

export default function getConf() {
  return initConfig();
}

const mainConf = { ...window.projConfig.main };

export { mainConf };
