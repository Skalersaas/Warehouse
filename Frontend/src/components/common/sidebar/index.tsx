import { useEffect, useRef, useState } from "react";
import styles from "./style.module.scss";
import {
  Archive,
  House,
  Menu,
  Package,
  ReceiptText,
  Ruler,
  TicketCheck,
  Users,
} from "lucide-react";
import { useLocation } from "react-router-dom";

const pages = [
  {
    id: 1,
    icon: <House width={20} />,
    name: "Dashboard",
    path: "/",
  },
  {
    id: 2,
    icon: <Package width={20} />,
    name: "Warehouse",
    path: "/balances",
  },
  {
    id: 3,
    icon: <ReceiptText width={20} />,
    name: "Receipts",
    path: "/receipts",
  },
  {
    id: 4,
    icon: <TicketCheck width={20} />,
    name: "Shipments",
    path: "/shipments",
  },
  {
    id: 5,
    icon: <Archive width={20} />,
    name: "Resources",
    path: "/resources",
  },
  {
    id: 6,
    icon: <Ruler width={20} />,
    name: "Units",
    path: "/units",
  },
  {
    id: 7,
    icon: <Users width={20} />,
    name: "Clients",
    path: "/clients",
  },
];

const Sidebar = () => {
  const [currentPage, setCurrentPage] = useState<string>("");
  const [sidebarStatus, setSidebarStatus] = useState(true);
  const menuRef = useRef<HTMLDivElement>(null);
  let location = useLocation();

  useEffect(() => {
    setCurrentPage("/" + location.pathname.split("/")[1]);
  }, [location]);

  const handleChangeStatus = () => {
    setSidebarStatus((sidebarStatus) => !sidebarStatus);
  };

  useEffect(() => {
    function updateStatus() {
      if (menuRef.current?.offsetHeight) {
        setSidebarStatus(false);
      } else {
        setSidebarStatus(true);
      }
    }
    updateStatus();
    window.addEventListener("resize", updateStatus);
    return () => window.removeEventListener("resize", updateStatus);
  }, []);

  return (
    <div className={`${styles["sidebar-container"]} ${!sidebarStatus && styles["inactive--sidebar-container"]}`} >
      <div
        className={styles["sidebar-container--icon"]}
        onClick={handleChangeStatus}
        ref={menuRef}
      >
        <Menu />
      </div>

      <div
        className={`${styles["sidebar"]} ${
          sidebarStatus ? styles["sidebar"] : styles["inactive--sidebar"]
        }`}
      >
        <div className={styles["sidebar__header"]}>
          <div className={styles["sidebar__header--logo"]}>
            <img src="/favicon.png" alt="Warehouse" />
          </div>
          <div className={styles["sidebar__header--name"]}>Warehouse</div>
        </div>
        <div className={styles["sidebar__body"]}>
          <div className={styles["sidebar__body--links"]}>
            {pages.map((page) => (
              <a
                href={page.path}
                key={page.id}
                className={`${styles["sidebar__body--links--link"]} ${
                  currentPage === page.path && styles["active"]
                }`}
              >
                <div className={styles["sidebar__body--links--link--logo"]}>
                  {page.icon}
                </div>
                <div className={styles["sidebar__body--links--link--name"]}>
                  {page.name}
                </div>
              </a>
            ))}
          </div>
        </div>
      </div>

      <div
        className={`${styles["sidebar__footer"]} ${
          sidebarStatus
            ? styles["sidebar__footer"]
            : styles["inactive--sidebar"]
        }`}
      >
        <span className={styles["sidebar__footer--text"]}>
         
        </span>
      </div>
    </div>
  );
};

export default Sidebar;
