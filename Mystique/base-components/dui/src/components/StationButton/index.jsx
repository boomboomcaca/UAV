import React from 'react';
import PropTypes from 'prop-types';
import SvgComponent from './SvgComponent/index.jsx';

const StationButton = (props) => {
  const { icon, size, onClick } = props;

  return (
    <SvgComponent size={size} onClick={onClick}>
      {icon}
    </SvgComponent>
  );
};
StationButton.defaultProps = {
  icon: null,
  size: '',
  onClick: null,
};

StationButton.propTypes = {
  icon: PropTypes.element,
  size: PropTypes.string,
  onClick: PropTypes.func,
};
export default StationButton;
