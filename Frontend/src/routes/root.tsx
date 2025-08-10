import { createBrowserRouter } from "react-router-dom";
import Dashboard from "../pages/dashboard";
import Layout from "../pages/layout";
import BalancePage from "../pages/balances";
import ClientPage from "../pages/clients";
import UnitPage from "../pages/units";
import ResourcePage from "../pages/resources";

const router = createBrowserRouter([
  {
    path: "/",
    Component: Layout,
    children: [
      { index: true, Component: Dashboard },
      { path: "/balances", Component: BalancePage },

      // {
      //   path: "auth",
      //   Component: AuthLayout,
      //   children: [
      //     { path: "login", Component: Login },
      //     { path: "register", Component: Register },
      //   ],
      // },
      {
        path: "/resources",
        children: [
          { index: true, Component: ResourcePage },
          // { path: ":id", Component: ResourceDetailPage },
          // { path: "create", Component: ResourceCreatePage },
        ],
      },
      {
        path: "/units",
        children: [
          { index: true, Component: UnitPage },
          // { path: ":id", Component: UnitDetailPage },
          // { path: "create", Component: UnitCreatePage },
        ],
      },
      {
        path: "/clients",
        children: [
          { index: true, Component: ClientPage },
          // { path: ":id", Component: ClientDetailPage },
          // { path: "create", Component: ClientCreatePage },
        ],
      },
    ],
  },
]);

export default router;
