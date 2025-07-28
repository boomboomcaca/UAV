import React, { useState, memo, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Empty } from 'dui';
import langT from 'dc-intl';
import icons from '../Icon';
import styles from './item.module.less';

const ItemContent = (props) => {
  const { item, toSelect, selected, onChecked, onPreview } = props;

  const { createTime, path2 } = item;

  const [noImg, setNoImg] = useState(false);

  const [checked, setChecked] = useState(selected);

  useEffect(() => {
    setChecked(selected);
  }, [selected]);

  return (
    <div
      className={classnames(styles.item, toSelect && (checked || selected) ? styles.itemchecked : null)}
      onClick={() => {
        if (toSelect) {
          const chk = !checked;
          setChecked(chk);
          onChecked(item, chk);
        }
      }}
    >
      <div
        className={classnames(
          styles.check,
          toSelect ? null : styles.hide,
          toSelect && (checked || selected) ? styles.checked : null,
        )}
      >
        {checked ? icons.check : null}
      </div>
      <div className={styles.time}>
        <div style={{ color: 'var(--theme-font-50)' }}>{langT('commons', 'screenShootTime')}</div>
        <div>{createTime?.replaceAll('-', '.')}</div>
      </div>
      <div
        className={styles.imgdiv}
        onClick={(e) => {
          e.stopPropagation();
        }}
        onDoubleClick={(e) => {
          onPreview(item);
        }}
      >
        {noImg ? (
          <Empty />
        ) : (
          <img
            alt=""
            src={path2}
            width="100%"
            height="100%"
            onError={
              (/* e */) => {
                // window.console.log(e.target.src);
                setNoImg(true);
              }
            }
          />
        )}
      </div>
    </div>
  );
};

ItemContent.defaultProps = {
  item: null,
  toSelect: false,
  selected: false,
  onChecked: () => {},
  onPreview: () => {},
};

ItemContent.propTypes = {
  item: PropTypes.any,
  toSelect: PropTypes.any,
  selected: PropTypes.any,
  onChecked: PropTypes.func,
  onPreview: PropTypes.func,
};

export default memo(ItemContent);
