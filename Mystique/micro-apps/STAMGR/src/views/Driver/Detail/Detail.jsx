import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Input, Empty, Button } from 'dui';
import { PropertyList } from 'searchlight-commons';
import Popup from '@/components/Popup';
import Fields, { Field } from '@/components/Fileds';
import styles from './detail.module.less';

const Detail = (props) => {
  const { className, data, onChange } = props;

  const [show, setShow] = useState(false);

  const onFieldChange = (name, value) => {
    onChange({ key: name, value });
  };

  const onParamsChanged = (_params, name, newVal /* , oldVal */) => {
    if (data && data.parameters) {
      const find = data.parameters.find((p) => {
        return p.name === name;
      });
      if (find) {
        find.value === newVal;
        onChange({ key: 'parameters', value: data.parameters });
      }
    }
  };

  return (
    <>
      <div className={styles.title}>
        {!show ? (
          '基本设置'
        ) : (
          <>
            高级设置
            <div
              className={styles.close}
              onClick={() => {
                setShow(false);
              }}
            />
          </>
        )}
      </div>
      <div className={classnames(styles.root, className)}>
        {data ? (
          <Fields data={data} labelStyle={{ width: 45 }} onChange={onFieldChange}>
            <Field label="型号" name="model" />
            <Field label="类型" name="moduleCategoryStr" />
            <Field
              label="名称"
              name="displayName"
              rules={[
                {
                  required: true,
                  message: '请输入名称',
                },
              ]}
            >
              <Input style={{ width: 220 }} placeholder="请输入" />
            </Field>
            <Field label="备注" name="description">
              <Input style={{ width: 600 }} placeholder="请输入" />
            </Field>
          </Fields>
        ) : (
          <Empty className={styles.empty} message="未选择功能" />
        )}
        {data ? (
          <Button
            onClick={() => {
              setShow(true);
            }}
            style={{ marginLeft: 60, marginTop: 45 }}
          >
            参数预览
          </Button>
        ) : null}
        <Popup visible={show} usePortal={false} PopupTransition="rtg-fade">
          <PropertyList filter="notinstall" disabled params={data?.parameters} OnParamsChanged={onParamsChanged} />
        </Popup>
      </div>
    </>
  );
};

Detail.defaultProps = {
  className: null,
  data: null,
  onChange: () => {},
};

Detail.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
  onChange: PropTypes.func,
};

export default Detail;
