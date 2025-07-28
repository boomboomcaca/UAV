import React from 'react';
import Display from './component/Display.jsx';
import ButtonPanel from './component/ButtonPanel.jsx';
import calculate from './logic/calculate.jsx';
import './Calculator.css';

export default class Calculator extends React.Component {
  state = {
    total: null,
    next: null,
    operation: null,
  };

  constructor(props) {
    super(props);
  }

  handleClick = (buttonName) => {
    this.setState(calculate(this.state, buttonName));
  };

  render() {
    return (
      <div className="component-app" style={this.props.style}>
        <Display value={this.state.next || this.state.total || '0'} />
        <ButtonPanel clickHandler={this.handleClick} />
      </div>
    );
  }
}
