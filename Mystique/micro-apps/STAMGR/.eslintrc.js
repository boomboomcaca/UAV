module.exports = {
  env: {
    browser: true,
    es6: true,
  },
  extends: ['airbnb', 'plugin:prettier/recommended'],
  parser: 'babel-eslint',
  parserOptions: {
    ecmaFeatures: {
      jsx: true,
    },
    ecmaVersion: 2021,
    sourceType: 'module',
  },
  plugins: ['react'],
  rules: {
    'max-len': [1, { code: 120 }],
    'react/jsx-filename-extension': [2, { extensions: ['.js', '.jsx'] }],
    'react/state-in-constructor': 0,
    'import/extensions': [0, 'ignorePackages', { ts: 'never', tsx: 'never', json: 'never', js: 'never' }], // 与alias冲突
    'import/no-extraneous-dependencies': [0, { devDependencies: false }], // cdn 导入会冲突警告
    'jsx-a11y/click-events-have-key-events': 0,
    'jsx-a11y/no-static-element-interactions': 0,
    'react/jsx-props-no-spreading': 0,
    'no-unused-expressions': 0,
    'jsx-a11y/anchor-is-valid': 0,
    'no-nested-ternary': 0,
    'react/static-property-placement': 0,
    'object-curly-newline': 0,
    'no-use-before-define': 0, // 因为是js项目，暂时关闭警告
    'no-param-reassign': 1,
    'no-unused-vars': 1,
    'react/forbid-prop-types': 0,
    'import/no-unresolved': 0,
  },
  settings: {
    'import/resolver': {
      node: {
        extensions: ['.js', '.jsx', '.ts', '.tsx'],
      },
    },
  },
};
