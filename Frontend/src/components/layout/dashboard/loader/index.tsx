import type { IShipment } from "../../../../types/common.type";
import styles from "./style.module.scss";

interface IProps {
  shipmentDocs: IShipment[];
}

const Loader = ({ shipmentDocs }: IProps) => {
  const loadedPacks = shipmentDocs?.filter((doc) => doc.status === 0);
  return (
    <div className={styles["shipment__loader"]}>
      <div className={styles["shipment__loader--bar"]}>
        <div
          className={styles["shipment__loader--bar--active"]}
          style={{
            width: `${(loadedPacks?.length / shipmentDocs?.length) * 100}%`,
          }}
        ></div>
      </div>
      <div className={styles["shipment__loader--details"]}>
        <div className={styles["shipment__loader--details--load"]}>
          {loadedPacks.length}
        </div>
        <p>of {shipmentDocs?.length} Loaded</p>
      </div>
      <button className={styles["shipment__loader--button"]}>See</button>
    </div>
  );
};

export default Loader;
