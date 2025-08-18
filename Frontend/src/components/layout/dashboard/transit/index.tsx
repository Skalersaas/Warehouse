import { useEffect, useRef, useState } from "react";
import styles from "./style.module.scss";
import type { IShipmentDocument } from "../../../../types/common.type";
import { formatDate } from "../../../../utils/dateFormatter";
import { getShipment } from "../../../../services";
import { useDispatch } from "react-redux";
import { setLoading } from "../../../../store/features/app/appSlice";
import useApi from "../../../../hooks/useApi";

const RecentDelivery = () => {
  const api = useApi();
  const dispatch = useDispatch();

  const [unloadPacks, setLoadPacks] = useState<IShipmentDocument[]>([]);
  const fetchShipmentDocs = async () => {
    dispatch(setLoading(true));
    const response = await api(getShipment, {
      filters: {
        status: "Draft",
      },
    });
    setLoadPacks(response.data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchShipmentDocs();
  }, []);

  const currentDelivery =
    unloadPacks.length > 0
      ? unloadPacks?.reduce((prev, current) => {
          const prevDate = new Date(prev.date.split(".").reverse().join("-"));
          const currentDate = new Date(
            current.date.split(".").reverse().join("-")
          );
          return currentDate < prevDate ? current : prev;
        })
      : null;

  const loaderRef = useRef<HTMLDivElement>(null);
  const [piecesCount, setPiecesCount] = useState<number>(0);

  useEffect(() => {
    function updatePiecesCount() {
      if (loaderRef.current) {
        const loaderWidth = loaderRef.current.offsetWidth;
        const usableWidth = loaderWidth;
        const count = Math.floor(usableWidth / (4 + 3));
        setPiecesCount(count);
      }
    }
    updatePiecesCount();
    window.addEventListener("resize", updatePiecesCount);
    return () => window.removeEventListener("resize", updatePiecesCount);
  }, []);

  return (
    <div className={styles["recent__delivery"]}>
      <div className={styles["recent__delivery--number"]}>
        #{currentDelivery?.number}
      </div>
      <div className={styles["recent__delivery--detail"]}>
        <div className={styles["recent__delivery--detail--text"]}>
          <div>Load Client:</div>
          <div className={styles["recent__delivery--detail--text--sub"]}>
            {currentDelivery?.clientName}
          </div>
        </div>
        <div className={styles["recent__delivery--detail--text"]}>
          <div>Load Date:</div>
          <div className={styles["recent__delivery--detail--text--sub"]}>
            {formatDate(currentDelivery?.date || "")}
          </div>
        </div>
      </div>
      <div className={styles["recent__delivery--image"]}>
        <img src="./src/assets/images/car.png" alt="car" />
      </div>
      <div ref={loaderRef} className={styles["recent__delivery--time--loader"]}>
        <div
          className={styles["recent__delivery--time--loader--active--pieces"]}
        >
          {Array.from({ length: Math.round(piecesCount * 0.8) }).map((_, i) => (
            <div key={`active-${i}`} className={styles["piece"]}></div>
          ))}
        </div>
        <div
          className={styles["recent__delivery--time--loader--inactive--pieces"]}
        >
          {Array.from({
            length: piecesCount - Math.round(piecesCount * 0.8),
          }).map((_, i) => (
            <div key={`inactive-${i}`} className={styles["piece"]}></div>
          ))}
        </div>
      </div>
      <div className={styles["recent__delivery--result"]}>
        {"80%"} Almost Done!
      </div>
    </div>
  );
};

export default RecentDelivery;
