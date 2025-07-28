/* eslint-disable no-param-reassign */
import { useState, useEffect } from 'react';
import { getTemplates } from '@/api/template';
import useDictionary from '@/hooks/useDictionary';

function useTemplate(type, visible, filter = null, reverse = false) {
  const [templates, setTemplates] = useState();

  const [selects, setSelects] = useState([]);

  const [versions, setVersions] = useState([]);

  const [key, setKey] = useState(0);

  const refresh = () => {
    setKey(key + 1);
  };

  const isSelected = (item) => {
    const find = selects.find((s) => {
      return s.id === item.id;
    });
    window.console.log(item, find);
    if (find) {
      return true;
    }
    return false;
  };

  const updateSelects = (item) => {
    const find = selects.find((s) => {
      return s.id === item.id;
    });
    let news = null;
    if (find) {
      const idx = selects.indexOf(find);
      selects.splice(idx, 1);
      news = [...selects];
    } else {
      news = [...selects, item];
    }
    setSelects(news);
    window.console.log(news);
  };

  const updateSelectsAfterVersion = (item) => {
    const res = selects.filter((s) => {
      return s.name === item.name && s.module_category === item.module_category && s.id !== item.id;
    });
    if (res) {
      res.forEach((r) => {
        const idx = selects.indexOf(r);
        selects.splice(idx, 1);
      });
      setSelects([...selects]);
    }
  };

  const updateVersions = (item) => {
    // TODO 关联的有点强
    updateSelectsAfterVersion(item);

    const find = versions.find((s) => {
      return s.name === item.name && s.module_category === item.module_category;
    });
    let news = null;
    if (find) {
      const idx = versions.indexOf(find);
      versions.splice(idx, 1);
    }
    news = [...versions, item];
    setVersions(news);
    window.console.log(news);
  };

  const { dictionary } = useDictionary(['moduleCategory']);

  const getTemplate = () => {
    getTemplates(type).then((res) => {
      console.log('get template:::', res);
      if (dictionary) {
        const mcates = [
          ...dictionary[0].data.map((dd) => {
            dd.templates = [];
            return dd;
          }),
        ];
        if (mcates) {
          let ret = res.result;
          if (filter) {
            ret = res.result.filter((rr) => {
              const bo = rr.moduleCategory === filter;
              return reverse ? !bo : bo;
            });
          }
          ret
            // TODO test MR3000A
            // .filter((r) => {
            //   return r.name === 'MR3000A';
            // })
            .forEach((rr) => {
              const find = mcates.find((m) => {
                return m.key === rr.moduleCategory;
              });
              if (find) {
                if (!find.templates) {
                  find.templates = [];
                } else {
                  rr.moduleCategoryStr = find.value;
                  find.templates.push(rr);
                }
              }
            });
        }
        console.log('set templates :::', mcates);
        setTemplates(mcates);
      }
    });
  };

  useEffect(() => {
    if (type && visible) {
      getTemplate();
    }
  }, [type, visible, dictionary]);

  useEffect(() => {
    if (key) {
      getTemplate();
    }
  }, [key]);

  const updateParam = (str) => {
    if (templates) {
      templates.forEach((t) => {
        t.templates?.forEach((tt) => {
          if (str) {
            if (tt.name.indexOf(str) > -1) {
              tt.show = true;
            } else {
              tt.show = false;
            }
          } else {
            tt.show = true;
          }
        });
      });
      setTemplates([...templates]);
    }
  };

  return { refresh, templates, updateParam, selects, setSelects, isSelected, updateSelects, versions, updateVersions };
}

export default useTemplate;
