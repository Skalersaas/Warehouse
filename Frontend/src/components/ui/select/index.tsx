import styles from "./style.module.scss";
import { ChevronDown, ChevronUp } from "lucide-react";
import type { ICommonType } from "../../../types/common.type";
interface IProps {
  label: string;
  data: ICommonType[];
  value: { id: number; name: string; isArchived: boolean | null };
  setValue: (value: {
    id: number;
    name: string;
    isArchived: boolean | null;
  }) => void;
  setModal: (isOpen: boolean) => void;
  isOpen: boolean;
}

const Select = ({ label, data, value, setValue, setModal, isOpen }: IProps) => {
  return (
    <div className={styles["select__wrapper"]}>
      <div className={styles["select__wrapper--label"]}>{label}</div>

      <div className={styles["select__wrapper--box"]}>
        <div
          className={styles["select__wrapper--box--title"]}
          onClick={() => setModal(!isOpen)}
        >
          <div className={styles["select__wrapper--box--title--text"]}>
            {value.name ? `${value.name}` : `${label}`}
          </div>
          <div className={styles["dropdown"]}>
            {isOpen ? (
              <ChevronUp width={14} height={14} />
            ) : (
              <ChevronDown width={14} height={14} />
            )}
          </div>
        </div>
        {isOpen && data.length > 0 && (
          <div className={styles["select__wrapper--box--options"]}>
            {
              data?.map(
                (dt: {
                  name: string;
                  id: number;
                  isArchived: boolean | null;
                }) => (
                  <div
                    key={dt.id}
                    className={styles["select__wrapper--box--option"]}
                    onClick={() => {
                      setValue(dt);
                      setModal(false);
                    }}
                  >
                    {dt.name}
                  </div>
                )
              )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Select;
