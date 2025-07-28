import React from 'react';
import { Table } from 'dui';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import coltemplate from './columnTemplate.jsx';
import styles from './index.module.less';
// import data from './testData';

const MarkGrid = (props) => {
  const { yUnit, type, className, dataSource, selectIndex, onMarkerChanged } = props;

  return (
    <div className={classnames(styles.default, className)}>
      <Table
        rowKey="id"
        showSelection={false}
        columns={coltemplate.columns(yUnit, type, onMarkerChanged)}
        onRowSelected={(row, idx, bo) => {
          onMarkerChanged({ tag: 'select', record: row, more: { idx, select: bo } });
        }}
        selectRowIndex={selectIndex}
        data={dataSource}
        options={{ rowHeight: 40, rowHover: false, canRowSelect: true, bordered: true }}
      />
    </div>
  );
};

MarkGrid.defaultProps = {
  yUnit: 'dBμV',
  type: false,
  className: null,
  dataSource: [],
  selectIndex: -1,
  onMarkerChanged: () => {},
};

MarkGrid.propTypes = {
  yUnit: PropTypes.string,
  type: PropTypes.bool,
  className: PropTypes.any,
  dataSource: PropTypes.array,
  selectIndex: PropTypes.number,
  onMarkerChanged: PropTypes.func,
};

export default MarkGrid;
