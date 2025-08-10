import { useEffect, useState } from "react";
import styles from "./style.module.scss";
import type { IShipment } from "../../../../types/common.type";
import { Calendar, Check, Package } from "lucide-react";
import Loader from "../loader";
import formatDate from "../../../../utils/dateFormatter";

const loadBtns = [
  {
    id: 0,
    name: "Loaded",
  },
  {
    id: 1,
    name: "UnLoaded",
  },
];

interface IProps {
  shipmentDocs: IShipment[];
}

const ShipmentSection = ({ shipmentDocs }: IProps) => {
  const [activeStatus, setActiveStatus] = useState(0);
  const handleClick = (id: number) => {
    setActiveStatus(id);
  };

  const [loadPacks, setLoadPacks] = useState<IShipment[]>();

  useEffect(() => {
    const filteredDocuments = shipmentDocs?.filter(
      (doc: IShipment) => doc.status === activeStatus
    );

    setLoadPacks(filteredDocuments);
  }, [activeStatus]);

  return (
    <div className={styles["shipment__container"]}>
      <div className={styles["shipment__container--title"]}>
        Shipment Services
      </div>
      <div className={styles["shipment__container--loader"]}>
        <Loader shipmentDocs={shipmentDocs}/>
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
                onClick={() => handleClick(btn.id)}
                className={` ${styles["shipment__section--button"]} ${
                  activeStatus === btn.id && styles["active--button"]
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
                    <Check width={12} /> {doc.statusName}
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
                        {rs.resourceName}
                      </div>
                      <div
                        className={
                          styles[
                            "shipment__section--body--card--resource--detail"
                          ]
                        }
                      >
                        <div>{rs.quantity}</div>
                        <div>{rs.unitName}</div>
                      </div>
                    </div>
                  ))}
                </div>

                <div
                  key={doc.id}
                  className={styles["shipment__section--body--card--footer"]}
                >
                  <div className={styles["shipment__section--number"]}>
                    ID#{doc.number}
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
