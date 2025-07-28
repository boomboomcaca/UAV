import React, { useState } from 'react';
// import { PlusCircleOutlined } from '@ant-design/icons';
import Drawer from '../index';
import Form, { Field } from '../../Form';
import Input from '../../Input';
import styles from './index.module.less';
import Select from '../../Select';

const { Option } = Select;

export default function Demo() {
  const [show, setShow] = useState(false);
  const [form2] = Form.useForm();
  const handleChange = (values) => window.console.log(values);

  return (
    <div>
      <div onClick={() => setShow(true)}>显示Drawer </div>
      <br />
      <br />
      <Drawer title="用户新增" visible={show} onCancel={() => setShow(false)} width="500px">
        <div>
          <div className={styles.modalbody}>
            <Form form={form2} initialValues={{}} layout="vertical" style={{ margin: '0 50px' }}>
              <Field
                name="name"
                label="姓名"
                rules={[
                  {
                    required: true,
                    message: '用户姓名不能为空',
                  },
                ]}
              >
                <Input style={{ width: '320px' }} placeholder="请输入姓名" />
              </Field>
              <Field name="sex" label="性别">
                <Select style={{ width: '320px' }}>
                  <Option value="">请选择</Option>
                  <Option value={1}>男</Option>
                  <Option value={2}>女</Option>
                </Select>
              </Field>
              <Field name="sex2" label="性别">
                <Select style={{ width: '320px' }}>
                  <Option value="">请选择</Option>
                  <Option value={1}>男</Option>
                  <Option value={2}>女</Option>
                </Select>
              </Field>
              <Field
                name="role_id"
                label="角色"
                rules={[
                  {
                    required: true,
                    message: '请选择角色',
                  },
                ]}
              >
                <Select style={{ width: '320px' }}>
                  <Option value="">请选择</Option>
                  <Option value={1}>男</Option>
                  <Option value={2}>女</Option>
                </Select>
              </Field>
              <Field
                name="phone"
                label="手机号码"
                rules={[
                  {
                    required: true,
                    message: '请输入正确的手机号码',
                    pattern: /^[1][3-9][0-9]{9}$/,
                  },
                ]}
              >
                <Input style={{ width: '320px' }} placeholder="请输入手机号码" type="number" />
              </Field>
              <Field name="remark" label="备注">
                <Input style={{ width: '320px' }} placeholder="请输入备注" />
              </Field>
              <Field
                name="password"
                label="登录密码"
                rules={[
                  {
                    message: '请输入登录密码',
                  },
                ]}
              >
                <Input
                  style={{ width: '320px' }}
                  name="password"
                  autoComplete="new-password"
                  placeholder="请输入密码"
                  type="password"
                />
              </Field>
            </Form>
          </div>
        </div>
      </Drawer>
    </div>
  );
}
