export const getColumns = (bo) => {
  const cols = [
    {
      key: 'id',
      name: 'ID',
      sort: false,
      sticky: bo ? 'left' : false,
      style: bo ? { width: '120px' } : null,
    },
    {
      key: 'name',
      name: '名称',
      sort: false,
      sticky: false,
      style: bo ? { width: '500px' } : null,
    },
    {
      key: 'display',
      name: '显示',
      sort: false,
      sticky: false,
      style: bo ? { width: '750px' } : null,
    },
    {
      key: 'display2',
      name: '显示2',
      sort: false,
      sticky: false,
      style: bo ? { width: '750px' } : null,
    },
    {
      key: 'display3',
      name: '显示3',
      sort: false,
      sticky: false,
      style: bo ? { width: '250px' } : null,
    },
    {
      key: 'age',
      name: '年份',
      sort: false,
      sticky: false,
      style: bo ? { width: '500px' } : null,
    },
    {
      key: '??',
      name: '未知',
      sort: false,
      sticky: bo ? 'right' : false,
      style: bo ? { width: '120px' } : null,
    },
  ];
  return cols;
};

export const getData = () => {
  const data = [];
  for (let i = 0; i < 10; i += 1) {
    const item = {
      id: i,
      name: '????????',
      display: '！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！',
      display2: '？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？',
      display3: '——————————————————————————————————————————————————————————————————————',
      age: 100 + i,
      '??': '不详',
    };

    data.push(item);
  }
  return data;
};
