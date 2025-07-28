import React from 'react';
import Form, { Field } from '../index';
import Input from '../../Input/index';
import Select from '../../Select/index';

const { Option } = Select;

export default function Demo() {
  const [form] = Form.useForm();
  const [form2] = Form.useForm();

  const handleChange = (values) => window.console.log(values);

  return (
    <div>
      <span>horizontal</span>
      <br />
      <br />
      <Form form={form} onFinish={handleChange} labelWidth="100px">
        <Field label="射频衰减">
          <Field
            name="aaa"
            rules={[
              {
                required: true,
                message: '请选择解调模式',
              },
            ]}
          >
            <Input suffix="dB" />
          </Field>
          -
          <Field name="bbb" rules={[{ required: true, message: '请输入射频衰减' }]}>
            <Input suffix="dB" />
          </Field>
        </Field>
        <Field
          name="a2"
          label="中频衰减"
          rules={[
            { required: true, message: '请输入中频衰减' },
            { type: 'integer', transform: Number, message: '请输入整数' },
          ]}
        >
          <Input suffix="dB" />
        </Field>
        <Field name="a3" label="解调模式" rules={[{ required: true, message: '请选择解调模式' }]}>
          <Select onChange={(value) => console.log('-----', value)}>
            <Option value="">所有类型</Option>
            <Option value="aaa">FM</Option>
            <Option value="bbb">222</Option>
            <Option value="ccc">333</Option>
            <Option value="ddd">444</Option>
            <Option value="eee">555</Option>
            <Option value="fff">666</Option>
            <Option value="ggg">777</Option>
          </Select>
        </Field>
        <Field labelOffset>
          <button type="submit">提交</button>
        </Field>
        <Field labelOffset="10px">
          <button type="submit">取消</button>
        </Field>
      </Form>
      <br />
      <br />
      <br />
      <span>vertical</span>
      <br />
      <br />
      <Form form={form2} onFinish={handleChange} layout="vertical">
        <Field
          name="a4"
          label="静噪门限1"
          rules={[{ required: true, message: '请输入静噪门限', pattern: '^[\\S\n\\s]{1,150}$' }]}
        >
          <Input suffix="dBμV" onChange={(value) => console.log('-----', value)} />
        </Field>
        <Field name="a5" label="静噪门限" rules={[{ required: true, message: '请输入静噪门限' }]}>
          <Input type="password" />
        </Field>
        <Field name="a6" rules={[{ required: true, message: '请输入静噪门限' }]}>
          <Input
            style={{ width: '320px' }}
            name="password"
            autoComplete="new-password"
            placeholder="请输入密码"
            type="password"
            rules={[
              {
                required: true,
                message: '请输入登陆密码',
              },
            ]}
          />
        </Field>
        <Field name="password">
          <button type="submit">提交</button>
        </Field>
      </Form>
    </div>
  );
}
