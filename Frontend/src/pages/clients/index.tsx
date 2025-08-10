import { useEffect, useState } from "react";
import Table from "../../components/ui/table";
import styles from "./style.module.scss";
import { getClient } from "../../services";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";

const ClientPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);

  const fetchData = async () => {
    dispatch(setLoading(true));
    const data = await api(getClient, {});
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, []);

  return (
    <div className={styles["clients-container"]}>
      <h1>Clients Page</h1>
      <Table data={data} isClient={true} page="clients" />
    </div>
  );
};

export default ClientPage;
