import React, { useRef } from 'react';
import PropTypes from 'prop-types';
import { Input, InputNumber, message, Select } from 'dui';
import Button from '@/components/Button';
import Fields, { Field, Group } from '@/components/Fileds';
import Cascader from '@/components/Cascader';
import useArea from '@/hooks/useArea';
import {
  stationType,
  stationaryCategory,
  mobileCategory,
  movableCategory,
  sensorCategory,
  portableCategory,
  airCategory,
  mcsType,
  fmAddrType,
} from '@/hooks/dictKeys';
import styles from './index.module.less';

const { Option } = Select;

const Detail = (props) => {
  const { data, editype, dictionary, onFinish, onChange } = props;

  const { options, zone, onSelectValue } = useArea(data?.areacode); // test: 140108

  const validateFieldsRef = useRef(null);

  const onSave = () => {
    const { current: validateFields } = validateFieldsRef;
    validateFields?.().then((res /* , rej */) => {
      // if (rej) {
      //   message.error(rej.message);
      // }
      if (res) {
        onFinish(res);
      }
    });
  };

  const getStaionTypes = () => {
    const getDict = (key) => {
      return dictionary.find((d) => {
        return d.dicNo === key;
      }).data;
    };
    const type = getDict(stationType);
    const category1 = getDict(stationaryCategory);
    const category2 = getDict(mobileCategory);
    const category3 = getDict(movableCategory);
    const category4 = getDict(sensorCategory);
    const category5 = getDict(portableCategory);
    const category6 = getDict(airCategory);
    if (type) {
      return type.map((t) => {
        let children = null;
        if (t.key === 'stationaryCategory') {
          children = category1;
        }
        if (t.key === 'mobileCategory') {
          children = category2;
        }
        if (t.key === 'movableCategory') {
          children = category3;
        }
        if (t.key === 'sensorCategory') {
          children = category4;
        }
        if (t.key === 'portableCategory') {
          children = category5;
        }
        if (t.key === 'airCategory') {
          children = category6;
        }
        return { ...t, children };
      });
    }
    return null;
  };

  const getAddrOptions = () => {
    const getDict = (key) => {
      return dictionary.find((d) => {
        return d.dicNo === key;
      }).data;
    };
    const types = getDict(fmAddrType);
    return types.map((t) => {
      return <Option value={t.key}>{t.value}</Option>;
    });
  };

  return (
    <div className={styles.root}>
      <div className={styles.form}>
        <Fields
          data={data}
          onInitialized={(func) => {
            validateFieldsRef.current = func;
          }}
          onChange={onChange}
        >
          <Field
            label="站点名称"
            name="name"
            rules={[
              {
                required: true,
                message: '请输入站点名称',
              },
            ]}
          >
            <Input style={{ width: 420 }} placeholder="请输入" />
          </Field>
          <Field
            label="站点编号"
            name="id"
            rules={[
              {
                required: true,
                message: '请输入站点编号',
              },
            ]}
          >
            {editype === 'new' ? (
              <Input style={{ width: 420 }} placeholder="请输入" />
            ) : (
              <div style={{ width: 420 }}>{data.id}</div>
            )}
          </Field>
          <Field
            label="站点序列号"
            name="mfid"
            rules={[
              {
                required: true,
                message: '请输入站点序列号',
              },
            ]}
          >
            {/* <Input style={{ width: 420 }} placeholder="请输入" /> */}
            {editype === 'new' ? (
              <Input style={{ width: 420 }} placeholder="请输入" />
            ) : (
              <div style={{ width: 420 }}>{data.mfid}</div>
            )}
          </Field>
          <Field
            label="站点类型"
            name={['type', 'category']}
            map="key"
            rules={[
              {
                required: true,
                all: true,
                keys: [
                  'stationaryCategory',
                  'mobileCategory',
                  'movableCategory',
                  'sensorCategory',
                  'portableCategory',
                  'airCategory',
                  // 'otherCategory',
                ],
                message: ['请选择站点类型', '请选择二级分类'],
              },
            ]}
          >
            <Cascader style={{ width: 420 }} keyMap={{ KEY: 'key', LABEL: 'value' }} options={getStaionTypes()} />
          </Field>
          <Field
            label="集成厂商"
            name="manufacturer"
            rules={[
              {
                required: true,
                message: '请输入集成厂商',
              },
            ]}
          >
            <Input style={{ width: 420 }} placeholder="请输入" />
          </Field>
          {editype === 'mod' ? (
            <Group label="网络地址" childStyle={{ width: 420 }}>
              <Field
                label="站点IP"
                name="ip"
                // rules={[
                //   {
                //     required: true,
                //     message: '请输入站点IP',
                //   },
                // ]}
              >
                {/* <Input style={{ width: 240 }} placeholder="请输入IP(非必填)" /> */}
                <div style={{ width: 200 }}>{data?.ip || '--'}</div>
              </Field>
              <Field label="端口" name="port">
                {/* <InputNumber style={{ width: 80 }} placeholder="非必填" /> */}
                <div style={{ width: 120 }}>{data?.port || '--'}</div>
              </Field>
            </Group>
          ) : null}
          <Field
            label="地理行政区域"
            name={['areacode', 'zone']}
            map={['code', 'city']}
            rules={[
              {
                required: true,
                message: '请选择地理行政区域',
              },
            ]}
            controlled={() => {
              return {
                transform: (vals) => {
                  if (vals) {
                    let areacode = null;
                    let zoneStr = '';
                    vals.forEach((z, i) => {
                      if (i === vals.length - 1) {
                        areacode = z.code;
                      }
                      zoneStr += `${z.city} `;
                    });
                    return { areacode, zone: zoneStr.substring(0, zoneStr.length - 1) };
                  }
                  return { areacode: null, zone: null };
                },
                control: {
                  values: zone,
                  onSelectValue,
                },
              };
            }}
          >
            <Cascader style={{ width: 420 }} splitter=" " keyMap={{ KEY: 'code', LABEL: 'city' }} options={options} />
          </Field>
          <Field
            label="站址类型"
            name="fmaddrtype"
            rules={[
              {
                required: true,
                message: '请选择站址类型',
              },
            ]}
          >
            <Select style={{ width: 420 }}>{getAddrOptions()}</Select>
          </Field>
          <Group label="地理位置" childStyle={{ width: 420 }}>
            <Field
              label="经度"
              name="longitude"
              rules={[
                {
                  required: true,
                  message: '请设置经度',
                },
              ]}
            >
              <InputNumber style={{ width: 160 }} digits={7} min={-180} max={180} placeholder="请输入" suffix="°" />
            </Field>
            <Field
              label="纬度"
              name="latitude"
              rules={[
                {
                  required: true,
                  message: '请设置纬度',
                },
              ]}
            >
              <InputNumber style={{ width: 160 }} digits={7} min={-90} max={90} placeholder="请输入" suffix="°" />
            </Field>
          </Group>
          <Field
            label="海拔高度"
            name="altitude"
            rules={[
              {
                required: true,
                message: '请设置海拔高度',
              },
            ]}
          >
            <InputNumber style={{ width: 420 }} placeholder="请输入" suffix="m" />
          </Field>
          <Field label="站点地址" name="address">
            <Input style={{ width: 420 }} placeholder="请输入" />
          </Field>
          <Field label="备注信息" name="remark">
            <Input style={{ width: 420 }} placeholder="请输入" />
          </Field>
        </Fields>
      </div>
      <Button className={styles.save} onClick={onSave}>
        保存
      </Button>
    </div>
  );
};

Detail.defaultProps = {
  data: null,
  editype: 'mod',
  dictionary: null,
  onFinish: () => {},
  onChange: () => {},
};

Detail.propTypes = {
  data: PropTypes.any,
  editype: PropTypes.string,
  dictionary: PropTypes.any,
  onFinish: PropTypes.any,
  onChange: PropTypes.any,
};

export default Detail;
