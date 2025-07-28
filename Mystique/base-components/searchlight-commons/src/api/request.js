import axios from '@dc/axios';

export default axios({
  getBaseURL: () => {
    return 'http://192.168.102.99:12001';
  },
});
