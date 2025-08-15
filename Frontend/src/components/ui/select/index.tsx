import styles from "./style.module.scss";
import { ChevronDown, ChevronUp } from "lucide-react";
import type { ICommonType } from "../../../types/common.type";
interface IProps {
  label: string;
  data: ICommonType[];
  value: { id: number; name: string };
  setValue: (value: { id: number; name: string }) => void;
  setModal: (isOpen: boolean) => void;
  isOpen: boolean;
}

const Select = ({ label, data, value, setValue, setModal, isOpen }: IProps) => {
  return (
    <div className={styles["select__wrapper"]}>
      <div className={styles["select__wrapper--label"]}>Select {label}</div>

      <div className={styles["select__wrapper--box"]}>
        <div
          className={styles["select__wrapper--box--title"]}
          onClick={() => setModal(!isOpen)}
        >
          {value.name ? `${value.name}` : `Select ${label}`}
          <div className={styles["dropdown"]}>
            {isOpen ? (
              <ChevronUp width={14} height={14} />
            ) : (
              <ChevronDown width={14} height={14} />
            )}
          </div>
        </div>
        {isOpen && (
          <div className={styles["select__wrapper--box--options"]}>
            {data &&
              data.map((dt: { name: string; id: number }) => (
                <div
                  key={dt.id}
                  className={styles["select__wrapper--box--option"]}
                  onClick={() => setValue(dt)}
                >
                  {dt.name}
                </div>
              ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Select;
