import { Check, Maximize2, Minimize2, Truck } from "lucide-react";
import { useEffect, useState } from "react";
import styles from "./style.module.scss";
import type { IShipmentDocument } from "../../../../types/common.type";
import { useDispatch } from "react-redux";
import { setLoading } from "../../../../store/features/app/appSlice";
import useApi from "../../../../hooks/useApi";
import { getShipment } from "../../../../services";

const TrackingDelivery = () => {
  const api = useApi();
  const dispatch = useDispatch();

  const [loadedPacks, setLoadedPacks] = useState<IShipmentDocument[]>([]);
  const [allLoadedPacks, setAllLoadedPacks] = useState<IShipmentDocument[]>([]);
  const [modalStatus, setModalStatus] = useState(true);
  const handleChangeStatus = () => {
    setModalStatus((modalStatus) => !modalStatus);
  };

  const fetchShipmentDocs = async (status: number) => {
    dispatch(setLoading(true));
    const response = await api(getShipment, {
      filters: {
        status: String(status),
      },
    });
    dispatch(setLoading(false));
    return response?.data ?? [];
  };

  useEffect(() => {
    const fetchData = async () => {
      const loadedDocs = await fetchShipmentDocs(0);
      const unLoadedDocs = await fetchShipmentDocs(1);

      const filterLoadedDocuments = loadedDocs.sort(
        (a: { date: string }, b: { date: string }) =>
          a.date.localeCompare(b.date)
      );
      const filterUnLoadedDocuments = unLoadedDocs.sort(
        (a: { date: string }, b: { date: string }) =>
          a.date.localeCompare(b.date)
      );
      setLoadedPacks(filterLoadedDocuments);
      setAllLoadedPacks([...filterLoadedDocuments, ...filterUnLoadedDocuments]);
    };

    fetchData();
  }, []);

  return (
    <div className={styles["tracking__delivery"]}>
      <iframe
        className={styles["tracking__delivery--map"]}
        src="https://www.google.com/maps/embed?pb=!1m14!1m12!1m3!1d97252.92244912653!2d49.8466816!3d40.3832832!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!5e0!3m2!1sen!2saz!4v1754750659983!5m2!1sen!2saz"
        width={100}
        height={100}
        style={{ border: 0 }}
        allowFullScreen={true}
        loading="lazy"
        referrerPolicy="no-referrer-when-downgrade"
      ></iframe>

      <div
        className={`${styles["tracking__delivery--card"]} ${
          !modalStatus ? styles["tracking__delivery--inactive--card"] : ""
        }`}
      >
        <div
          className={styles["tracking__delivery--card--close"]}
          onClick={handleChangeStatus}
        >
          {modalStatus ? <Minimize2 width={12} /> : <Maximize2 width={12} />}
        </div>
        <div
          className={`${styles["tracking__delivery--card--title"]} ${
            modalStatus ? "" : styles["inactive--title"]
          }`}
        >
          <div className={styles["tracking__delivery--card--title--text"]}>
            Tracking Delivery
          </div>
          <div className={styles["tracking__delivery--card--title--number"]}>
            ID#{loadedPacks?.pop()?.number}
          </div>
        </div>

        <div
          className={`${styles["tracking__delivery--card--track"]} ${
            modalStatus ? "" : styles["inactive--track"]
          }`}
        >
          {allLoadedPacks?.map((packs) => {
            return (
              <div
                key={packs.id}
                className={styles["tracking__delivery--card--track--default"]}
              >
                <div
                  className={`${
                    styles["tracking__delivery--card--track--default--status"]
                  } ${packs.status === 0 && styles["active__status"]}`}
                >
                  <div
                    className={`${
                      styles[
                        "tracking__delivery--card--track--default--status--icon"
                      ]
                    } ${packs.status === 0 && styles["active__status--icon"]}`}
                  >
                    {packs.status === 0 &&
                    packs ===
                      loadedPacks?.pop() ? (
                      <Truck width={10} />
                    ) : packs.status === 0 ? (
                      <Check width={10} />
                    ) : (
                      ""
                    )}
                  </div>
                  <div
                    className={`${
                      styles[
                        "tracking__delivery--card--track--default--status--line"
                      ]
                    } ${packs.status === 0 && styles["active__status--line"]}`}
                  ></div>
                </div>

                <div
                  className={
                    styles["tracking__delivery--card--track--default--detail"]
                  }
                >
                  <div
                    className={
                      styles[
                        "tracking__delivery--card--track--default--detail--text"
                      ]
                    }
                  >
                    <div
                      className={
                        styles[
                          "tracking__delivery--card--track--default--detail--client"
                        ]
                      }
                    >
                      {packs.clientName}{" "}
                    </div>
                  </div>

                  <div
                    className={
                      styles[
                        "tracking__delivery--card--track--default--detail--resources"
                      ]
                    }
                  >
                    {packs.items.map((res) => res.resourceName).join(", ")}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default TrackingDelivery;
