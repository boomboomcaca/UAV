import { useState, useEffect } from 'react';

function useServer() {
  const [processes, setProcesses] = useState([]);

  useEffect(() => {}, []);

  return { processes };
}

export default useServer;
