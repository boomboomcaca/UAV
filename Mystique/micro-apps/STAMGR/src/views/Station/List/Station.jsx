import React from 'react';
import PropTypes from 'prop-types';
import { Input, Table, Button, Empty, message, Modal } from 'dui';
import { FilterIcon, DeleteIcon, TreeMapIcon, AddIcon, SettingIcon } from 'dc-icon';
import { restartEdge, getList } from '@/api/cloud';
import useDictionary from '@/hooks/useDictionary';
import useStationList from '@/hooks/useStationList';
import useStationFilter from '@/hooks/useStaionFilter';
import ContentButton from '@/components/ContentButton';
import ClosableButton from '@/components/ClosableButton';
import Filter from '@/components/Filter';
import Filters from '@/components/Filters';
import { deviceUrl } from '@/api/path';
import getColumns from '../Columns/columns.jsx';
import styles from './index.module.less';

const Station = (props) => {
  const { onStationClick, refreshKey, onShowTemplate } = props;

  const { dictionary, getDictValue } = useDictionary();

  const {
    attachKeys,
    refresh,
    updateParams,
    stations,
    selectStations,
    setSelectStations,
    onDelete,
    groupName,
    setGroupName,
    showGroup,
    setShowGroup,
    onGroup,
  } = useStationList(dictionary, getDictValue, refreshKey);

  const {
    showFilter,
    setShowFilter,
    categoryfilters,
    zonefilters,
    groupfilters,
    statefilters,
    onCategoryChanged,
    categoryValues,
    onZoneChanged,
    zoneValues,
    onGroupChanged,
    groupValues,
    onStateChanged,
    stateValues,
    Reset,
    Confrim,
    onActiveFilter,
    onDisposeFilter,
  } = useStationFilter(stations);

  const onEdit = (d) => {
    const argd = { ...d };
    attachKeys.forEach((ak) => {
      delete argd[ak];
    });
    onStationClick({ tag: 'mod', args: d });
  };

  const onCopy = (d) => {
    const argd = { ...d };
    attachKeys.forEach((ak) => {
      delete argd[ak];
    });
    delete argd.id;
    delete argd.mfid;
    onStationClick({ tag: 'new', args: argd });
  };

  const onActive = (d) => {
    window.console.log(d);
  };

  const onRestart = async (d) => {
    window.console.log(d);
    const p = { edgeId: d.id };
    // const res1 = await getList(deviceUrl, {
    //   page: 1,
    //   rows: 1000,
    //   sort: 'desc',
    //   order: 'createTime',
    //   moduleType: 'device',
    //   edgeId: d.id,
    // });
    // if (res1.result) {
    //   const find = res1.result.find((r) => {
    //     return r.moduleCategory[0] === 'control';
    //   });
    //   if (find) {
    //     p.deviceId = find.id;
    //   }
    // }
    const res2 = await restartEdge(p);
    if (res2) {
      message.success({ key: 'tip', content: '发送重启指令成功' });
    }
  };

  const onFiltersClick = (tag) => {
    if (tag === 'reset') {
      Reset();
    }
    if (tag === 'confirm') {
      Confrim();
      setShowFilter(false);
    }
  };

  const getStations = () => {
    if (
      stations &&
      ((zoneValues.length > 0 &&
        zoneValues.find((v) => {
          return v.active === true;
        })) ||
        (categoryValues.length > 0 &&
          categoryValues.find((v) => {
            return v.active === true;
          })) ||
        (groupValues.length > 0 &&
          groupValues.find((v) => {
            return v.active === true;
          })) ||
        (stateValues.length > 0 &&
          stateValues.find((v) => {
            return v.active === true;
          })))
    ) {
      return stations.filter((s) => {
        return (
          categoryValues.find((v) => {
            return v.value === s.categoryStr && v.active === true;
          }) ||
          zoneValues.find((v) => {
            return v.value === s.zone && v.active === true;
          }) ||
          groupValues.find((v) => {
            return v.value === s.groupName && v.active === true;
          }) ||
          stateValues.find((v) => {
            return v.value === s.stateStr && v.active === true;
          })
        );
      });
    }
    return stations;
  };

  return (
    <div className={styles.root}>
      <div className={styles.query}>
        <Input
          allowClear
          showSearch
          onSearch={(str) => updateParams({ 'name.lk': str })}
          onPressEnter={(str) => updateParams({ 'name.lk': str })}
          onChange={(val) => {
            if (val === '') {
              updateParams({ 'name.lk': null });
            }
          }}
          placeholder="搜索"
          style={{ width: 260 }}
        />
        <ClosableButton.Group className={styles.filters}>
          {[...zoneValues, ...categoryValues, ...groupValues, ...stateValues].map((v) => {
            return (
              <ClosableButton
                key={v.id}
                content={v.value === '' || v.value === undefined ? `${v.title}-无` : v.value}
                activate={v.active || false}
                onActive={() => {
                  onActiveFilter(v);
                }}
                onClose={() => {
                  onDisposeFilter(v);
                }}
              />
            );
          })}
        </ClosableButton.Group>
        <div className={styles.operate}>
          <Button onClick={refresh}>刷新</Button>
          <ContentButton icon={<AddIcon />} text="新增" onClick={onCopy} />
          <ContentButton
            icon={<FilterIcon />}
            text="筛选"
            onClick={() => {
              setShowFilter(true);
            }}
          />
          <ContentButton
            icon={<TreeMapIcon />}
            text="分组"
            disabled={selectStations === null || selectStations.length === 0}
            onClick={() => {
              setShowGroup(true);
            }}
          />
          <ContentButton
            icon={<DeleteIcon />}
            text="删除"
            disabled={selectStations === null || selectStations.length === 0}
            onClick={onDelete}
          />
          <ContentButton
            icon={<SettingIcon />}
            text="模板"
            // disabled={selectStations === null || selectStations.length === 0}
            onClick={(e) => {
              if (onShowTemplate) {
                onShowTemplate();
              }
            }}
          />
        </div>
      </div>
      <div className={styles.list}>
        <Table
          columns={getColumns(onEdit, onCopy, onActive, onRestart)}
          data={getStations()}
          onSelectionChanged={setSelectStations}
        />
        {stations.length === 0 ? <Empty className={styles.empty} /> : null}
      </div>

      <Filters visible={showFilter} title="站点筛选" onCancel={() => setShowFilter(false)} onClick={onFiltersClick}>
        <Filter title="站点类型" filters={categoryfilters} values={categoryValues} onChanged={onCategoryChanged} />
        <Filter title="行政区划" filters={zonefilters} values={zoneValues} onChanged={onZoneChanged} />
        <Filter title="分组" filters={groupfilters} values={groupValues} onChanged={onGroupChanged} />
        <Filter title="状态" filters={statefilters} values={stateValues} onChanged={onStateChanged} />
      </Filters>

      <Modal
        visible={showGroup}
        title="站点分组"
        closable={false}
        onCancel={() => {
          setShowGroup(false);
        }}
        onOk={() => {
          onGroup();
          setShowGroup(false);
        }}
      >
        <Input
          style={{ width: '100%' }}
          value={groupName}
          onChange={(val) => {
            setGroupName(val);
          }}
          placeholder="请输入分组名"
        />
      </Modal>
    </div>
  );
};

Station.defaultProps = {
  onStationClick: () => {},
  onShowTemplate: () => {},
  refreshKey: 0,
};

Station.propTypes = {
  onStationClick: PropTypes.func,
  onShowTemplate: PropTypes.func,
  refreshKey: PropTypes.any,
};

export default Station;
