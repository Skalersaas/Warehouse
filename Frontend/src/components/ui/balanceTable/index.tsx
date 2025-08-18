import styles from "./style.module.scss";
import type { IBalance } from "../../../types/common.type";

interface IProps {
  data: IBalance[];
}

const BalanceTable = ({ data }: IProps) => {
  return (
    <div className={styles["balance__table__container"]}>
      {data.length > 0 ? (
        data?.map((dt) => (
          <div key={dt.id} className={styles["balance__table__container--row"]}>
            <div className={styles["balance__table__container--row--column"]}>
              {dt.resourceName}
            </div>
            <div className={styles["balance__table__container--row--column"]}>
              {dt.unitName}
            </div>
            <div className={styles["balance__table__container--row--column"]}>
              {dt.quantity}
            </div>
          </div>
        ))
      ) : (
        <div className={styles["balance__table__container--row"]}>No Data Available</div>
      )}
    </div>
  );
};

export default BalanceTable;
