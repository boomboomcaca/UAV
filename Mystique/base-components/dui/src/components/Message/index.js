import Message, { ShowType } from './Message.jsx';

const success = (msg) => Message.show(ShowType.SUCCESS, msg);
const info = (msg) => Message.show(ShowType.INFO, msg);
const warning = (msg) => Message.show(ShowType.WARNING, msg);
const warn = (msg) => Message.show(ShowType.WARNING, msg);
const error = (msg) => Message.show(ShowType.ERROR, msg);
const loading = (msg) => Message.show(ShowType.LOADING, msg);
const Toast = (msg) => Message.show(ShowType.TOAST, msg);

export default { success, info, warning, warn, error, loading, Toast };
