import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { InputNumber } from 'dui';
import styles from './ddd.module.less';

const DDD = (props) => {
  const { className, maximum, minimum, value, onChange } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <InputNumber
        value={value || ''}
        style={{ width: 264, marginRight: 5 }}
        digits={6}
        max={maximum}
        min={minimum}
        suffix="°"
        placeholder="+ DD.DDDDDD"
        onChange={onChange}
      />
    </div>
  );
};

DDD.defaultProps = {
  className: null,
  maximum: 90,
  minimum: -90,
  value: null,
  onChange: () => {},
};

DDD.propTypes = {
  className: PropTypes.any,
  maximum: PropTypes.any,
  minimum: PropTypes.any,
  value: PropTypes.any,
  onChange: PropTypes.func,
};

export default DDD;
