import styles from "./style.module.scss";
import { Bell, ChevronDown, Search, Settings, User } from "lucide-react";

const Navbar = () => {
  return (
    <div className={styles["navbar"]}>
      <div className={styles["navbar__search"]}>
        <div className={styles["navbar__search--icon"]}>
          <Search width={18} color="#555555" />
        </div>
        <input
          className={styles["navbar__search--input"]}
          type="text"
          name="search"
          id="search"
          placeholder="Search"
        />
      </div>
      <div className={styles["navbar__auth"]}>
        <div className={styles["navbar__auth--settings"]}>
          <Settings width={18} />
        </div>
        <div className={styles["navbar__auth--bell"]}>
          <Bell width={18} />
        </div>
        <div className={styles["navbar__auth--user--profile"]}>
          <div className={styles["navbar__auth--user--profile--logo"]}>
            <User width={18} />
          </div>
          <div className={styles["navbar__auth--user--profile--name"]}>Emin Amirov</div>
          <div className={styles["navbar__auth--user--profile--icon"]}>
            <ChevronDown width={14} height={14}/>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Navbar;
