import styles from "./style.module.scss";
import type { ICommonType } from "../../../types/common.type";
import { Archive, ArchiveX, Info, Trash } from "lucide-react";
import { Link } from "react-router-dom";
import type { Dispatch, SetStateAction } from "react";

interface IProps {
  data: ICommonType[];
  isClient?: boolean;
  page: string;
  setAlertIsOpen: Dispatch<SetStateAction<boolean>>;
  setSelectedId: Dispatch<SetStateAction<number | undefined>>;
  setArchiveStatus?: Dispatch<SetStateAction<boolean | null>>;
  setAlertAction?: Dispatch<SetStateAction<string>>;
}

const Table = ({
  data,
  isClient,
  page,
  setAlertIsOpen,
  setSelectedId,
  setArchiveStatus,
  setAlertAction,
}: IProps) => {
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
                <div
                  className={styles["table__container--row--action--archive"]}
                  onClick={() => {
                    setAlertIsOpen(true);
                    setSelectedId(dt.id);
                    setArchiveStatus?.(dt.isArchived);
                    setAlertAction?.("archive");
                  }}
                >
                  {dt.isArchived ? (
                    <ArchiveX width={14} />
                  ) : (
                    <Archive width={14} />
                  )}
                </div>
              <div
                className={styles["table__container--row--action--delete"]}
                onClick={() => {
                  setAlertIsOpen(true);
                  setSelectedId(dt.id);
                  setAlertAction?.("delete");
                }}
              >
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
