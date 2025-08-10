import { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import Table from "../../components/ui/table";
import useApi from "../../hooks/useApi";
import { getUnit } from "../../services";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";
import styles from "./style.module.scss";

const UnitPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);

  const fetchData = async () => {
    dispatch(setLoading(true));
    const data = await api(getUnit, {});
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, []);

  return (
    <div className={styles["units-container"]}>
      <h1>Units Page</h1>
      <Table data={data} page="units" />
    </div>
  );
};

export default UnitPage;
