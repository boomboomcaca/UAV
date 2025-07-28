export const getTimeStr = (time) => {
  const year = time.getFullYear();
  let month = time.getMonth() + 1;
  month = month < 10 ? `0${month}` : month;
  const day = time.getDate() < 10 ? `0${time.getDate()}` : time.getDate();
  const hours = time.getHours() < 10 ? `0${time.getHours()}` : time.getHours();
  const mins = time.getMinutes() < 10 ? `0${time.getMinutes()}` : time.getMinutes();
  const secs = time.getSeconds() < 10 ? `0${time.getSeconds()}` : time.getSeconds();
  return `${year}-${month}-${day} ${hours}:${mins}:${secs}`;
};

export const tes = {};
