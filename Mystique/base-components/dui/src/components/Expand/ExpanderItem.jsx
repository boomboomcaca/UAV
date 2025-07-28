import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './item.module.less';

const ExpanderItem = (props) => {
  const { content, style, selected, canSelected, onClick } = props;
  return (
    <div
      className={classnames(
        styles.item,
        canSelected ? styles.itemCanSel : null,
        canSelected && selected ? styles.select : null,
      )}
      style={style}
      onClick={onClick}
    >
      {content}
    </div>
  );
};

ExpanderItem.defaultProps = { content: null, style: null, selected: false, canSelected: true, onClick: () => {} };

ExpanderItem.propTypes = {
  content: PropTypes.any,
  style: PropTypes.any,
  selected: PropTypes.bool,
  canSelected: PropTypes.bool,
  onClick: PropTypes.func,
};

export default ExpanderItem;
