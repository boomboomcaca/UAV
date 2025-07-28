import React, { useCallback, useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Empty, Switch } from 'dui';
// import Switch from './Switch';
import ListItem from './ListItem.jsx';
import HireachyList from './HireachyList.jsx';
import { groupBy, scrollToAnchor, PropertyType } from './weapon';
import { createTimeID } from '../../lib/random';
import styles from './index.module.less';

const PropertyList = (props) => {
  const { enable, params, filter, hideKeys, disableKeys, refresh, OnParamsChanged } = props;

  const pid = useRef(createTimeID()).current;

  const groupRef = useRef();
  const headRef = useRef();
  const footRef = useRef();
  const groupItems = useRef([]).current;

  const [groupedParams, setGroupedParams] = useState(null);

  const [navIndex, setNavIndex] = useState(-1);

  const [key, setKey] = useState(0);

  useEffect(() => {
    const offset = 50;
    const group = groupRef.current;
    const head = headRef.current;
    const foot = footRef.current;
    if (foot) {
      foot.style.height = `${group.offsetHeight}px`;
    }
    if (group) {
      group.addEventListener('scroll', (e) => {
        const top = e.target.scrollTop;
        if (groupItems.length > 0) {
          if (top < head.offsetHeight / 2) {
            setNavIndex(-1);
          } else {
            for (let i = 0; i < groupItems.length; i += 1) {
              const elem1 = document.getElementById(groupItems[i]);
              const h1 = elem1.offsetHeight;
              const gtop = elem1.offsetTop;
              if (top > gtop - offset && top < gtop + h1 - offset) {
                setNavIndex(i);
                break;
              }
            }
          }
        }
      });
    }
    setTimeout(() => {
      if (groupItems.current?.length === groupedParams?.length) {
        setNavIndex(0);
        scrollToAnchor(groupItems[0]);
      }
    }, 0);
  }, []);

  useEffect(() => {
    if (params) {
      let parameters = null;
      if (filter === 'install') {
        parameters = params.filter((pp) => {
          return pp.isInstallation;
        });
      } else if (filter === 'notinstall') {
        parameters = params.filter((pp) => {
          return !pp.isInstallation;
        });
      } else {
        parameters = params;
      }
      parameters = parameters.filter((p) => {
        return !hideKeys.includes(p.name) && p.displayName !== '';
      });
      const groupArr = groupBy(parameters, (item) => {
        return item.category; // 按照category进行分组
      });
      setGroupedParams(groupArr);
    }
  }, [params, filter, key]);

  const OnValueChanged = (p, v, bo = true) => {
    const find = params.find((param) => {
      return param.name === p.name;
    });
    const old = bo ? find.value : find.parameters;
    if (find && old !== v) {
      bo ? (find.value = v) : (find.parameters = v);
      refresh && setKey(key + 1);
      let newParams = [...params];
      if (filter === 'notinstall') {
        newParams = [
          ...params.filter((pp) => {
            return !pp.isInstallation;
          }),
        ];
      }
      OnParamsChanged(newParams, p.name, old, v);
    }
  };

  const genNavs = useCallback(() => {
    return groupedParams?.map((g, i) => {
      const id = `${pid}-${g.name}`;
      return (
        <div
          key={id}
          className={i === navIndex ? styles.navChked : null}
          onClick={() => {
            setNavIndex(i);
            scrollToAnchor(id);
          }}
        >
          {g.name}
        </div>
      );
    });
  }, [navIndex, groupedParams]);

  const genGroups = useCallback(() => {
    groupItems.splice(0, groupItems.length);
    return groupedParams?.map((g, i) => {
      const id = `${pid}-${g.name}`;
      groupItems.push(id);
      return (
        <div key={id} id={id} style={{ paddingBottom: 28 }}>
          <div className={classnames(styles.title, navIndex === i ? styles.titleChked : null)}>
            {g.name}
            <div />
          </div>
          {g.data
            .filter((item) => {
              return !(item.type === 5 || item.type === 'bool');
            })
            .map((p) => {
              const type = ListItem.getType(p);
              const disable = disableKeys === undefined || disableKeys === null ? false : disableKeys?.includes(p.name);
              return (
                <div key={p.name} className={styles.groupItem}>
                  <span
                    className={classnames(disable ? styles.disable : null)}
                    style={{ marginRight: 16 }}
                  >{`${p.displayName}`}</span>
                  {type === PropertyType.LIST ? (
                    <HireachyList
                      param={p}
                      disableKeys={disableKeys}
                      onValueChanged={(param, val) => {
                        OnValueChanged(param, val, false);
                      }}
                    />
                  ) : (
                    <ListItem disabled={disable} param={{ ...p }} onValueChanged={OnValueChanged} />
                  )}
                </div>
              );
            })}
          {/* <div
            className={styles.switchesPan}
            style={
              g.data.filter((item) => {
                return item.type === 5 || item.type === 'bool';
              }).length > 0
                ? null
                : { margin: '24px 0 0 0' }
            }
          > */}
          {g.data
            .filter((item) => {
              return item.type === 5 || item.type === 'bool';
            })
            .map((p) => {
              const disable = disableKeys === undefined || disableKeys === null ? false : disableKeys?.includes(p.name);
              let idx = -1;
              if (p.values && p.displayValues) {
                idx = p.values.indexOf(p.value);
              }
              return (
                <div key={`${p.name}`} className={styles.switchesItm}>
                  <span
                    className={classnames(disable ? styles.disable : null)}
                    style={{ marginRight: 16 }}
                  >{`${p.displayName}`}</span>
                  <Switch
                    selected={p.value}
                    disable={disable}
                    // checkedChildren={p.displayValues[0]}
                    // unCheckedChildren={p.displayValues[1]}
                    onChange={(val) => {
                      OnValueChanged(p, val);
                    }}
                  />
                  <div className={styles.switchTag}>
                    {idx === -1 ? p.displayValues[p.value ? 0 : 1] : p.displayValues[idx]}
                  </div>
                </div>
              );
            })}
        </div>
        // </div>
      );
    });
  }, [navIndex, groupedParams, disableKeys]);

  return (
    <div className={styles.root}>
      <div className={styles.nav}>{genNavs()}</div>
      <div ref={groupRef} className={styles.group}>
        <div
          style={
            enable
              ? null
              : {
                  pointerEvents: 'none',
                  opacity: '0.75',
                }
          }
        >
          <div ref={headRef} className={styles.header} />
          {groupedParams && groupedParams.length > 0 ? genGroups() : null}
          <div
            ref={footRef}
            className={styles.footer}
            // style={groupedParams && groupedParams.length > 0 ? null : { height: 0 }}
          />
        </div>
      </div>
      {groupedParams && groupedParams.length > 0 ? null : <Empty className={styles.empty} />}
    </div>
  );
};

PropertyList.defaultProps = {
  enable: true,
  params: null,
  filter: 'notinstall',
  hideKeys: [],
  disableKeys: [],
  refresh: false,
  OnParamsChanged: () => {},
};

PropertyList.propTypes = {
  enable: PropTypes.bool,
  params: PropTypes.array,
  filter: PropTypes.any,
  hideKeys: PropTypes.array,
  disableKeys: PropTypes.any,
  refresh: PropTypes.bool,
  OnParamsChanged: PropTypes.func,
};

export default PropertyList;
