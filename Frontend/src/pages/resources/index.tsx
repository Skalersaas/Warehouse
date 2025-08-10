import { useDispatch } from "react-redux";
import Table from "../../components/ui/table";
import useApi from "../../hooks/useApi";
import styles from "./style.module.scss";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";
import { useEffect, useState } from "react";
import { getResource } from "../../services";

const ResourcePage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);

  const fetchData = async () => {
    dispatch(setLoading(true));
    const data = await api(getResource, {});
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, []);

  return (
    <div className={styles["resources-container"]}>
      <h1>Resources Page</h1>
      <Table data={data} page="resources" />
    </div>
  );
};

export default ResourcePage;
