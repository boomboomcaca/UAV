import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { Drawer, Input, Switch } from 'dui';
import classnames from 'classnames';
import { groupBy } from '@/utils/data.structure';
import Button from '@/components/Button';
import useTemplate from '@/hooks/useTemplate';
import Item from './Item.jsx';
import styles from './template.module.less';

const Template = (props) => {
  const { visible, type, onCancel, onConfirm, filter, reverse } = props;

  const { templates, updateParam, selects, setSelects, isSelected, updateSelects, versions, updateVersions } =
    useTemplate(type, visible, filter, reverse);

  const [showLatest, setShowLatest] = useState(true);

  const getItems = (subTemps) => {
    if (subTemps && subTemps.length > 0) {
      subTemps?.sort((s1, s2) => {
        if (s1.name === s2.name) {
          return s1.version > s2.version ? -1 : 1;
        }
        return s1.name > s2.name ? -1 : 1;
      });

      let subTempsTrans = subTemps;

      if (showLatest) {
        subTempsTrans = groupBy(subTemps, (x) => {
          return x.name;
        });
      }

      const show = (tt) => {
        if (showLatest) {
          const it = tt.data[0];
          return it.show === true || it.show === undefined;
        }
        return tt.show === true || tt.show === undefined;
      };

      return subTempsTrans.map((tt) => {
        let vers = null;
        if (showLatest) {
          vers =
            versions.find((v) => {
              return tt.data.find((td) => {
                return td.id === v.id;
              });
            }) || tt.data[0];
        }
        return (
          <Item
            item={tt}
            multiple={showLatest}
            className={classnames(
              styles.item,
              show(tt) ? null : styles.itemHide,
              isSelected(vers || tt) ? styles.itemCheck : null,
            )}
            onClick={updateSelects}
            onSelect={updateVersions}
          />
        );
      });
    }
    return [];
  };

  return (
    <Drawer
      visible={visible}
      width="350px"
      title={(() => {
        if (type === 'driver') {
          return '功能模板';
        }
        return '设备模板';
      })()}
      onCancel={() => {
        onCancel();
      }}
    >
      <div className={styles.filter}>
        <Input
          allowClear
          showSearch
          onSearch={(str) => updateParam(str)}
          onPressEnter={(str) => updateParam(str)}
          onChange={(val) => {
            if (val === '') {
              updateParam(null);
            }
          }}
          placeholder="搜索"
          style={{ flex: 1 }}
        />
        <Switch
          selected={showLatest}
          onChange={() => {
            setShowLatest(!showLatest);
          }}
          checkedChildren="最新"
          unCheckedChildren="平铺"
        />
      </div>
      <div className={styles.root}>
        {templates?.map((t) => {
          return (
            <>
              <div className={styles.itemTitle}>{t.value}</div>
              {getItems(t.templates)}
            </>
          );
        })}
      </div>
      <div className={styles.btns}>
        <Button
          primary={false}
          className={styles.btn}
          onClick={() => {
            setSelects([]);
            onCancel();
          }}
        >
          取消
        </Button>
        <Button
          className={styles.btn}
          onClick={() => {
            const ret = [...selects];
            setSelects([]);
            onConfirm(ret);
          }}
        >
          确定
        </Button>
      </div>
    </Drawer>
  );
};

Template.defaultProps = {
  visible: false,
  type: 'device',
  filter: null,
  reverse: false,
  onCancel: () => {},
  onConfirm: () => {},
};

Template.propTypes = {
  visible: PropTypes.bool,
  type: PropTypes.string,
  filter: PropTypes.any,
  reverse: PropTypes.bool,
  onCancel: PropTypes.func,
  onConfirm: PropTypes.func,
};

export default Template;
