const gradientColors = (colors, outCount) => {
  const gap = outCount / (colors.length - 1);
  const outColors = [];
  for (let i = 1; i < colors.length; i += 1) {
    const s = Math.round(gap * (i - 1));
    const e = Math.round(gap * i);
    // window.console.log(String(colors[i - 1]).slice(1, 3), String(colors[i - 1]).slice(3, 5));
    const r1 = parseInt(String(colors[i - 1]).slice(1, 3), 16);
    const g1 = parseInt(String(colors[i - 1]).slice(3, 5), 16);
    const b1 = parseInt(String(colors[i - 1]).slice(5, 7), 16);
    // window.console.log(r1, g1, b1);
    const r2 = parseInt(String(colors[i]).slice(1, 3), 16);
    const g2 = parseInt(String(colors[i]).slice(3, 5), 16);
    const b2 = parseInt(String(colors[i]).slice(5, 7), 16);
    // window.console.log(r2, g2, b2);
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = s; l < e; l += 1) {
      const r = Math.round(r1 + rsetp * (l - s));
      const g = Math.round(g1 + gstep * (l - s));
      const b = Math.round(b1 + bstep * (l - s));
      // window.console.log(r, g, b);
      const rs = r.toString(16).padStart(2, '0');
      const gs = g.toString(16).padStart(2, '0');
      const bs = b.toString(16).padStart(2, '0');
      const hex = `#${rs}${gs}${bs}`;
      outColors[l] = hex;
    }
  }
  return outColors;
};

export default gradientColors;
