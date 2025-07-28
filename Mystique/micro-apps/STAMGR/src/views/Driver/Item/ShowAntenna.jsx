import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { Checkbox, Radio } from 'dui';
import classnames from 'classnames';
import { getDeviceParam } from '@/api/template';
import styles from './showAntenna.module.less';

const defaultID = '00000000-0000-0000-0000-000000000000';

const ShowAntenna = (props) => {
  const { className, antennaID, driverParams, onParamChanged } = props;

  const antennaSetRef = useRef(null);
  const antennasRef = useRef(null);
  const asModeRef = useRef(null);
  const antIDRef = useRef(null);
  const polarRef = useRef(null);

  const [options1, setOptions1] = useState([]);
  const [value1, setValue1] = useState([]);

  const [options2, setOptions2] = useState([]);
  const [value2, setValue2] = useState(null);

  useEffect(() => {
    if (antennaID) {
      getDeviceParam(antennaID).then((res) => {
        window.console.log(res);
        if (res.result) {
          const antennaSet = res.result.parameters.find((p) => {
            return p.name === 'antennaSet';
          });
          const antennas = driverParams.find((p) => {
            return p.name === 'antennas';
          });
          const asMode = driverParams.find((p) => {
            return p.name === 'antennaSelectionMode';
          });
          // TODO warning 固定为manual
          asMode.value = 'manual';
          onParamChanged({ name: 'antennaSelectionMode', value: 'manual' });
          let antID = driverParams.find((p) => {
            return p.name === 'antennaID';
          });
          if (!antID) {
            antID = res.result.parameters.find((p) => {
              return p.name === 'antennaID';
            });
          }
          let polar = driverParams.find((p) => {
            return p.name === 'polarization';
          });
          if (!polar) {
            polar = res.result.parameters.find((p) => {
              return p.name === 'polarization';
            });
          }

          asModeRef.current = asMode;
          antennaSetRef.current = antennaSet;
          antennasRef.current = antennas;
          antIDRef.current = antID;
          polarRef.current = polar;

          if (antennaSet && antennas) {
            const opts = antennaSet.parameters.map((ap) => {
              return {
                label: ap.displayName || ap.name,
                value: ap.id,
              };
            });
            const vals = antennas.parameters.map((ap) => {
              return ap.id;
            });
            // if (vals.length === 0) {
            //   vals.push(antennaSet.parameters[0].id);
            // }
            const set = antennas.parameters.filter((p) => {
              return opts.find((opt) => {
                return opt.value === p.id;
              });
            });
            const values = [];
            const displayValues = [];
            set.forEach((s) => {
              values.push(s.id);
              displayValues.push(s.name);
            });
            antIDRef.current = { ...antIDRef.current, name: 'antennaID', values, displayValues };
            onParamChanged(antIDRef.current);

            setOptions1(opts);
            setValue1(vals);
          }
          if (asMode) {
            const opts = asMode.values.map((av, i) => {
              return {
                label: asMode.displayValues[i],
                value: av,
              };
            });
            setOptions2(opts);
            setValue2(asMode.value);
          }
        }
      });
    }
  }, [antennaID]);

  const onValue1Change = (vals) => {
    setValue1(vals);
    if (antennaSetRef.current) {
      const set = antennaSetRef.current.parameters.filter((p) => {
        return vals.includes(p.id);
      });
      onParamChanged({ name: 'antennas', parameters: set });
      const values = [];
      const displayValues = [];
      set.forEach((s) => {
        if (s.id !== defaultID) {
          values.push(s.id);
          displayValues.push(s.displayName || s.name);
        }
      });
      antIDRef.current = { ...antIDRef.current, name: 'antennaID', values, displayValues };
      onParamChanged(antIDRef.current);
    }
  };

  const onValue2Change = (val) => {
    setValue2(val);
    onParamChanged({ name: 'antennaSelectionMode', value: val });
    if (val === 'polarization') {
      onParamChanged(polarRef.current);
    }
    if (val === 'manual') {
      onParamChanged(antIDRef.current);
    }
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.item} style={{ display: 'none' }}>
        <div className={styles.title}>天线模式：</div>
        <div className={styles.selects}>
          <Radio options={options2} value={value2} onChange={onValue2Change} />
        </div>
      </div>
      {/* {value2 === 'manual' ? ( */}
      <div className={styles.item}>
        <div className={styles.title}>选择的天线集合：</div>
        <div className={styles.selects}>
          <Checkbox.Group options={options1} value={value1} onChange={onValue1Change} />
        </div>
      </div>
      {/* ) : null} */}
    </div>
  );
};

ShowAntenna.defaultProps = {
  className: null,
  antennaID: null,
  driverParams: null,
  onParamChanged: () => {},
};

ShowAntenna.propTypes = {
  className: PropTypes.any,
  antennaID: PropTypes.any,
  driverParams: PropTypes.any,
  onParamChanged: PropTypes.func,
};

export default ShowAntenna;
