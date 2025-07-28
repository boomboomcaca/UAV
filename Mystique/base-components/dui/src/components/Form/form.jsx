import React, { useRef, forwardRef } from 'react';
import PropTypes from 'prop-types';
import FieldContext from './context';
import useForm from './useForm.jsx';
import styles from './styles.module.less';

const Form = forwardRef((props, ref) => {
  const { form, children, initialValues, onFinish, onFinishFailed, labelWidth, layout, ...restProps } = props;
  const [formInstance] = useForm(form);

  const { setInitialValues, setCallbacks } = formInstance.getInternalHooks();

  React.useImperativeHandle(ref, () => formInstance);

  // 第一次渲染时初始化表单的值
  const mountRef = useRef(null);
  setInitialValues(initialValues, !mountRef.current);
  if (!mountRef.current) {
    mountRef.current = true;
  }

  setCallbacks({
    onFinish,
    onFinishFailed,
  });

  return (
    <form
      {...restProps}
      className={styles[layout]}
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();

        formInstance.submit();
      }}
    >
      <FieldContext.Provider value={{ ...formInstance, labelWidth }}>{children}</FieldContext.Provider>
    </form>
  );
});

Form.defaultProps = {
  children: null,
  form: null,
  initialValues: null,
  onFinish: null,
  onFinishFailed: null,
  labelWidth: null,
  layout: 'horizontal',
};

Form.propTypes = {
  children: PropTypes.any,
  form: PropTypes.any,
  initialValues: PropTypes.object,
  onFinish: PropTypes.func,
  onFinishFailed: PropTypes.func,
  labelWidth: PropTypes.string,
  layout: PropTypes.oneOf(['horizontal', 'vertical']),
};

export default Form;
