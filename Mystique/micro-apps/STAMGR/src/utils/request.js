import axios from '@dc/axios';
import getConfig from '@/config';

export default axios({
  getBaseURL: () => {
    const { apiBaseUrl } = getConfig();
    return apiBaseUrl;
  },
});
