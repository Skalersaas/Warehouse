import type { IShipmentDocument } from "../../../../types/common.type";
import styles from "./style.module.scss";
import { useDispatch } from "react-redux";
import { setLoading } from "../../../../store/features/app/appSlice";
import useApi from "../../../../hooks/useApi";
import { getShipment } from "../../../../services";
import { useEffect, useState } from "react";

interface IProps {
  shipmentDocs: IShipmentDocument[];
}

const Loader = ({ shipmentDocs }: IProps) => {
  const api = useApi();
  const dispatch = useDispatch();

  const [loadPacks, setLoadPacks] = useState<IShipmentDocument[]>([]);
  const fetchShipmentDocs = async () => {
    dispatch(setLoading(true));
    const response = await api(getShipment, {
      filters: {
        status: "0",
      },
    });
    setLoadPacks(response.data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchShipmentDocs();
  }, []);

  return (
    <div className={styles["shipment__loader"]}>
      <div className={styles["shipment__loader--bar"]}>
        <div
          className={styles["shipment__loader--bar--active"]}
          style={{
            width: `${(loadPacks?.length / shipmentDocs?.length) * 100}%`,
          }}
        ></div>
      </div>
      <div className={styles["shipment__loader--details"]}>
        <div className={styles["shipment__loader--details--load"]}>
          {loadPacks.length}
        </div>
        <p>of {shipmentDocs?.length} Loaded</p>
      </div>
      <button className={styles["shipment__loader--button"]}>See</button>
    </div>
  );
};

export default Loader;
