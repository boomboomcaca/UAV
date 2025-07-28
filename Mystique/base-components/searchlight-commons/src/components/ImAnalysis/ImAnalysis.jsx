import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Button, Select, InputNumber, Table, Radio, Empty, message } from 'dui';
import langT from 'dc-intl';
import { Utility } from './utils';
import styles from './ImAnalysis.module.less';

const { Option } = Select;
const btnStyle = {
  fontSize: '14px',
  margin: '0 10px 0 0',
};
const ImAnalysis = (props) => {
  const { text, className } = props;
  const formulaOptions = [
    {
      label: langT('commons', 'ImAnalysisSelect1') /* '二信号二阶' */,
      value: 22,
      children: 'f1±f2=Rf±BW',
    },
    {
      label: langT('commons', 'ImAnalysisSelect2') /* '二信号三阶' */,
      value: 23,
      children: '2xf1-f2=Rf±BW',
    },
    {
      label: langT('commons', 'ImAnalysisSelect3') /* '二信号五阶' */,
      value: 25,
      children: '3xf1-2xf2=Rf±W',
    },
    {
      label: langT('commons', 'ImAnalysisSelect4') /* '二信号七阶' */,
      value: 27,
      children: '4xf1-3xf2=Rf±BW',
    },
    {
      label: langT('commons', 'ImAnalysisSelect5') /* '三信号三阶' */,
      value: 33,
      children: 'f1-f2+f3=Rf±BW',
    },
    {
      label: langT('commons', 'ImAnalysisSelect6') /* '三信号五阶' */,
      value: 35,
      children: (
        <span>
          <span>2xf1-2xf2+f3=Rf±BW</span>
          <br />
          <span>3xf1-f2-f3=Rf±BW</span>
        </span>
      ),
    },
    {
      label: langT('commons', 'ImAnalysisSelect7') /* '三信号七阶' */,
      value: 37,
      children: (
        <span>
          <span>2xf1-3xf2+2xf3=Rf±BW</span>
          <br />
          <span>3xf1-3xf2+f3=Rf±BW</span>
          <br />
          <span>4xf1-2xf2+f3=Rf±BW</span>
        </span>
      ),
    },
  ];
  const [formula, setFormula] = useState(formulaOptions[0]);
  const [dataSource, setDataSource] = useState([]);
  const dataSourceRef = useRef(dataSource);
  const [selectedInput, setSelectedInput] = useState([]);
  const selectedInputRef = useRef(selectedInput);
  const [resultSource, setResultSource] = useState([]);
  const [count, setCount] = useState(0);
  const [resultColumns, setResultColumns] = useState([]);
  const [calFreq, setCalFreq] = useState(25);
  const [resultFreq, setResultFreq] = useState(100);
  const [resultType, setResultType] = useState(1);
  useEffect(() => {
    return () => {
      // 重置数据
      setFormula(formulaOptions[0]);
      setDataSource([]);
      setResultSource([]);
      setSelectedInput([]);
      setCount(0);
      setCalFreq(25);
      setResultFreq(100);
      setResultType(1);
      selectedInputRef.current = [];
    };
  }, []);
  const frequencyCellChange = (value, index) => {
    // 信号列表 频率改变
    if (value) {
      dataSourceRef.current[index].frequency = value;
    }
  };
  const onSelectionChanged = (items) => {
    // table选择项
    selectedInputRef.current = items;
    setSelectedInput(items);
  };
  const handleAdd = () => {
    // 添加信号
    const newData = {
      kid: count,
      rank: dataSource.length + 1,
      frequency: 100,
    };
    setDataSource([...dataSourceRef.current, newData]);
    dataSourceRef.current = [...dataSourceRef.current, newData];
    setCount(count + 1);
  };
  const handleDelete = () => {
    // 删除信号列表项
    for (let i = 0; i < dataSourceRef.current.length; i += 1) {
      for (let j = 0; j < selectedInputRef.current.length; j += 1) {
        if (dataSourceRef.current[i].kid === selectedInputRef.current[j].kid) {
          dataSourceRef.current.splice(i, 1);
        }
      }
    }
    dataSourceRef.current.map((item, index) => {
      item.rank = index + 1;
      return item;
    });
    setDataSource(dataSourceRef.current);
    setResultSource([]);
  };
  const calFreqChange = (value) => {
    // 带宽频率 change
    setCalFreq(value);
  };
  const formulaChange = (value, option) => {
    // 互调公式选择
    const choosedFormula = formulaOptions.find((item) => item.value === value);
    if (choosedFormula) {
      setFormula(choosedFormula);
    }
  };
  const resultTypeChange = (value) => {
    // 互调结果 RadioChange
    setResultType(value);
  };
  const resultFreqChange = (value) => {
    // 互调结果频率 change
    setResultFreq(value);
  };
  const startCalculate = () => {
    // 计算公式结果并输出 c#---> btnCalc_Click()
    if (dataSource.length === 0) {
      message.warning(langT('commons', 'ImAnalysisTip1') /* '请先添加信号列表' */);
      return;
    }
    const frequencys = [];
    let duplicated = false;
    for (let i = 0; i < dataSource.length; i += 1) {
      if (!frequencys.includes(dataSource[i].frequency)) {
        frequencys.push(dataSource[i].frequency);
      } else if (!duplicated) {
        duplicated = true;
      }
    }
    if (duplicated) {
      message.warning(langT('commons', 'ImAnalysisTip2') /* '频率列表中重复的项将被忽略' */);
    }
    // 是否选择 所有二信号互调结果
    if (resultType === 3) {
      // 是否选择三阶  c#---> SetDualData()
      if (formula.value > 30) {
        message.warning(langT('commons', 'ImAnalysisTip3') /* '三信号互调频点过多，分析无意义' */);
        return;
      }
      const newColumns = [
        {
          name: '频率/MHz',
          key: 'rank',
          style: {
            width: 50,
          },
          render: (record) => <span>{record.rank || ''}</span>,
        },
      ];
      // 临时修改 20220812
      const dataSourceLen = duplicated ? frequencys.length : dataSource.length;
      const newResultSource = [];
      for (let i = 0; i < dataSourceLen; i += 1) {
        const columnItem = {
          name: dataSource[i].frequency,
          key: dataSource[i].frequency,
          style: {
            width: 100,
          },
        };
        if (i === dataSourceLen - 1) {
          columnItem.fixed = 'right';
        }
        newColumns.push(columnItem);
        const row = { rank: dataSource[i].frequency };
        for (let j = 0; j < dataSourceLen; j += 1) {
          if (i !== j) {
            let dd = [];
            let s = '';
            if (formula.value === 22) {
              dd = Utility.Cal2Freq2(dataSource[i].frequency, dataSource[j].frequency);
              s = `${dd[0]} / ${dd[1]}`;
            }
            if (formula.value === 23) {
              s = Utility.Cal2Freq3(dataSource[i].frequency, dataSource[j].frequency);
            }
            if (formula.value === 25) {
              s = Utility.Cal2Freq5(dataSource[i].frequency, dataSource[j].frequency);
            }
            if (formula.value === 27) {
              s = Utility.Cal2Freq7(dataSource[i].frequency, dataSource[j].frequency);
            }
            row[dataSource[j].frequency] = s;
          } else {
            row[dataSource[j].frequency] = '';
          }
        }
        newResultSource.push(row);
      }
      setResultColumns([...newColumns]);
      setResultSource(newResultSource);
    } else {
      // c#---> SetDataByFormula()
      const newColumns = [
        {
          name: '序号',
          key: 'rank',
          style: {
            width: 50,
          },
          render: (record) => <span>{record.rank || ''}</span>,
        },
        {
          name: `${langT('commons', 'ImAnalysisTh3') /* 频率 */}1(MHz)`,
          key: 'freq1',
        },
        {
          name: `${langT('commons', 'ImAnalysisTh3') /* 频率 */}2(MHz)`,
          key: 'freq2',
        },
        {
          name: `${langT('commons', 'ImAnalysisTh4') /* 互调频率 */}(MHz)`,
          key: 'intermodulationFreq',
        },
        {
          name: `${langT('commons', 'ImAnalysisTh5') /* 被干扰频率 */}(MHz)`,
          key: 'interferingFreq',
        },
      ];
      const dataSourceLen = dataSource.length;
      const newResultSource = [];
      if (formula.value > 30) {
        newColumns.splice(3, 0, {
          name: `${langT('commons', 'ImAnalysisTh3') /* 频率 */}3(MHz)`,
          key: 'freq3',
        });
      }
      setResultColumns([...newColumns]);
      let counts = 0;
      for (let i = 0; i < dataSourceLen; i += 1) {
        for (let j = 0; j < dataSourceLen; j += 1) {
          if (formula.value < 30) {
            if (i !== j) {
              const freqs = get2Freqs(dataSource[i].frequency, dataSource[j].frequency);
              for (let m = 0; m < freqs.length; m += 1) {
                const freq = IsInterupted(freqs[m]);
                if (freq !== -1) {
                  newResultSource.push({
                    rank: newResultSource.length + 1,
                    freq1: dataSource[i].frequency,
                    freq2: dataSource[j].frequency,
                    intermodulationFreq: freqs[m],
                    interferingFreq: freq,
                  });
                  counts += 1;
                }
                if (counts > 1000) {
                  message.warning(langT('commons', 'ImAnalysisTip4') /* '分析已超过1000个频点，无意义！' */);
                  return;
                }
              }
            }
          } else {
            for (let k = 0; k < dataSourceLen; k += 1) {
              if (i !== j && i !== k && j !== k) {
                const freqs = get3Freqs(dataSource[i].frequency, dataSource[j].frequency, dataSource[k].frequency);
                for (let m = 0; m < dataSourceLen; m += 1) {
                  const freq = IsInterupted(freqs[m]);
                  if (freq !== -1) {
                    newResultSource.push({
                      rank: newResultSource.length + 1,
                      freq1: dataSource[i].frequency,
                      freq2: dataSource[j].frequency,
                      freq3: dataSource[k].frequency,
                      intermodulationFreq: freqs[m],
                      interferingFreq: freq,
                    });
                    counts += 1;
                  }
                  if (counts > 1000) {
                    message.warning(langT('commons', 'ImAnalysisTip4') /* '分析已超过1000个频点，无意义！' */);
                    return;
                  }
                }
              }
            }
          }
        }
      }
      setResultSource(newResultSource);
    }
  };
  const get2Freqs = (f1, f2) => {
    // c#---> GetFreqs
    let res = [];
    switch (formula.value) {
      case 22:
        // 二信号二阶
        res = Utility.Cal2Freq2(f1, f2);
        break;
      case 23:
        // 二信号三阶
        res[0] = Utility.Cal2Freq3(f1, f2);
        break;
      case 25:
        // 二信号五阶
        res[0] = Utility.Cal2Freq5(f1, f2);
        break;
      case 27:
        // 二信号七阶
        res[0] = Utility.Cal2Freq7(f1, f2);
        break;
      default:
        break;
    }
    return res;
  };
  const get3Freqs = (f1, f2, f3) => {
    // c#---> GetFreqs
    let res = 0;
    switch (formula.value) {
      case 33:
        // 三信号三阶
        res = Utility.Cal3Freq3(f1, f2, f3);
        break;
      case 35:
        // 三信号五阶
        res = Utility.Cal3Freq5(f1, f2, f3);
        break;
      case 37:
        // 三信号七阶
        res = Utility.Cal3Freq7(f1, f2, f3);
        break;
      default:
        break;
    }
    return res;
  };
  const IsInterupted = (freq) => {
    // c#---> IsInterupted
    if (resultType === 2) {
      // 互调结果为频率
      if (resultFreq * 1000 < freq * 1000 + calFreq && resultFreq * 1000 > freq * 1000 - calFreq) {
        return resultFreq;
      }
    } else {
      for (let i = 0; i < dataSource.length; i += 1) {
        if (
          dataSource[i].frequency * 1000 < freq * 1000 + calFreq &&
          dataSource[i].frequency * 1000 > freq * 1000 - calFreq
        ) {
          return dataSource[i].frequency;
        }
      }
    }
    return -1;
  };
  const columns = [
    {
      name: '序号',
      key: 'rank',
      style: {
        width: 50,
      },
      render: (record) => <span>{record.rank || ''}</span>,
    },
    {
      name: `${langT('commons', 'ImAnalysisTh1') /* 信号列表 */}(MHz)`,
      key: 'frequency',
      style: {
        width: 100,
      },
      render: (record, index) => (
        <div
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          <InputNumber
            key={`input-${record.kid}`}
            defaultValue={record.frequency || '100'}
            onChange={(e) => frequencyCellChange(e, index)}
          />
        </div>
      ),
    },
  ];
  const radioOptions = [
    {
      label: langT('commons', 'ImAnalysisRadio1') /* '所有互调结果' */,
      value: 1,
    },
    {
      label: `${langT('commons', 'ImAnalysisLabel2')}(MHz)` /* '互调结果为频率' */,
      value: 2,
    },
    {
      label: langT('commons', 'ImAnalysisRadio2') /* '所有二信号互调结果' */,
      value: 3,
    },
  ];
  return (
    <div className={classnames(styles.container, className)}>
      <div className={styles.modalContainer}>
        <div className={styles.modalContent}>
          <div className={styles.modalContent_flex}>
            <div className={styles.modalContent_left}>
              <div className={styles.modalContent_left_control}>
                <div className={styles.modalContent_left_space}>
                  <Button style={btnStyle} onClick={handleAdd}>
                    {/* 添加 */}
                    {langT('commons', 'ImAnalysisBtn1')}
                  </Button>
                  <Button style={btnStyle} disabled={selectedInput.length === 0} onClick={handleDelete}>
                    {/* 删除 */}
                    {langT('commons', 'ImAnalysisBtn2')}
                  </Button>
                </div>
              </div>
              <Table
                className={styles.antd_table}
                options={{ bordered: { inline: true, outline: true } }}
                columns={columns}
                data={dataSource}
                onSelectionChanged={onSelectionChanged}
              />
            </div>
            <div className={styles.modalContent_right}>
              <div className={styles.modalContent_right_item}>
                <span>
                  {/* 计算带宽 */}
                  {`${langT('commons', 'ImAnalysisLabel1')}(kHz)`}
                </span>
                <InputNumber defaultValue={calFreq} onChange={calFreqChange} />
              </div>
              <div className={styles.modalContent_right_item}>
                <Radio options={radioOptions} value={resultType} onChange={resultTypeChange} />
              </div>
              <div
                className={
                  resultType === 2
                    ? styles.modalContent_right_item
                    : [styles.modalContent_right_item, styles.right_item_disabled].join(' ')
                }
              >
                <span>
                  {/* 互调结果为频率 */}
                  {`${langT('commons', 'ImAnalysisLabel2')}(kHz)`}
                </span>
                <InputNumber disabled={resultType !== 2} defaultValue={resultFreq} onChange={resultFreqChange} />
              </div>
              <div className={styles.modalContent_right_item}>
                <span>
                  {/* 互调公式 */}
                  {langT('commons', 'ImAnalysisLabel3')}
                </span>
                <Select className={styles.right_item_select} value={formula.value} onChange={formulaChange}>
                  {formulaOptions.map((item, index) => (
                    <Option key={`formulaOptions-${index + 1}`} value={item.value}>
                      {item.label}
                    </Option>
                  ))}
                </Select>
              </div>
              <div className={styles.modalContent_right_item}>{formula.children}</div>
            </div>
          </div>
          <div className={styles.modalContent_control}>
            <Button style={btnStyle} onClick={startCalculate}>
              {/* 开始计算 */}
              {langT('commons', 'ImAnalysisBtn3')}
            </Button>
          </div>
        </div>
        <div className={styles.modalFooter}>
          {resultSource.length > 0 ? (
            <Table
              className={styles.antd_table}
              options={{ bordered: { inline: true, outline: true } }}
              columns={resultColumns}
              data={resultSource}
              showSelection={false}
              rowClassName={styles.table_cell}
            />
          ) : (
            <Empty emptype={Empty.Normal} message={langT('commons', 'ImAnalysisTip5')} />
          )}
        </div>
      </div>
    </div>
  );
};

ImAnalysis.defaultProps = {
  text: '干扰互调分析',
  className: null,
};

ImAnalysis.propTypes = {
  text: PropTypes.string,
  className: PropTypes.any,
};

export default ImAnalysis;
