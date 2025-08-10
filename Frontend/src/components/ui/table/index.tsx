import styles from "./style.module.scss";
import type { ICommonType } from "../../../types/common.type";
import { Archive, ArchiveX, Info, Trash } from "lucide-react";
import { Link } from "react-router-dom";

interface IProps {
  data: ICommonType[];
  isClient?: boolean;
  page: string;
}

const Table = ({ data, isClient, page }: IProps) => {
  return (
    <div className={styles["table__container"]}>
      {data.length ? (
        data.map((dt) => (
          <div key={dt.id} className={styles["table__container--row"]}>
            <div className={styles["table__container--row--column"]}>
              {dt.name}
            </div>
            {isClient && (
              <div className={styles["table__container--row--column"]}>
                {dt?.address}
              </div>
            )}
            <div className={styles["table__container--row--actions"]}>
              <Link
                to={`/${page}/${dt.id}`}
                className={styles["table__container--row--action--detail"]}
              >
                <Info width={14} />
              </Link>
              <div className={styles["table__container--row--action--archive"]}>
                {dt.isArchived ? (
                  <ArchiveX width={14} />
                ) : (
                  <Archive width={14} />
                )}
              </div>
              <div className={styles["table__container--row--action--delete"]}>
                <Trash width={14} />
              </div>
            </div>
          </div>
        ))
      ) : (
        <div className={styles["table__container--row"]}>No Data Available</div>
      )}
    </div>
  );
};

export default Table;
