/* eslint-disable prefer-promise-reject-errors */
import { useRef } from 'react';
import { setValues, getValue } from './utils/valueUtil';

import allPromiseFinish from './utils/asyncUtil';

class FormStore {
  store = {};

  fieldEntities = [];

  initialValues = {};

  callbacks = {};

  getFieldValue = (name) => {
    return this.store[name];
  };

  getFieldsValue = () => {
    return this.store;
  };

  getFieldEntities = () => {
    return this.fieldEntities;
  };

  notifyObservers = (prevStore) => {
    this.getFieldEntities().forEach((entity) => {
      const { onStoreChange } = entity;
      onStoreChange(prevStore, this.getFieldsValue());
    });
  };

  setFieldsValue = (curStore) => {
    const prevStore = this.store;
    if (curStore) {
      this.store = setValues(this.store, curStore);
    }
    this.notifyObservers(prevStore);
  };

  registerField = (entity) => {
    this.fieldEntities.push(entity);
    return () => {
      this.fieldEntities = this.fieldEntities.filter((item) => item !== entity);
      delete this.store[entity.props.name];
    };
  };

  validateFields = () => {
    const promiseList = [];
    this.getFieldEntities().forEach((field) => {
      const { name, rules } = field.props;
      if (!rules || !rules.length) {
        return;
      }
      const promise = field.validateRules();
      promiseList.push(
        promise
          .then(() => ({ name, errors: [] }))
          .catch((errors) =>
            Promise.reject({
              name,
              errors,
            }),
          ),
      );
    });
    const summaryPromise = allPromiseFinish(promiseList);
    const returnPromise = summaryPromise
      .then(() => {
        return Promise.resolve(this.getFieldsValue());
      })
      .catch((results) => {
        const errorList = results.filter((result) => result && result.errors.length);
        return Promise.reject({
          values: this.getFieldsValue(),
          errorFields: errorList,
        });
      });

    // Do not throw in console
    returnPromise.catch((e) => e);

    return returnPromise;
  };

  submit = () => {
    this.validateFields()
      .then((values) => {
        const { onFinish } = this.callbacks;
        if (onFinish) {
          try {
            onFinish(values);
          } catch (err) {
            window.console.error(err);
          }
        }
      })
      .catch((e) => {
        const { onFinishFailed } = this.callbacks;
        if (onFinishFailed) {
          onFinishFailed(e);
        }
      });
  };

  getInitialValue = (namePath) => getValue(this.initialValues, namePath);

  setInitialValues = (initialValues, init) => {
    this.initialValues = initialValues;
    // 初始化的时候，一定要把值同步给store.
    if (init) {
      // setValues 是工具类，不用过多的去关注它，只需要知道它能做什么即可。
      this.store = setValues({}, initialValues, this.store);
    }
  };

  setCallbacks = (callbacks) => {
    this.callbacks = callbacks;
  };

  getForm = () => ({
    getFieldValue: this.getFieldValue,
    getFieldsValue: this.getFieldsValue,
    setFieldsValue: this.setFieldsValue,
    registerField: this.registerField,
    validateFields: this.validateFields,
    submit: this.submit,
    getInternalHooks: () => {
      return {
        setInitialValues: this.setInitialValues,
        setCallbacks: this.setCallbacks,
      };
    },
  });
}

function useForm(form) {
  const formRef = useRef();
  if (!formRef.current) {
    if (form) {
      formRef.current = form;
    } else {
      const formStore = new FormStore();
      formRef.current = formStore.getForm();
    }
  }
  return [formRef.current];
}

export default useForm;
