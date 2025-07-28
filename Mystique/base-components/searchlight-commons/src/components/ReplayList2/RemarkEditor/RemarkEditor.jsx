import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Input } from 'dui';
import langT from 'dc-intl';
import { BackIcon, StoreIcon, PenEditIcon } from 'dc-icon';
import styles from './index.module.less';

const RemarkEditor = (props) => {
  const { className, remark, flag, editing, value, onChange, onBack, onStore, onAdd, onEdit } = props;

  return (
    <div className={classnames(styles.root, className)}>
      {editing ? (
        <>
          <Input
            placeholder={langT('commons', 'pleaseInputRemark')}
            style={{ marginRight: 8, width: 120 }}
            maxLength={20}
            allowClear
            value={value}
            onChange={onChange}
          />
          <BackIcon
            color="var(--theme-font-100)"
            iconSize={18}
            onClick={onBack}
            style={{ marginRight: 8, cursor: 'pointer' }}
          />
          <StoreIcon color="var(--theme-font-100)" iconSize={18} onClick={onStore} style={{ cursor: 'pointer' }} />
          {value.length > 10 ? <div className={styles.err}>最多输入10个字符</div> : null}
        </>
      ) : !remark ? (
        <span
          style={{ cursor: 'pointer', color: !flag ? 'rgba(60, 229, 211, 1)' : 'rgba(60, 229, 211, 0.5)' }}
          onClick={onAdd}
        >
          {langT('commons', 'add')}
        </span>
      ) : (
        <>
          <span style={{ marginRight: 8 }}>{remark}</span>
          <div style={{ width: 18, cursor: 'pointer' }}>
            <PenEditIcon
              iconSize={18}
              color={!flag ? 'var(--theme-font-100)' : 'var(--theme-font-50)'}
              onClick={onEdit}
            />
          </div>
        </>
      )}
    </div>
  );
};

RemarkEditor.defaultProps = {
  className: null,
  remark: '',
  flag: false,
  editing: false,
  value: '',
  onChange: () => {},
  onBack: () => {},
  onStore: () => {},
  onAdd: () => {},
  onEdit: () => {},
};

RemarkEditor.propTypes = {
  className: PropTypes.any,
  remark: PropTypes.string,
  flag: PropTypes.bool,
  editing: PropTypes.bool,
  value: PropTypes.string,
  onChange: PropTypes.func,
  onBack: PropTypes.func,
  onStore: PropTypes.func,
  onAdd: PropTypes.func,
  onEdit: PropTypes.func,
};

export default RemarkEditor;
