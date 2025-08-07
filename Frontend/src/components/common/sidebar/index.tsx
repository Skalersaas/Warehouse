import React from "react";
import styles from "./style.module.scss";

const Sidebar = () => {
  return (
    <div className={styles["sidebar"]}>
      <div className="sidebar__header">
        <div className="sidebar__header--logo"></div>
        <div className="sidebar__header--name"></div>
      </div>
      <div className="sidebar__body">
        <div className="sidebar__body--links">
          <div className="sidebar__body--links--main">
            <div className="logo"></div>
            <div className="name"></div>
          </div>

          <div className="sidebar__body--links--footer">
            <div className="logo"></div>
            <div className="name"></div>
          </div>
        </div>
      </div>
      <div className="sidebar__footer">
        <span></span>
      </div>
    </div>
  );
};

export default Sidebar;
