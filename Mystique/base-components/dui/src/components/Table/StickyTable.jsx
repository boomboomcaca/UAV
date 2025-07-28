import React, { useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Empty from '../Empty';
import styles from './sticky.table.module.less';

const StickyTable = (props) => {
  const { className, rowKey, columns, data, border } = props;

  const getStyle = useCallback((tag, sticky) => {
    switch (sticky) {
      case 'left':
        return styles[`sticky${tag}Left`];
      case 'right':
        return styles[`sticky${tag}Right`];
      default:
        return null;
    }
  }, []);

  const getBorder = useCallback((tag, isLast = null, isEnd = null) => {
    if (border) {
      const { type, style } = border;
      if (type === 'all' || tag === type) {
        if (tag === 'outer') {
          return style || { border: '1px solid #ffffff40' };
        }
        return {
          ...style,
          borderTopStyle: 'hidden',
          borderLeftStyle: 'hidden',
          ...(isEnd ? { borderBottomStyle: 'hidden' } : {}),
          ...(isLast ? { borderRightStyle: 'hidden' } : {}),
        };
      }
    }
    return {};
  }, []);

  return (
    <div className={classnames(styles.root, className)} style={getBorder('outer')}>
      <table className={styles.table} cellSpacing={0}>
        {columns ? (
          <thead className={styles.thead}>
            <tr>
              {columns.map((col, idx) => {
                return (
                  <th
                    key={col.key}
                    className={getStyle('H', col.sticky)}
                    style={{ ...col.style, ...getBorder('inner', idx >= columns.length - 1) }}
                  >
                    {col.name}
                  </th>
                );
              })}
            </tr>
          </thead>
        ) : null}
        <tbody className={styles.tbody}>
          {data && columns
            ? data.map((d, index) => {
                return (
                  <tr key={d[rowKey]} className={styles.row}>
                    {columns.map((col, idx) => {
                      return (
                        <td
                          key={col.key}
                          className={getStyle('B', col.sticky)}
                          style={{
                            ...col.style,
                            ...getBorder('inner', idx >= columns.length - 1, index >= data.length - 1),
                          }}
                        >
                          {d[col.key]}
                        </td>
                      );
                    })}
                  </tr>
                );
              })
            : null}
        </tbody>
      </table>
      {data && columns ? null : <Empty className={styles.empty} />}
    </div>
  );
};

StickyTable.defaultProps = {
  className: null,
  rowKey: 'id',
  columns: null,
  data: null,
  border: { type: 'all', style: null },
};

StickyTable.propTypes = {
  className: PropTypes.any,
  rowKey: PropTypes.string,
  columns: PropTypes.any,
  data: PropTypes.any,
  border: PropTypes.exact({
    type: PropTypes.oneOf(['all', 'outer', 'inner']),
    style: PropTypes.any,
  }),
};

export default StickyTable;
