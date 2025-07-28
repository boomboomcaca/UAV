const { Worker } = require("worker_threads");

const serverWorker = new Worker("./startServer.js");

