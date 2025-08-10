import { Eye, User2 } from "lucide-react";
import styles from "./style.module.scss";
import type { ICommonType } from "../../../../types/common.type";

interface IProps {
  clients: ICommonType[];
}

const Clients = ({clients}: IProps) => {
  return (
    <div className={styles["client__container"]}>
      <div className={styles["client__container--title"]}>Clients</div>
      <div className={styles["client__section"]}>
        {clients?.map((user) => (
          <div key={user.name} className={styles["client__section--card"]}>
            <div className={styles["client__section--card--image"]}>
              <User2 />
            </div>
            <div className={styles["client__section--card--details"]}>
              <div className={styles["client__section--card--name"]}>
                {user.name}
              </div>
              <div className={styles["client__section--card--address"]}>
                {user?.address}
              </div>
            </div>
            <button className={styles["client__section--card--button"]}>
              <Eye width={12} />
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Clients;
