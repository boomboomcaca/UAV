Require all dependencies you need in the main.js file that is run by electron. (this seemed to be the first important part for me)
Run npm i -D electron-rebuild to add the electron-rebuild package
Remove the node-modules folder, as well as the packages-lock.json file.
Run npm i to install all modules.

- Run ./node_modules/.bin/electron-rebuild (.\node_modules\.bin\electron-rebuild.cmd for Windows) to rebuild everything

npm rebuild better-sqlite3  ::: If you're using Electron, try running electron-rebuild
