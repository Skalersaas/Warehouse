import ShipmentSection from "../../components/layout/dashboard/shipment";
import RecentDelivery from "../../components/layout/dashboard/transit";
import Clients from "../../components/layout/dashboard/client";
import TrackingDelivery from "../../components/layout/dashboard/map";
import styles from "./style.module.scss";
import Loader from "../../components/layout/dashboard/loader";

import { useDispatch } from "react-redux";
import { setLoading } from "../../store/features/app/appSlice";
import useApi from "../../hooks/useApi";
import { getClient, getShipment } from "../../services";
import { useEffect, useState } from "react";
import type { IClient, IShipment } from "../../types/common.type";

const Dashboard = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [shipmentData, setShipmentData] = useState<IShipment[]>([]);
  const [clientData, setClientData] = useState<IClient[]>([]);

  const fetchShipmentDocs = async () => {
    dispatch(setLoading(true));
    const response = await api(getShipment, {});
    setShipmentData(response.data ?? []);
    dispatch(setLoading(false));
  };

    const fetchClient = async () => {
    dispatch(setLoading(true));
    const response = await api(getClient, {});
    setClientData(response.data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchShipmentDocs();
    fetchClient();
  }, []);

  return (
    <div className={styles["dashboard-container"]}>
      <ShipmentSection shipmentDocs={shipmentData} />
      <div className={styles["delivery-section"]}>
        <div className={styles["delivery-section-loader"]}>
          <Loader shipmentDocs={shipmentData} />
        </div>
        <div className={styles["delivery-section-header"]}>
          <RecentDelivery shipmentDocs={shipmentData} />
          <Clients clients={clientData} />
        </div>
        <TrackingDelivery shipmentDocs={shipmentData} />
      </div>
    </div>
  );
};

export default Dashboard;
