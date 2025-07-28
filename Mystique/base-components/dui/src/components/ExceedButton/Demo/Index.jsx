import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import ExceedButton from '..';
import styles from './index.module.less';

const Index = (props) => {
  const { className } = props;

  const [value1, setValue1] = useState(2);
  const [value2, setValue2] = useState(2);
  const [value3, setValue3] = useState(2);
  const [checked1, setChecked1] = useState(false);
  const [checked2, setChecked2] = useState(false);

  const [show] = useState(false);
  const [changE, setChangE] = useState('点击按钮获得onChange');

  return (
    <div className={classnames(styles.root, className, show ? null : styles.hide)}>
      <div className={styles.ee}>
        {`${new Date()}`}
        <br />
        <br />
        {`${JSON.stringify(changE)}`}
      </div>
      <div className={styles.col}>
        <ExceedButton
          content="点击"
          showArrow={false}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          disable
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
        />
        <ExceedButton
          content="天线选择"
          label="垂直极化"
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
        />
        <ExceedButton
          content="天线选择"
          label="垂直极化"
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
          waiting={3000}
        />
        <ExceedButton
          content="天线选择"
          label="垂直极化"
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
          waiting
        />
        <ExceedButton
          content="天线选择"
          label="垂直极化"
          disable
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
          }}
        />
      </div>
      <div className={styles.col}>
        <ExceedButton
          content="点击"
          showArrow={false}
          checked={checked1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked1(!checked1);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          disable
          checked={checked1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked1(!checked1);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          checked={checked2}
          indicator={<ExceedButton.RGB type={checked2 ? 'r' : 'rgb'} />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked2(!checked2);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          disable
          checked={checked2}
          indicator={<ExceedButton.RGB type={checked2 ? 'r' : 'rgb'} />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked2(!checked2);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          checked={checked2}
          indicator={<ExceedButton.RGB type={checked2 ? 'g' : 'rgb'} />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked2(!checked2);
          }}
        />
        <ExceedButton
          content="点击"
          showArrow={false}
          checked={checked2}
          indicator={<ExceedButton.RGB type={checked2 ? 'b' : 'rgb'} />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            setChecked2(!checked2);
          }}
        />
      </div>
      <div className={styles.col}>
        <ExceedButton
          content="切换按钮"
          showArrow={false}
          indicator={<ExceedButton.Switch />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue3(e.args.value);
            }
          }}
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
          ]}
          value={value3}
          switchTrigger={3}
        />
        <ExceedButton
          content="切换按钮"
          showArrow={false}
          disable
          indicator={<ExceedButton.Switch />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue3(e.args.value);
            }
          }}
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
          ]}
          value={value3}
          switchTrigger={3}
        />
        <ExceedButton
          content="切换按钮"
          showArrow={false}
          indicator={<ExceedButton.Switch />}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue3(e.args.value);
            }
          }}
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
          ]}
          value={value3}
          switchTrigger={2}
        />
      </div>
      <div className={styles.col}>
        <ExceedButton
          content="选项按钮"
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
            { label: '选项第四', value: 4 },
            { label: '选项第五', value: 5 },
            { label: '选项第六', value: 6 },
            { label: '选项第七', value: 7 },
            { label: '选项第八', value: 8 },
            { label: '选项第九', value: 9 },
            { label: '选项第十', value: 10 },
          ]}
          value={value1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue1(e.args.value);
            }
          }}
        />
        <ExceedButton
          content="选项按钮"
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
            { label: '选项第四', value: 4 },
            { label: '选项第五', value: 5 },
            { label: '选项第六', value: 6 },
            { label: '选项第七', value: 7 },
            { label: '选项第八', value: 8 },
            { label: '选项第九', value: 9 },
            { label: '选项第十', value: 10 },
          ]}
          disable
          value={value1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue1(e.args.value);
            }
          }}
        />
        <ExceedButton
          content="选项按钮"
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
            { label: '选项第四', value: 4 },
            { label: '选项第五', value: 5 },
            { label: '选项第六', value: 6 },
            { label: '选项第七', value: 7 },
            { label: '选项第八', value: 8 },
            { label: '选项第九', value: 9 },
            { label: '选项第十', value: 10 },
          ]}
          // disable
          checked={[1, 2, 3].includes(value1)}
          value={value1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue1(e.args.value);
            }
          }}
        />
        <ExceedButton
          content="选项按钮"
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
            { label: '选项第四', value: 4 },
            { label: '选项第五', value: 5 },
            { label: '选项第六', value: 6 },
            { label: '选项第七', value: 7 },
            { label: '选项第八', value: 8 },
            { label: '选项第九', value: 9 },
            { label: '选项第十', value: 10 },
          ]}
          disable
          checked={[1, 2, 3].includes(value1)}
          value={value1}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue1(e.args.value);
            }
          }}
        />
      </div>
      <div className={styles.col}>
        <ExceedButton
          content="选项按钮"
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
          ]}
          openIfUnchecked
          checked={[1, 2, 3].includes(value2)}
          value={value2}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue2(e.args.value);
            }
            if (e.event === 'uncheck') {
              setValue2(null);
            }
          }}
        />
        <ExceedButton
          content="选项按钮"
          disable
          options={[
            { label: '选项第一', value: 1 },
            { label: '选项第二', value: 2 },
            { label: '选项第三', value: 3 },
          ]}
          openIfUnchecked
          checked={[1, 2, 3].includes(value2)}
          value={value2}
          onChange={(e) => {
            window.console.log(e);
            setChangE(e);
            if (e.event === 'valueChange') {
              setValue2(e.args.value);
            }
            if (e.event === 'uncheck') {
              setValue2(null);
            }
          }}
        />
      </div>
    </div>
  );
};

Index.defaultProps = {
  className: null,
};

Index.propTypes = {
  className: PropTypes.any,
};

export default Index;
