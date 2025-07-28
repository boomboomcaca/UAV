import AsyncLoadable from "../utils/AsyncLoadable.jsx";

// 首页
const Index = AsyncLoadable(() => import("../views/Home/Index1.jsx"));

const MorscrPage = AsyncLoadable(() => import("../views/Morscr/Index.jsx"));

const Histotry = AsyncLoadable(() => import("../views/History/History.jsx"));

const WhiteBlackList = AsyncLoadable(() =>
  import("../views/WhiteBlackList/WhiteBlackList.jsx")
);

const SystemConfig = AsyncLoadable(() =>
  import("../views/SystemConfig/SystemConfig.jsx")
);

const DefenseStrategy = AsyncLoadable(() =>
  import("../views/DefenseStrategy/DefenseStrategy.jsx")
);

const ThreatLevel = AsyncLoadable(() =>
  import("../views/ThreatLevel/ThreatLevel.jsx")
);

const defaultRoutes = [
  { path: "/index", exact: true, name: "Index", component: Index, auth: [1] },
  // { path: "/index?fit", exact: false, name: "IndexFit", component: Index, auth: [10] },
  // {
  //   path: "/protectedregion",
  //   exact: true,
  //   name: "protectedregion",
  //   component: ProtectedRegion,
  //   auth: [2],
  // },
  {
    path: "/morscr1",
    exact: true,
    name: "morscr1",
    component: MorscrPage,
    auth: [3],
  },
  {
    path: "/history",
    exact: true,
    name: "history",
    component: Histotry,
    auth: [4],
  },
  // {
  //   path: "/whiteregions",
  //   exact: true,
  //   name: "whiteregions",
  //   component: WhiteRegions,
  //   auth: [5],
  // },
  {
    path: "/whiteblacklist",
    exact: true,
    name: "whiteblacklist",
    component: WhiteBlackList,
    auth: [6],
  },
  {
    path: "/systemconfig",
    exact: true,
    name: "systemconfig",
    component: SystemConfig,
    auth: [7],
  },
  {
    path: "/threatlevel",
    exact: true,
    name: "threatlevel",
    component: ThreatLevel,
    auth: [8],
  },

  {
    path: "/defensestrategy",
    exact: true,
    name: "defensestrategy",
    component: DefenseStrategy,
    auth: [9],
  },
];

export default defaultRoutes;
