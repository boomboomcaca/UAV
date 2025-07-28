import { useState, useRef, useEffect } from 'react';
import { useThrottleFn } from 'ahooks';

function useScrollMore(scrollRef, length, onLoadMore) {
  const prevScrollTopRef = useRef(0);

  const triggerRef = useRef(null);
  const [loading, setLoading] = useState(false);

  const onScrollListener = () => {
    if (triggerRef.current || !scrollRef.current) {
      return;
    }

    const scrollUp = scrollRef.current.scrollTop < prevScrollTopRef.current;
    prevScrollTopRef.current = scrollRef.current.scrollTop;
    if (scrollUp) {
      return;
    }

    const { current } = scrollRef;
    const { clientHeight, scrollTop, scrollHeight } = current;
    const atBottom = scrollTop + clientHeight >= scrollHeight - 20;
    if (atBottom) {
      triggerRef.current = true;
      if (atBottom) {
        onLoadMore((canload) => {
          if (canload) {
            setLoading(true);
          }
        });
      }
      setTimeout(() => {
        triggerRef.current = false;
        setLoading(false);
      }, 5000);
    }
  };

  useEffect(() => {
    triggerRef.current = false;
    setLoading(false);
  }, [length]);

  const { run: runScroll } = useThrottleFn(onScrollListener, { wait: 250 });

  useEffect(() => {
    if (scrollRef.current) {
      // eslint-disable-next-line no-param-reassign
      scrollRef.current.onscroll = runScroll;
    }
  }, [scrollRef]);

  return [loading];
}

export default useScrollMore;
