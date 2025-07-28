import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Radio, Select, Input, InputNumber } from 'dui';
import { PropertyType } from './weapon';

const ListItem = (props) => {
  const { param, styled, disabled, onValueChanged } = props;

  const [value, setValue] = useState(undefined);

  useEffect(() => {
    if (param) {
      setValue(param.value);
    }
  }, [param]);

  const OnTextChanged = (val) => {
    setValue(val);
    onValueChanged(param, val);
  };

  const OnValueChanged = (val) => {
    const v = val === '' ? param.value || param.default : val;
    if (v !== val) {
      setValue(undefined);
    }
    setValue(v);
    onValueChanged(param, v);
  };

  const getElem = () => {
    let elem = null;
    const type = ListItem.getType(param);
    switch (type) {
      case PropertyType.TEXT:
        elem = (
          <Input
            placeholder={param.description}
            suffix={param.unit}
            value={value}
            onChange={(val, error) => {
              window.console.log(val);
              window.console.log(error);
              OnTextChanged(val);
            }}
            rules={[
              {
                required: param.required,
                message: `请输入${param.displayName}`,
              },
            ]}
            style={styled ? { marginBottom: '10px', width: 200 } : { width: 150 }}
          />
        );
        break;
      case PropertyType.RANGE_INTEGER:
        elem = (
          <div>
            <InputNumber
              min={param.minimum === '' ? null : param.minimum}
              max={param.maximum === '' ? null : param.maximum}
              suffix={param.unit}
              value={value}
              step={1}
              digits={0}
              onChange={OnValueChanged}
              style={styled ? { width: 200 } : { width: 150 }}
            />
          </div>
        );
        break;
      case PropertyType.RANGE_DECIMAL:
        elem = (
          <div>
            <InputNumber
              min={param.minimum === '' ? null : param.minimum}
              max={param.maximum === '' ? null : param.maximum}
              suffix={param.unit}
              value={value}
              step={0.5}
              digits={3}
              onChange={OnValueChanged}
              style={styled ? { width: 200 } : { width: 150 }}
            />
          </div>
        );
        break;
      case PropertyType.SELECTION_SHORT: {
        const options = param.values.map((v, j) => {
          return { label: param.displayValues[j], value: v };
        });
        elem = <Radio options={options} value={value} onChange={OnValueChanged} />;
        break;
      }
      case PropertyType.SELECTION_LONG: {
        const options = param.values.map((v, j) => {
          return {
            label: param.displayValues[j],
            value: v,
          };
        });
        elem = (
          <Select value={value} onChange={OnValueChanged} style={styled ? { width: 200 } : { width: 150 }}>
            {options.map((item) => (
              <Select.Option value={item.value} key={item.value}>
                {item.label}
              </Select.Option>
            ))}
          </Select>
        );
        break;
      }
      case PropertyType.BOOL:
        // handle outside
        break;
      case PropertyType.LIST:
        // handle outside
        break;
      default:
        break;
    }
    return elem;
  };

  return (
    <div
      style={
        !disabled
          ? null
          : {
              pointerEvents: 'none',
              opacity: '0.5',
            }
      }
    >
      {getElem()}
    </div>
  );
};

ListItem.defaultProps = {
  param: null,
  styled: true,
  disabled: false,
  onValueChanged: () => {},
};

ListItem.propTypes = {
  param: PropTypes.any,
  styled: PropTypes.bool,
  disabled: PropTypes.bool,
  onValueChanged: PropTypes.func,
};

ListItem.getType = (item) => {
  let itemType = PropertyType.TEXT;
  if (item.template === null || item.template.length === 0) {
    if (item.values !== null && item.values.length > 0) {
      if (item.values.length < 5) {
        itemType = PropertyType.SELECTION_SHORT;
      } else {
        itemType = PropertyType.SELECTION_LONG;
      }
      if (item.type === 5 || item.type === 'bool') {
        itemType = PropertyType.BOOL;
      }
    } else {
      if (
        item.type === 1 ||
        item.type === 2 ||
        item.type === 'float' ||
        item.type === 'double' ||
        item.type === 'number'
      ) {
        itemType = PropertyType.RANGE_DECIMAL;
      }
      if (item.type === 3 || item.type === 4 || item.type === 'int') {
        itemType = PropertyType.RANGE_INTEGER;
      }
      if (item.type === 5 || item.type === 'bool') {
        itemType = PropertyType.BOOL;
      }
    }
  } else {
    itemType = PropertyType.LIST;
  }
  return itemType;
};

export default ListItem;
