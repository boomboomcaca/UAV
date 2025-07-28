import React, { useEffect } from 'react';
// eslint-disable-next-line import/no-extraneous-dependencies
import loadable from '@loadable/component';
import NProgress from 'nprogress';

const useLoadingComponent = () => {
  useEffect(() => {
    NProgress.start();
    return () => {
      NProgress.done();
    };
  }, []);
  return <div />;
};

export default (Loader, Loading = useLoadingComponent) => {
  return loadable(Loader, {
    fallback: <Loading />,
  });
};
