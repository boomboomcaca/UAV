import React, { useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './pagination.module.less';

const Pagination = (props) => {
  const { current, pageSize, total, className, onChange } = props;

  let Length = useRef(0).current;

  const onItemClick = (tag) => {
    let page = current;
    if (tag === -1) {
      page = current - 1;
    } else if (tag === -2) {
      page = current + 1;
    } else {
      page = tag;
    }
    onChange(page, pageSize);
  };

  const getItems = () => {
    Length = Math.ceil(total / pageSize);
    let arr = [];
    if (Length !== 0) {
      if (Length < 10) {
        arr = [...new Array(Length).keys()].map((itm) => {
          return itm + 1;
        });
      } else {
        // ??
        arr = [1, 2, 3];
        if (current > 3 && current < Length - 2) {
          if (current - 1 === 3) {
            arr.push(4);
            arr.push(5);
            arr.push(6);
            arr.push(-1);
          } else if (current + 1 === Length - 2) {
            arr.push(-1);
            arr.push(Length - 5);
            arr.push(Length - 4);
            arr.push(Length - 3);
          } else {
            arr.push(-1);
            arr.push(current - 1);
            arr.push(current);
            arr.push(current + 1);
            arr.push(-1);
          }
        } else {
          const center = Math.ceil(Length / 2);
          arr.push(-1);
          arr.push(center - 1);
          arr.push(center);
          arr.push(center + 1);
          arr.push(-1);
        }
        arr.push(Length - 2);
        arr.push(Length - 1);
        arr.push(Length);
      }
    }
    return arr.map((itm, idx) => {
      return (
        <div
          key={`${itm}-${idx}`}
          className={classnames(itm === -1 ? styles.omis : styles.item, current === itm ? styles.select : null)}
          onClick={() => {
            itm === -1 ? null : onItemClick(itm);
          }}
        >
          {itm === -1 ? `...` : itm}
        </div>
      );
    });
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div
        className={classnames(styles.item, total === 0 || current === 1 || current === 0 ? styles.disabled : null)}
        onClick={() => {
          onItemClick(-1);
        }}
      >{`<`}</div>
      {getItems()}
      <div
        className={classnames(styles.item, total === 0 || current === Length ? styles.disabled : null)}
        onClick={() => {
          onItemClick(-2);
        }}
      >{`>`}</div>
    </div>
  );
};

Pagination.defaultProps = {
  current: 0,
  pageSize: 20,
  total: 0,
  className: null,
  onChange: () => {},
};

Pagination.propTypes = {
  current: PropTypes.number,
  pageSize: PropTypes.number,
  total: PropTypes.number,
  className: PropTypes.any,
  onChange: PropTypes.func,
};

export default Pagination;
