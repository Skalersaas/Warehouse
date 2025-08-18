import { useEffect, useState } from "react";
import styles from "./style.module.scss";
import type { IShipmentDocument } from "../../../../types/common.type";
import { Calendar, Check, Package } from "lucide-react";
import Loader from "../loader";
import { formatDate } from "../../../../utils/dateFormatter";
import { useDispatch } from "react-redux";
import { setLoading } from "../../../../store/features/app/appSlice";
import useApi from "../../../../hooks/useApi";
import { getShipment } from "../../../../services";

const loadBtns = [
  {
    id: 0,
    name: "Signed",
  },
  {
    id: 1,
    name: "Draft",
  },
];

interface IProps {
  shipmentDocs: IShipmentDocument[];
}

const ShipmentSection = ({ shipmentDocs }: IProps) => {
  const api = useApi();
  const dispatch = useDispatch();

  const [activeStatus, setActiveStatus] = useState<string>("Signed");
  const handleClick = (name: string) => {
    setActiveStatus(name);
  };

  const [loadPacks, setLoadPacks] = useState<IShipmentDocument[]>();
  const fetchShipmentDocs = async () => {
    dispatch(setLoading(true));
    const response = await api(getShipment, {
      filters: {
        status: activeStatus,
      },
    });
    setLoadPacks(response.data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchShipmentDocs();
  }, [activeStatus]);

  return (
    <div className={styles["shipment__container"]}>
      <div className={styles["shipment__container--title"]}>
        Shipment Services
      </div>
      <div className={styles["shipment__container--loader"]}>
        <Loader shipmentDocs={shipmentDocs} />
      </div>
      <div className={styles["shipment__section"]}>
        <div className={styles["shipment__section--header"]}>
          <div className={styles["shipment__section--header--text"]}>
            Loading Packages
          </div>
          <div className={styles["shipment__section--buttons"]}>
            {loadBtns.map((btn) => (
              <button
                key={btn.id}
                onClick={() => handleClick(btn.name)}
                className={` ${styles["shipment__section--button"]} ${
                  activeStatus === btn.name && styles["active--button"]
                }`}
              >
                {btn.name}
              </button>
            ))}
          </div>
        </div>

        <div className={styles["shipment__section--body"]}>
          {loadPacks?.map((doc) => {
            return (
              <div
                key={doc.id}
                className={styles["shipment__section--body--card"]}
              >
                <div className={styles["shipment__section--body--card--title"]}>
                  <div className={styles["shipment__section--client"]}>
                    {doc.clientName}
                  </div>
                  <div className={styles["shipment__section--status"]}>
                    <Check width={12} />
                    {doc.status === 1 ? "Signed" : "Draft"}
                  </div>
                </div>

                <div
                  className={styles["shipment__section--body--card--resources"]}
                >
                  {doc.items.map((rs, i) => (
                    <div
                      key={i}
                      className={
                        styles["shipment__section--body--card--resource"]
                      }
                    >
                      <div
                        className={
                          styles[
                            "shipment__section--body--card--resource--detail"
                          ]
                        }
                      >
                        <div
                          className={
                            styles[
                              "shipment__section--body--card--resource--detail--icon"
                            ]
                          }
                        >
                          <Package />
                        </div>
                        <div
                          className={
                            styles[
                              "shipment__section--body--card--resource--detail--text"
                            ]
                          }
                        >
                          {rs.resourceName}
                        </div>
                      </div>
                      <div
                        className={
                          styles[
                            "shipment__section--body--card--resource--detail"
                          ]
                        }
                      >
                        <div
                          className={
                            styles[
                              "shipment__section--body--card--resource--detail--text"
                            ]
                          }
                        >
                          {rs.quantity}
                        </div>
                        <div
                          className={
                            styles[
                              "shipment__section--body--card--resource--detail--text"
                            ]
                          }
                        >
                          {rs.unitName}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                <div
                  key={doc.id}
                  className={styles["shipment__section--body--card--footer"]}
                >
                  <div className={styles["shipment__section--number"]}>
                    #{doc.number}
                  </div>
                  <div className={styles["shipment__section--date"]}>
                    <Calendar width={12} /> {formatDate(doc.date)}
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

export default ShipmentSection;
