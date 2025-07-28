import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import icons from '../Icon';
import styles from './footer.module.less';

const OperateFooter = (props) => {
  const { className, disabled, onIconClick, allChecked, count, children } = props;

  return (
    <div className={classnames(styles.root, className, disabled ? styles.hide : null)}>
      <div className={styles.tag}>
        <div className={styles.icon} style={{ cursor: 'pointer' }} onClick={onIconClick}>
          {allChecked ? icons.check : icons.checkSome}
        </div>
        <div
          style={{
            cursor: 'pointer',
            color: allChecked ? 'var(--theme-font-100)' : 'var(--theme-font-50)',
          }}
          onClick={onIconClick}
        >
          {langT('commons', 'selectAll')}
        </div>
        <div>
          {langT('commons', 'selected')}
          <span style={{ fontSize: 18, lineHeight: '14px', marginLeft: 2 }}>{count}</span>
          {langT('commons', 'items')}
        </div>
      </div>
      {children}
    </div>
  );
};

OperateFooter.defaultProps = {
  className: null,
  children: null,
  disabled: true,
  allChecked: false,
  count: 0,
  onIconClick: () => {},
};

OperateFooter.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
  disabled: PropTypes.bool,
  allChecked: PropTypes.bool,
  count: PropTypes.number,
  onIconClick: PropTypes.func,
};

export default OperateFooter;
