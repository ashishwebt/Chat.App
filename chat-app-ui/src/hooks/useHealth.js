import { useEffect, useState } from 'react';
import { api } from '../api/client';

export function useHealth(pollMs = 30000) {
  const [status, setStatus] = useState('checking'); // 'checking' | 'ok' | 'down'

  useEffect(() => {
    let cancelled = false;

    async function check() {
      try {
        await api.health();
        if (!cancelled) setStatus('ok');
      } catch {
        if (!cancelled) setStatus('down');
      }
    }

    check();
    const id = setInterval(check, pollMs);
    return () => {
      cancelled = true;
      clearInterval(id);
    };
  }, [pollMs]);

  return status;
}
