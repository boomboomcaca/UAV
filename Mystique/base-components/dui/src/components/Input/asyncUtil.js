/* eslint-disable no-await-in-loop */
/* eslint-disable no-async-promise-executor */
import RawAsyncValidator from 'async-validator';

export default async function valid(name, value, rules) {
  const errorList = [];
  for (let i = 0; i < rules.length; i += 1) {
    const errors = await validateRule(name, value, rules[i]);
    if (errors.length) {
      errorList.push(...errors);
    }
  }
  return errorList;
}

const validateRule = async (name, value, rule) => {
  const cloneRule = { ...rule };
  const validator = new RawAsyncValidator({
    [name]: [cloneRule],
  });
  let result = [];
  try {
    await Promise.resolve(validator.validate({ [name]: value }));
  } catch (e) {
    if (e.errors) {
      result = e.errors.map((c) => c.message);
    }
  }
  return result;
};
