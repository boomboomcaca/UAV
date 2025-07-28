/* eslint-disable */
const config = require('../data/config/config');

exports.corsHandler = async (
  { headers: { origin = '' }, raw: { method } },
  reply
) => {
  reply.header('Access-Control-Allow-Origin', '*');
  reply.header(
    'Access-Control-Allow-Headers',
    'Authorization, Origin, No-Cache, X-Requested-With, If-Modified-Since, Pragma, Last-Modified, Cache-Control, Expires, Content-Type, X-E4M-With'
  );
  reply.header(
    'Access-Control-Allow-Methods',
    'PUT,PATCH,POST,GET,DELETE,OPTIONS'
  );
  // OPTIONS
  if (method === 'OPTIONS') {
    return reply.code(200).send();
  }
};
