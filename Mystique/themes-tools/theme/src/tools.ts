const KEY = 'theme';
const MODE_KEY = 'theme-mode';
const LIHGT_KEY = 'light';
const DARK_KEY = 'dark';
const AUTO_KEY = 'auto';

export const Mode = {
  light: LIHGT_KEY,
  dark: DARK_KEY,
  auto: AUTO_KEY,
};

class Theme {
  constructor(mode: string | undefined) {
    const m = <string>(mode || this.getMode());
    // 设置默认模式
    this.setMode(m);
    // 设置默认主题
    const t = m === Mode.auto ? this.getMedia() : m;
    this.setTheme(t);
  }

  /**
   * 获取主题
   *
   * @returns
   * @memberof Theme
   */
  public getTheme(): string | null {
    return document.documentElement.getAttribute(KEY);
  }

  /**
   * 设置主题
   *
   * @param {*} [theme=string]
   * @memberof Theme
   */
  public setTheme(theme: string): void {
    document.documentElement.setAttribute(KEY, theme);
    localStorage.setItem(KEY, theme);
  }

  /**
   * 切换主题
   *
   * @memberof Theme
   */
  public switchTheme(): void {
    const m = this.getTheme() === LIHGT_KEY ? DARK_KEY : LIHGT_KEY;

    this.setTheme(m);
    this.setMode(m);
  }

  /**
   * 设置模式
   *
   * @param {*} mode
   * @memberof Theme
   */
  public setMode(mode: string): void {
    document.documentElement.setAttribute(MODE_KEY, mode);
    localStorage.setItem(MODE_KEY, mode);
  }

  /**
   * 获取模式
   *
   * @returns
   * @memberof Theme
   */
  public getMode(): string | null {
    return localStorage.getItem(MODE_KEY) || Mode.auto;
  }

  /**
   * 获取系统模式
   *
   * @returns
   * @memberof Theme
   */
  public getMedia(): string {
    const isDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches;
    return isDark ? DARK_KEY : LIHGT_KEY;
  }
}

export default Theme;
