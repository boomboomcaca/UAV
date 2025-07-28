import React, { useState } from 'react';
import ButtonRadio from '../index';

export default function Demo() {
  const [status, setStatus] = useState(false);

  return (
    <>
      <ButtonRadio size="small" status={status} onClick={(e) => setStatus(e)}>
        Button
      </ButtonRadio>
      <ButtonRadio status={status} onClick={(e) => setStatus(e)}>
        Button
      </ButtonRadio>
      <ButtonRadio size="large" status={status} onClick={(e) => setStatus(e)}>
        Button
      </ButtonRadio>
    </>
  );
}
