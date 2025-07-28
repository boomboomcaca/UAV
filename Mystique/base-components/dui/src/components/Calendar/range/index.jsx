import React from 'react';
import PropTypes from 'prop-types';
import Day from './day.jsx';
import DayTime from './daytime.jsx';

const Range = (props) => {
  const { type, ...ppp } = props;

  return type === 'day' ? <Day {...ppp} /> : <DayTime {...ppp} />;
};

Range.defaultProps = {
  type: 'day',
};

Range.propTypes = {
  type: PropTypes.string,
};

export default Range;
