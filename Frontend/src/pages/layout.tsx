import { Outlet } from "react-router-dom";
import "../index.scss";
import Sidebar from "../components/common/sidebar";
import Navbar from "../components/common/navbar";

export default function Layout() {
  return (
    <div className="layout">
      <Sidebar />
      <main id="page">
        <Navbar/>
        <Outlet />
      </main>
    </div>
  );
}
