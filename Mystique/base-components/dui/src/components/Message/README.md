# Message

## 何时使用

- 消息组件
- 例子详见 Demo

## API

### Message props

| 参数     | 说明     | 类型            | 默认值                              |
| -------- | -------- | --------------- | ----------------------------------- |
| kid      | 唯一标识 | any             | null                                |
| type     | 类型     | ShowType:string | ShowType.INFO [ShowType](#ShowType) |
| msg      | 提示信息 | string          | '暂无消息提示。'                    |
| icon      | 自定义icon | children     | 在使用Toast时生效                  |
| duration | TTL      | number          | 2                                   |

- <a id="ShowType" name="ShowType">ShowType</a>

```js
const ShowType = {
  SUCCESS: 'success',
  INFO: 'info',
  WARNING: 'warning',
  ERROR: 'error',
  LOADING: 'loading',
  TOAST: 'Toast',
};
```

### 实际使用

```js
import message from 'dui';
message.success();
message.info();
message.warning();
message.warn();
message.error();
message.loading();
message.Toast();
```
