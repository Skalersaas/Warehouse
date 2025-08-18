import styles from "./style.module.scss";
import type { ICommonDocument } from "../../../types/common.type";
import {
  FilePenLineIcon,
  Info,
  Trash,
  Undo2,
} from "lucide-react";
import { Link } from "react-router-dom";
import type { Dispatch, SetStateAction } from "react";
import { formatDate } from "../../../utils/dateFormatter";

interface IProps {
  data: ICommonDocument[];
  page: string;
  setAlertIsOpen: Dispatch<SetStateAction<boolean>>;
  setSelectedId: Dispatch<SetStateAction<number | undefined>>;
  setSignStatus?: Dispatch<SetStateAction<number | undefined>>;
  setAlertAction?: Dispatch<SetStateAction<string>>;
}

const DocumentTable = ({
  data,
  page,
  setAlertIsOpen,
  setSelectedId,
  setSignStatus,
  setAlertAction,
}: IProps) => {
  return (
    <div className={styles["table__container"]}>
      {data.length ? (
        data.map((dt) => (
          <div key={dt.id} className={styles["table__container--row"]}>
            <div className={styles["table__container--row--column"]}>
              {dt.number}
            </div>
            {dt.clientId && (
              <div className={styles["table__container--row--column"]}>
                {dt.clientName}
              </div>
            )}
            <div className={styles["table__container--row--column"]}>
              {formatDate(dt.date)}
            </div>
            <div className={styles["table__container--row--actions"]}>
              <Link
                to={`/${page}/${dt.id}`}
                className={styles["table__container--row--action--detail"]}
              >
                <Info width={14} />
              </Link>
              {["shipments"].includes(page) && (
                <div
                  className={styles["table__container--row--action--sign"]}
                  onClick={() => {
                    setAlertIsOpen(true);
                    setSelectedId(dt.id);
                    setSignStatus?.(dt?.status);
                    setAlertAction?.("sign");
                  }}
                >
                  {dt.status === 1 ? (
                    <Undo2 width={14} />
                  ) : (
                    <FilePenLineIcon width={14} />
                  )}
                </div>
              )}
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

export default DocumentTable;
