// 互调公式
export const Utility = {
  Cal2Freq2: (f1, f2) => [Math.round(Math.abs(f1 - f2), 6), Math.round(Math.abs(f1 + f2), 6)],
  Cal2Freq3: (f1, f2) => Math.round(Math.abs(2 * f1 - f2), 6),
  Cal2Freq5: (f1, f2) => Math.round(Math.abs(3 * f1 - 2 * f2), 6),
  Cal2Freq7: (f1, f2) => Math.round(Math.abs(4 * f1 - 3 * f2), 6),
  Cal3Freq3: (f1, f2, f3) => [Math.round(Math.abs(f1 - f2 + f3), 6)],
  Cal3Freq5: (f1, f2, f3) => [Math.round(Math.abs(2 * f1 - 2 * f2 + f3), 6), Math.round(Math.abs(3 * f1 - f2 - f3), 6)],
  Cal3Freq7: (f1, f2, f3) => [
    Math.round(Math.abs(2 * f1 - 3 * f2 + 2 * f3), 6),
    Math.round(Math.abs(3 * f1 - 3 * f2 + f3), 6),
    Math.round(Math.abs(4 * f1 - 2 * f2 + f3), 6),
  ],
};
export default { Utility };
