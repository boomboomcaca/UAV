import { useEffect, useContext } from 'react';
import AppContext from '@/store/context';
import { LOCATION, STEPABLE } from '@/store/reducer';

function useLocation(lock, action) {
  const {
    state: { headereturn, location, stepable },
    dispatch,
  } = useContext(AppContext);

  const updateLocation = (lo) => {
    dispatch({ type: LOCATION, payload: lo });
  };

  const updateStepable = (bo) => {
    dispatch({ type: STEPABLE, payload: bo });
  };

  useEffect(() => {
    if (lock === location) {
      action?.();
    }
  }, [headereturn]);

  return { updateLocation, updateStepable, stepable };
}

export default useLocation;
