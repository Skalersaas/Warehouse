import { createBrowserRouter } from "react-router-dom";
import Dashboard from "../pages/dashboard";
import Layout from "../pages/layout";
import BalancePage from "../pages/balances";
import ClientPage from "../pages/clients";
import UnitPage from "../pages/units";
import ResourcePage from "../pages/resources";
import ReceiptPage from "../pages/receipts";
import ShipmentPage from "../pages/shipment";
import ReceiptDetail from "../pages/receipts/detail";
import CreateReceipt from "../pages/receipts/create";
import ShipmentDetail from "../pages/shipment/detail";
import CreateShipment from "../pages/shipment/create";

const router = createBrowserRouter([
  {
    path: "/",
    Component: Layout,
    children: [
      { index: true, Component: Dashboard },
      { path: "/balances", Component: BalancePage },
      {
        path: "/receipts",
        children: [
          { index: true, Component: ReceiptPage },
          { path: ":id", Component: ReceiptDetail },
          { path: "create", Component: CreateReceipt },
        ],
      },
      {
        path: "/shipments",
        children: [
          { index: true, Component: ShipmentPage },
          { path: ":id", Component: ShipmentDetail },
          { path: "create", Component: CreateShipment },
        ],
      },
      {
        path: "/resources",
        children: [
          { index: true, Component: ResourcePage },
          { path: ":id", Component: ResourcePage },
          { path: "create", Component: ResourcePage },
        ],
      },
      {
        path: "/units",
        children: [
          { index: true, Component: UnitPage },
          { path: ":id", Component: UnitPage },
          { path: "create", Component: UnitPage },
        ],
      },
      {
        path: "/clients",
        children: [
          { index: true, Component: ClientPage },
          { path: ":id", Component: ClientPage },
          { path: "create", Component: ClientPage },
        ],
      },
    ],
  },
]);

export default router;
