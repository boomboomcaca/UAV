import React from 'react';
import PropTypes from 'prop-types';
import { Button } from 'dui';
import { setLocale } from 'dc-intl';
import ImAnalysis from '@/components/ImAnalysis';
import styles from './ImAnalysisDemo.module.less';

const ImAnalysisDemo = () => {
  return (
    <div>
      <div>
        <Button onClick={() => setLocale('zh')}>中文</Button>
        <Button onClick={() => setLocale('en')}>英文</Button>
      </div>
      <ImAnalysis />
    </div>
  );
};

ImAnalysisDemo.defaultProps = {};

ImAnalysisDemo.propTypes = {};

export default ImAnalysisDemo;
