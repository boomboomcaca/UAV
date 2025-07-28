import React, { useContext } from 'react';
import classnames from 'classnames';
import PropTypes from 'prop-types';
import AppContext from './context';

import styles from './index.module.less';

const MenuItem = (props) => {
  const { children, eventKey, transit } = props;
  const { onClick, selectValue } = useContext(AppContext);

  const onInternalClick = () => {
    onClick(eventKey, transit);
  };

  return (
    <div
      className={classnames(styles.menuitem, { [styles.select]: selectValue === eventKey })}
      onClick={onInternalClick}
    >
      <div className={styles.menuitemct}>{children}</div>
    </div>
  );
};

MenuItem.defaultProps = {
  children: '',
  eventKey: null,
  transit: null,
};

MenuItem.propTypes = {
  children: PropTypes.string,
  transit: PropTypes.any,
  eventKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
};

export default MenuItem;
