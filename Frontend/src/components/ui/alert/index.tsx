import styles from "./style.module.scss";

interface AlertTypes {
  isOpen: boolean;
  setOpen: Function;
  alertAction: string;
  onDeleteSuccess: Function;
  onArchiveSuccess?: Function;
  onSignSuccess?: Function;
  message?: string;
}

const Alert = ({
  isOpen,
  setOpen,
  message,
  onDeleteSuccess,
  onArchiveSuccess,
  onSignSuccess,
  alertAction,
}: AlertTypes) => {

  const success = async () => {
    if (alertAction === "delete") await onDeleteSuccess();
    else if (alertAction === "archive") await onArchiveSuccess?.();
    else if (alertAction === "sign") await onSignSuccess?.();

    setOpen(false);
  };
  const cancel = () => {
    setOpen(false);
  };

  const handleFunc = (e: any) => {
    if (e.target.classList.value.includes("alert-backdrop")) {
      cancel();
    }
  };
  return (
    <div
      id={styles["alert"]}
      className={`${styles["alert-backdrop"]} ${
        isOpen ? styles["active"] : styles[""]
      }`}
      onClick={handleFunc}
    >
      <div className={styles["alert-box"]}>
        <div className={styles["content-body"]}>
          {message || "Are you sure ?"}
        </div>
        <div className={styles["footer"]}>
          <button onClick={cancel}>Cancel</button>
          <button onClick={success} style={{backgroundColor: `${alertAction === "delete" ? "#1359d1" : "#d19b13" }`}}>Confirm</button>
        </div>
      </div>
    </div>
  );
};

export default Alert;
