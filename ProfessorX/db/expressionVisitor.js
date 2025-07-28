/* eslint-disable */
const { knexInstance } = require('./dbHelper');

// todo:后续根据需求添加 add update delete
/**
 * Lamda 表达式解析
 *
 * @param {*} tableName
 */
function ExpressionVisitor(tableName) {
  this.sql = '';
  this.tableName = tableName;
  // limit 取多少条 take
  this.limit = 0;
  // offest 偏移多少条 skip
  this.offset = 0;

  this.isFirst = false;

  this.queryColumn = [];

  this.condition = [
    { key: '&&', value: 'and' },
    { key: '||', value: 'or' },
  ];
  this.method = ['startsWith', 'endsWith', 'contains'];
  // 操作符 '==='直接映射成一个=
  this.operator = ['===', '>', '>=', '<', '<='];
  // 存放数据 key,value key 字段 value 值
  this.dataVisitor = [];
  // 存放操作符

  // 更新
  this.update = function (data, expression) {};
  // 删除
  this.delete = function (expression) {};
  // 添加
  this.add = function (data) {};
  /**
   * where 条件查询 Lamda 表达式 箭头函数
   *
   * @param {*} expression
   */
  this.where = function (expression) {
    //  移除字符串中的空格
    //  todo:统计代码覆盖率 在函数表达式里面侵入了 一些代码 导致故障
    //  s=>{/*istanbulignorenext*/cov_bggyv8n54().f[1]++;cov_bggyv8n54().s[19]++;returns.account===data.account;}
    //  s=>s.aouunt===data.account
    //  这个不是万能的
    //  s=>{/*istanbulignorenext*/cov_1ekq10aizf().f[1]++;cov_1ekq10aizf().s[19]++;returns.account===data.account;}
    expression = expression.toString().replace(/\s*/g, '');
    expression = expression.replace(/\r\n/g, '');
    console.log(expression);
    // 如果检测到有代码覆盖率 侵入
    if (expression.includes('/*istanbulignorenext*/')) {
      // todo:如果有这一样注释则 会忽略代码覆盖率检测 也可以在使用的地方加上注释
      /* istanbul ignore next */
      const endIdx = expression.indexOf('returns.');
      expression =
        expression.substring(0, 3) +
        expression.substring(endIdx + 8, expression.length - 2);
    }
    // expression=expression.replace(/'/g,'');
    // 去掉首位空格
    const expr = expression.match(/^[(\s]*([^()]*?)[)\s]*=>(.*)/);
    if (expr.length < 2) {
      throw '表达式无效';
    }
    const param = expr[1];
    const exprStr = expr[2];
    const exprData = [];
    let startIndex = 0;
    let endIndex = 0;
    // 条件拆分
    //.replace(/s*\|s*/g, '|'); 正则拆分
    for (let i = 0; i < exprStr.length - 1; i++) {
      const item = exprStr[i] + exprStr[i + 1];
      const result = this.condition.find((s) => s.key == item);
      // 如果找到了 还应该把条件操作符存到这个里面
      if (result) {
        endIndex = i;
        exprData.push({
          key: exprStr.substring(startIndex, endIndex),
          value: result.value,
        });
        startIndex = i + 2;
      }
    }
    exprData.push({
      key: exprStr.substring(startIndex, exprStr.length),
      value: '',
    });

    // userContext.where((p) => p.userName > 33 && p.account == 44);
    for (let i = 0; i < exprData.length; i++) {
      const item = exprData[i].key;
      const condition = exprData[i].value;
      // 单独处理 in 查询
      let isInMethod = false;
      const methodItem = this.method.filter((p) => item.includes(p));
      // 如果使用Contains 关键字处理比较困难 逻辑混乱
      if (item.includes('.in')) {
        // 外部变量 //外部变量有可能是A.B.C
        const externalVar = item.split('.in')[0].trim().split('.').last();
        //  externalVar=externalVar.split('.').last();
        // 处理 in 查询
        if (
          this[externalVar] ||
          (externalVar.indexOf(']') >= 0 && externalVar.indexOf('[') >= 0)
        ) {
          // 表示是in查询
          isInMethod = true;
          const field = item
            .match(/\((.+?)\)/g)[0]
            .replace(')', '')
            .replace('(', '')
            .replace(/'/g, '')
            .replace(`${param}.`, '');
          const method = 'in';
          // 去掉第一位和最后一位[]
          const value =
            this[externalVar] ||
            externalVar
              .substring(1, externalVar.length - 1)
              .replace(/'/g, '')
              .split(',');
          // 记录外部变量完成之后需要进行变量清理
          this.dataVisitor.push({
            field,
            value,
            operator: method,
            condition,
            externalVar,
          });
        }
      }
      // 处理其他非 in 方法 contains startWith endWith
      if (methodItem.length > 0 && !isInMethod) {
        const field = item.split('.')[1];
        const method = methodItem[0];
        const value = item
          .match(/\((.+?)\)/g)[0]
          .replace(')', '')
          .replace('(', '')
          .replace(/'/g, '');
        this.dataVisitor.push({
          field,
          value,
          operator: method,
          condition,
        });
      }
      // 解析表达式中的字段和值以及字段直接的条件
      this.operator.forEach((s) => {
        if (item.includes(s)) {
          const field = item.split(s)[0].replace(`${param}.`, '').trim();
          const value = item.split(s)[1].trim();
          // 直接替换三个=== 操作符
          s = s == '===' ? '=' : s;
          this.dataVisitor.push({
            field,
            value,
            operator: s,
            condition,
          });
        }
      });
    }
    // console.log(this.dataVisitor);
    return this;
  };
  // 数据清理
  this.clear = function () {
    this.sql = '';
    this.limit = 0;
    this.offset = 0;
    this.isFirst = false;
    this.queryColumn = [];
    this.dataVisitor = [];
  };
  /**
   * 查询指定条数据
   *
   * @param {*} num
   */
  this.take = function (num) {
    this.limit = num;
    return this;
  };
  /**
   * 跳过指定条数据
   *
   * @param {*} num
   */
  this.skip = function (num) {
    this.offset = num;
    return this;
  };
  /**
   * 取第一条数据
   */
  this.first = function () {
    // 如果是直接first
    this.offset = 0;
    this.limit = 1;
    this.isFirst = true;
    return this;
  };
  this.select = function (queryColumn) {
    this.queryColumn = queryColumn;
    return this;
  };
  /**
   * 解析Sql进行数据库查询自己拼接Sql--测试通过
   */
  this.toList1 = async function () {
    // 如果有错误直接上层捕获
    // 直接访问dataVisitor;
    // 后期可以考虑加上缓存！如果两个查询条件一样则缓存
    // todo:方法拆分 可以提供delete update 使用
    // todo:底层也可以考虑knex 拼接查询条件 这样可以避免数据库问题
    let sql = `select * from ${this.tableName} `;
    // 处理字段映射，字段取别名
    if (this.queryColumn.length > 0) {
      sql = 'select ';
      Object.entries(this.queryColumn).forEach((item) => {
        const field = item[1];
        if (typeof field === 'object') {
          // `id` as `edgeID`
          const [key, value] = Object.entries(field)[0];

          sql += ` \`${value}\` as \`${key} \`,`;
        } else {
          sql += ` \`${field}\`, `;
        }
      });
      // 移除最后一个,
      sql = sql.trim();
      sql = sql.substring(0, sql.length - 1);
      sql += ` from  ${this.tableName} `;
    }
    if (this.dataVisitor.length > 0) {
      sql += ' where ';
    }
    this.dataVisitor.forEach((item) => {
      // 处理特殊操作符后面代码优化可以拆分成为方法
      // 需要对item.value 进行 值得映射
      // todo:测试 = 单引号效果
      // 外部变量进行值得替换
      if (typeof item.value === 'string') {
        item.value = item.value.replace(/'/g, '');
        const externalVar = item.value.split('.').last();
        item.value = this[externalVar] || item.value;
        // delete this[item.externalVar];
      }

      // 处理in查询
      if (item.operator === 'in') {
        // account in ('admin','testAdmin')
        // 测试证明数字也都加上单引号效果比较好，会走索引
        console.log(item.value);
        sql += `  \`${item.field}\` in (${item.value
          .map((s) => `'${s}'`)
          .toString()})  ${item.condition} `;
        // 删除外部变量 很多地方都在使用UserContext 不清理数据量会越来越大？
        // 换个角度来说 也不会很大！比较你的属性也就这么多！
        // 暂时不用清理 如果需要清理可以进行手动清理
        // delete this[item.externalVar];
      }
      // 处理方法
      else if (item.operator === 'contains') {
        sql += `  \`${item.field}\` like '%${item.value}%'  ${item.condition} `;
      } else if (item.operator === 'startsWith') {
        sql += `  \`${item.field}\` like '${item.value}%'  ${item.condition} `;
      } else if (item.operator === 'endsWith') {
        sql += `  \`${item.field}\` like '%${item.value}'  ${item.condition} `;
      }
      // 处理普通数据库查询
      else {
        sql += `  \`${item.field}\` ${item.operator} '${item.value}' ${item.condition} `;
      }
    });
    // 处理分页
    if (this.offset > 0) {
      sql += ` limit ${this.offset},${this.limit} `;
    } else if (this.limit > 0) {
      sql += ` limit ${this.limit} `;
    }
    // 如果sql一样则从缓存中取!
    // 如果操作了添加、删除、修改 则删除缓存
    // 缓存后面再说 慎用 默认关闭开启后则进行缓存
    console.log(sql);
    const res = await knexInstance.raw(sql);
    const [data] = res;

    // 如果查询出来的结果有数据
    if (data && this.isFirst) {
      this.clear();
      return data[0] || null;
    }
    this.clear();
    return data || null;
  };
  /**
   * 直接使用knex 进行条件拼接
   */
  this.toList = async function () {
    // todo:不用考虑sql 优化问题 直接使用knex给你优化的，少些坑
    // 如果有错误直接上层捕获
    // 直接访问dataVisitor;
    // 后期可以考虑加上缓存！如果两个查询条件一样则缓存
    // todo:方法拆分 可以提供delete update 使用
    // todo:底层也可以考虑knex 拼接查询条件 这样可以避免数据库问题
    let query = knexInstance.select().from(tableName);
    if (this.queryColumn.length > 0) {
      query = knexInstance.column(queryColumn).select().from(tableName);
    }
    this.dataVisitor.forEach((item) => {
      if (typeof item.value === 'string') {
        // todo:小数测试
        item.value = item.value.replace(/'/g, '');
        const externalVar = item.value.split('.').last();
        item.value = this[externalVar] || item.value;
        // delete this[item.externalVar];
      }
      if (item.operator === 'in' && item.operator != 'or') {
        query = query.whereIn(item.field, item.value);
      } else if (item.operator === 'in') {
        query = query.orWhereIn(item.field, item.value);
      } else if (item.operator === 'contains') {
        item.value = `%${item.value}%`;
        item.operator = 'like';
      } else if (item.operator === 'startsWith') {
        item.value = `%${item.value}`;
        item.operator = 'like';
      } else if (item.operator === 'endsWith') {
        item.value = `${item.value}%`;
        item.operator = 'like';
      }

      if (item.operator !== 'in' && item.condition !== 'or') {
        query = query.andWhere(item.field, item.operator, item.value);
      } else if (item.operator !== 'in') {
        query = query.orWhere(item.field, item.operator, item.value);
      }
    });

    if (this.limit > 0) {
      query = query.limit(this.limit).offset(this.offset);
    } else if (this.offset > 0) {
      query = query.offset(this.offset);
    }
    // 如果sql一样则从缓存中取!
    // 如果操作了添加、删除、修改 则删除缓存
    // 缓存后面再说 慎用 默认关闭开启后则进行缓存
    console.log(query.toString());
    let data = await query;
    // todo:待测试如果没有的情况
    data = this.isFirst && data ? data[0] : data;
    this.clear();
    return data || null;
  };
}
module.exports = ExpressionVisitor;
