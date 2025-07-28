import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Options from './plugins/Options';
import Indicator from './plugins/Indicator';
import Arrow from './plugins/Arrow';
import Label from './plugins/Label';
import Loading from './plugins/Loading';
import useExceedButton from './useExceedButton';
import styles from './eb.module.less';

const ExceedButton = (props) => {
  const {
    className,
    disable,
    checked,
    waiting,
    content,
    options,
    value,
    label,
    tooltip,
    indicator,
    showArrow,
    onChange,
    onChangeTrigger,
    openIfUnchecked,
    switchTrigger,
  } = props;

  const { rootRef, popupRef, showOptions, valueLabel, loading, onRootClick, onItemClick } = useExceedButton(
    checked,
    disable,
    waiting,
    value,
    options,
    onChange,
    onChangeTrigger,
    switchTrigger,
  );

  const fShowIndicator = () => {
    return (checked !== null && checked !== undefined) || indicator;
  };

  const fShowLabel = () => {
    return options || label;
  };

  const fShowArrow = () => {
    return showArrow;
  };

  return (
    <div
      ref={rootRef}
      title={tooltip}
      className={classnames(styles.root, className)}
      onClick={() => {
        if (checked && openIfUnchecked && checked !== null && checked !== undefined) {
          onChange({ event: 'uncheck' });
        } else {
          onRootClick();
        }
      }}
    >
      <div className={classnames(styles.content, disable ? styles.disable : null, checked ? styles.checked : null)}>
        <span>{content}</span>
        {fShowArrow() && <Arrow disable={disable} opened={showOptions} />}
        {fShowLabel() && <Label disable={disable} label={label || valueLabel} />}
        {fShowIndicator() && <Indicator disable={disable} checked={checked} indicator={indicator} />}
      </div>
      {options && (
        <Options ref={popupRef} visible={showOptions} options={options} value={value} onItemClick={onItemClick} />
      )}
      {loading ? <Loading className={styles.loading} /> : null}
    </div>
  );
};

ExceedButton.defaultProps = {
  className: null,
  disable: false,
  checked: null,
  waiting: false,
  content: null,
  options: null,
  value: null,
  label: null,
  tooltip: null,
  indicator: null,
  showArrow: true,
  onChange: () => {},
  onChangeTrigger: 'valueChange', // valueChange,itemClick
  openIfUnchecked: false,
  switchTrigger: -1,
};

ExceedButton.propTypes = {
  className: PropTypes.any,
  disable: PropTypes.bool,
  checked: PropTypes.any,
  waiting: PropTypes.any,
  content: PropTypes.any,
  options: PropTypes.any, // label,value
  value: PropTypes.any,
  label: PropTypes.any,
  tooltip: PropTypes.any,
  indicator: PropTypes.any,
  showArrow: PropTypes.bool,
  onChange: PropTypes.func,
  onChangeTrigger: PropTypes.any,
  openIfUnchecked: PropTypes.bool,
  switchTrigger: PropTypes.number,
};

export default ExceedButton;
