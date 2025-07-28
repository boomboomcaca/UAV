import StreamDemo from "../demos/streamDemo.jsx";
import SpectrumDemo2 from "../demos/SpectrumDemo2.jsx";
import MScanDemo from "../demos/mscanDemo.jsx";
import CombineDemo2 from "../demos/combineDemo2.jsx";
import RainChartWithWorker from "../demos/rainChartWithWorker.jsx";
import RainChartWithWorkerGPU from "../demos/rainChartWithWorkerGPU.jsx";
import IQDemo from "../demos/iqDemo.jsx";
import DDCDemo from "../demos/DDCDemo.jsx";
import DPXDemo from "../demos/DPXDemo.jsx";

const routeContig = [
  {
    name: "streamlevel",
    path: "/streamlevel",
    Element: StreamDemo,
  },
  {
    name: "spectrum",
    path: "/spectrum2",
    Element: SpectrumDemo2,
  },
  {
    name: "mscan",
    path: "/mscan",
    Element: MScanDemo,
  },
  {
    name: "CombineScan",
    path: "/scan",
    Element: CombineDemo2,
  },
  {
    name: "Rain-Worker",
    path: "/rainTest",
    Element: RainChartWithWorker,
  },
  {
    name: "Rain-WorkerGPU",
    path: "/rainTest1",
    Element: RainChartWithWorkerGPU,
  },
  {
    name: "IQDemo",
    path: "/iqdemo",
    Element: IQDemo,
  },
  {
    name: "DDCDemo",
    path: "/ddcdemo",
    Element: DDCDemo,
  },
  {
    name: "DPXDemo",
    path: "/dpxdemo",
    Element: DPXDemo,
  },
];

export default routeContig;
