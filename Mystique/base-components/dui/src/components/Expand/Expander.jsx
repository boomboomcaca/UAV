import React from 'react';
import PropTypes from 'prop-types';
import ExpanderItem from './ExpanderItem.jsx';
import dropIcon from './dropIcon.jsx';
import styles from './expander.module.less';

const Expander = (props) => {
  const {
    id,
    title,
    children,
    canSelect,
    expand,
    selectIndex,
    dataSource,
    itemTemplate,
    onExpandChanged,
    onItemClick,
  } = props;

  const getRender = () => {
    let ret = null;
    if ((children || dataSource) && expand) {
      if (children) {
        ret = children;
      }
      if (dataSource && itemTemplate) {
        ret = dataSource.map((c, i) => {
          return (
            <ExpanderItem
              key={c.id || c}
              canSelected={canSelect}
              selected={selectIndex === i}
              content={itemTemplate(c, selectIndex === i)}
              onClick={() => {
                onItemClick(dataSource, c, i);
              }}
            />
          );
        });
      }
    }
    return ret;
  };

  return (
    <div className={styles.root} id={id}>
      <div className={styles.header} onClick={onExpandChanged}>
        <div className={styles.title}>{title}</div>
        {dropIcon(expand ? 0 : -90)}
      </div>
      <div>{getRender()}</div>
    </div>
  );
};

Expander.defaultProps = {
  title: null,
  id: null,
  children: null,
  canSelect: true,
  expand: false,
  selectIndex: -1,
  dataSource: null,
  itemTemplate: null,
  onExpandChanged: () => {},
  onItemClick: () => {},
};

Expander.propTypes = {
  title: PropTypes.any,
  id: PropTypes.any,
  children: PropTypes.any,
  canSelect: PropTypes.bool,
  expand: PropTypes.bool,
  selectIndex: PropTypes.number,
  dataSource: PropTypes.array,
  itemTemplate: PropTypes.func,
  onExpandChanged: PropTypes.func,
  onItemClick: PropTypes.func,
};

export default Expander;
