import FieldForm from './form.jsx';
import Field from './field.jsx';
import useForm from './useForm.jsx';

const Form = FieldForm;
Form.Field = Field;
Form.useForm = useForm;

export { Field };

export default Form;
