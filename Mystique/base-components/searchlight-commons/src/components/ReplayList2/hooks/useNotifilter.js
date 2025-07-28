import { useState, useEffect } from 'react';
import notifilter from 'notifilter';
import { message } from 'dui';
import langT from 'dc-intl';

function useNotifilter(config, showPlay = false) {
  const [syncData, setSyncData] = useState(null);

  useEffect(() => {
    const unregister = notifilter.register({
      url: config.wsNotiUrl,
      onmessage: (res) => {
        const { result } = res;
        if (result && result.dataCollection) {
          for (let i = 0; i < result.dataCollection.length; i += 1) {
            const noti = result.dataCollection[i];
            if (noti.type === 'sync') {
              if (noti && noti.sourceFile !== '' && (showPlay === false ? noti.pluginId === config.appid : true)) {
                if (noti.syncCode === 200) {
                  setSyncData(noti);
                } else {
                  message.error({ key: 'commons-main', content: langT('commons', 'dataSyncFailure') });
                  setSyncData({ sourceFile: noti.sourceFile, rate: '-1%' /* , syncCode: 200 */ });
                }
              }
            }
          }
        }
      },
      dataType: ['sync'],
    });
    return () => {
      if (unregister) {
        unregister();
      }
    };
  }, []);

  return { syncData, setSyncData };
}

export default useNotifilter;
