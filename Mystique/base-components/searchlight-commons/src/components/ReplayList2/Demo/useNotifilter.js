import { useState, useEffect } from 'react';
import notifilter from 'notifilter';
import { message } from 'dui';
import langT from 'dc-intl';

function useNotifilter(wsNotiUrl, appid) {
  const [syncData, setSyncData] = useState(null);

  useEffect(() => {
    const unregister = notifilter.register({
      url: wsNotiUrl,
      onmessage: (res) => {
        const { result } = res;
        if (result && result.dataCollection) {
          for (let i = 0; i < result.dataCollection.length; i += 1) {
            const noti = result.dataCollection[i];
            if (noti.type === 'sync') {
              if (res.result && res.result.dataCollection) {
                const ress = res.result.dataCollection[0];
                if (ress && ress.sourceFile !== '' && ress.pluginId === appid) {
                  if (ress.syncCode === 200) {
                    setSyncData(ress);
                  } else {
                    message.error({ key: 'commons-main', content: langT('commons', 'dataSyncFailure') });
                    setSyncData({ sourceFile: ress.sourceFile, rate: '-1%' /* , syncCode: 200 */ });
                  }
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
