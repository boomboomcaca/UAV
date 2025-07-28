import React from 'react';
import PropTypes from 'prop-types';
import Day from './day.jsx';
import DayTime from './daytime.jsx';

const Single = (props) => {
  const { type, ...ppp } = props;

  return type === 'day' ? <Day {...ppp} /> : <DayTime {...ppp} />;
};

Single.defaultProps = {
  type: 'day',
};

Single.propTypes = {
  type: PropTypes.string,
};

export default Single;
