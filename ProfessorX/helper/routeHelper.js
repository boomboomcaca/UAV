exports.regGlobalHandler = (request, reply) => {
  // OPTIONS
  if (request.method.toLowerCase() === 'options') {
    reply.header('Access-Control-Allow-Origin', '*');
    reply.header(
      'Access-Control-Allow-Headers',
      'Authorization, Origin, No-Cache, X-Requested-With, If-Modified-Since, Pragma, Last-Modified, Cache-Control, Expires, Content-Type, X-E4M-With'
    );
    reply.header(
      'Access-Control-Allow-Methods',
      'PUT,PATCH,POST,GET,DELETE,OPTIONS'
    );
    return reply.code(200).send();
  }
  return reply.code(404).send({
    message: '无效的 api 请求。',
  });
};
