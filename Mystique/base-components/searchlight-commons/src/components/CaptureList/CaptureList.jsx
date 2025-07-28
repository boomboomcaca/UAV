import React, { useMemo, useRef, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import { Input, Button, Modal, ListView, PopUp, Loading } from 'dui';
import { CloseIcon, FinishIcon } from 'dc-icon';
import OperateFooter from './OperateFooter';
import icons from './Icon';
import ItemContent from './Item/ItemContent.jsx';
import styles from './index.module.less';

const CaptureList = (props) => {
  const {
    baseUrl,
    onLoadMore,
    dataSource,
    style,
    className,
    onSearchChanged,
    onNameChanged,
    onDelete,
    onDownload,
    deleting,
    downloading,
    onRefresh,
  } = props;

  const selectItemsRef = useRef([]);

  const [curSels, setCurSels] = useState(null);

  const [disabled, setDisabled] = useState(true);
  const [isSelectMode, setIsSelectMode] = useState(true);

  const [visible, setVisible] = useState(false);
  const [editItem, setEditItem] = useState(null);

  const selectItemRef = useRef(null);
  const [showPreview, setShowPreview] = useState(false);
  const onPreview = (item) => {
    selectItemRef.current = item;
    setShowPreview(true);
  };

  const [searchText, setSearchText] = useState(undefined);
  useEffect(() => {
    if (searchText !== undefined) {
      onSearchChanged(searchText);
    }
  }, [searchText]);

  useEffect(() => {
    selectItemsRef.current = [];
    setDisabled(true);
  }, [dataSource]);

  const onDeleteSelect = () => {
    if (selectItemsRef.current.length > 0) {
      onDelete(selectItemsRef.current);
    }
  };
  const onDeleteConfirm = () => {
    const iselect = !isSelectMode;
    setIsSelectMode(iselect);
    if (iselect) {
      setDisabled(selectItemsRef.current.length === 0);
    } else {
      setDisabled(true);
    }
  };
  const onCancel = () => {
    setVisible(false);
    editItem.newName = editItem.name;
  };
  const onConfirm = () => {
    setVisible(false);
    window.console.log(editItem);
    if (editItem.newName && editItem.newName !== editItem.name) {
      onNameChanged(editItem, (bo) => {
        window.console.log(bo);
      });
    }
  };

  const onEditName = (item) => {
    setEditItem(item);
    setVisible(true);
  };

  const onItemChecked = (item, chk) => {
    if (chk) {
      selectItemsRef.current.push(item);
    } else {
      const has = selectItemsRef.current.find((i) => {
        return i.id === item.id;
      });
      if (has) {
        selectItemsRef.current.splice(selectItemsRef.current.indexOf(has), 1);
      }
    }
    setCurSels([...selectItemsRef.current]);
    setDisabled(selectItemsRef.current.length === 0);
  };

  const onAllChecked = (chk) => {
    if (chk) {
      selectItemsRef.current = dataSource.map((d) => {
        return { ...d, id: d.id, path2: `${baseUrl}/${d.path}` };
      });
    } else {
      selectItemsRef.current = [];
    }
    setCurSels([...selectItemsRef.current]);
    setDisabled(selectItemsRef.current.length === 0);
  };

  const getList = useMemo(() => {
    return (
      <ListView
        className={styles.list}
        baseSize={{ width: 380, height: 'auto' }}
        loadMore={onLoadMore}
        dataSource={dataSource}
        itemTemplate={(item) => {
          let fixedItem = item;
          if (item.path) {
            fixedItem = { ...item, id: item.id, path2: `${baseUrl}/${item.path}` };
          }
          return (
            <ItemContent
              item={fixedItem}
              toSelect={isSelectMode}
              selected={selectItemsRef.current.find((s) => {
                return s.id === fixedItem.id;
              })}
              onEditName={onEditName}
              onChecked={onItemChecked}
              onPreview={onPreview}
            />
          );
        }}
      />
    );
  }, [dataSource, isSelectMode, curSels]);

  const onDeleteCancel = () => {
    // TODO 取消
    selectItemsRef.current = [];
    setCurSels(null);
    setDisabled(true);
  };

  const onDownloadSelect = () => {
    // TODO 下载图片
    if (selectItemsRef.current.length > 0) {
      onDownload(selectItemsRef.current);
    }
  };

  return (
    <div className={classnames(styles.root, className)} style={style}>
      <div className={styles.tool}>
        <Input
          allowClear
          showSearch
          onSearch={(str) => setSearchText(str)}
          onPressEnter={(str) => setSearchText(str)}
          onChange={(val) => {
            if (val === '') {
              setSearchText('');
            }
          }}
          placeholder={langT('commons', 'inputHolderSearch')}
          style={{ width: 260, position: 'absolute', left: 0 }}
        />
        <Button disabled={isSelectMode} onClick={onRefresh}>
          {langT('commons', 'refresh')}
        </Button>
        <Button disabled={disabled} onClick={onDeleteSelect}>
          <div className={styles.idel}>
            {icons.remove(disabled ? 0.2 : 1)}
            {langT('commons', 'delete')}
          </div>
        </Button>
        <Button onClick={onDeleteConfirm}>
          {isSelectMode ? langT('commons', 'finish') : langT('commons', 'select')}
        </Button>
      </div>
      {getList}

      <Modal
        visible={visible}
        title={langT('commons', 'editScreenshotName')}
        style={{ top: '50%', transform: 'translateY(-50%)', width: '420px' }}
        onCancel={onCancel}
        onOk={onConfirm}
      >
        <Input
          defaultValue={editItem?.name}
          onChange={(val) => {
            editItem.newName = val;
          }}
          style={{ width: '100%' }}
        />
      </Modal>

      <PopUp visible={showPreview} popupTransition="rtg-fade" usePortal mask={false}>
        <div className={styles.imgMax}>
          <div className={styles.image}>
            <img alt="" src={selectItemRef.current?.path2} />
          </div>
          <div className={styles.btnMax}>
            <div
              className={styles.okMax}
              onClick={() => {
                onDownload([{ ...selectItemRef.current }]);
                selectItemRef.current = null;
                setShowPreview(false);
              }}
            >
              <FinishIcon />
            </div>
            <div
              className={styles.closeMax}
              onClick={() => {
                selectItemRef.current = null;
                setShowPreview(false);
              }}
            >
              <CloseIcon />
            </div>
          </div>
        </div>
      </PopUp>

      <OperateFooter
        disabled={disabled}
        count={curSels?.length || 0}
        allChecked={dataSource?.length === curSels?.length}
        onIconClick={() => {
          onAllChecked(dataSource?.length !== curSels?.length);
        }}
      >
        <Button disabled={disabled || deleting || downloading} onClick={onDownloadSelect}>
          <div className={styles.idel}>
            {icons.download(disabled || deleting || downloading ? 0.2 : 1)}
            {langT('commons', 'download')}
            {downloading ? <Loading className={styles.loading} /> : null}
          </div>
        </Button>
        <Button disabled={disabled || deleting || downloading} onClick={onDeleteSelect}>
          <div className={styles.idel}>
            {icons.remove(disabled || deleting || downloading ? 0.2 : 1)}
            {langT('commons', 'delete')}
            {deleting ? <Loading className={styles.loading} /> : null}
          </div>
        </Button>
        <Button onClick={onDeleteCancel}>{langT('commons', 'cancel')}</Button>
      </OperateFooter>
    </div>
  );
};

CaptureList.defaultProps = {
  baseUrl: '',
  dataSource: null,
  style: null,
  className: null,
  onLoadMore: () => {},
  onSearchChanged: () => {},
  onNameChanged: () => {},
  onDelete: () => {},
  onDownload: () => {},
  deleting: false,
  downloading: false,
  onRefresh: () => {},
};

CaptureList.propTypes = {
  baseUrl: PropTypes.string,
  dataSource: PropTypes.any,
  style: PropTypes.any,
  className: PropTypes.any,
  onLoadMore: PropTypes.func,
  onSearchChanged: PropTypes.func,
  onNameChanged: PropTypes.func,
  onDelete: PropTypes.func,
  onDownload: PropTypes.func,
  deleting: PropTypes.bool,
  downloading: PropTypes.bool,
  onRefresh: PropTypes.func,
};

export default CaptureList;
