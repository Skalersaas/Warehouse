import { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { getBalance } from "../../services";
import { setLoading } from "../../store/features/app/appSlice";
import type { IBalance } from "../../types/common.type";
import styles from "./style.module.scss";
import BalanceTable from "../../components/ui/balanceTable";
import Pagination from "../../components/ui/pagination";

const BalancePage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<IBalance[]>([]);

  const [totalRows, setTotalRows] = useState<number>(0);
  const [perPage, setPerPage] = useState<number>(10);
  const [pageNumber, setPageNumber] = useState<number>(1);

  const handlePageChange = (pageNumber: number) => {
    setPageNumber(pageNumber);
  };
  const handlePerRowsChange = async (
    newPerPage?: number,
    pageNumber?: number
  ) => {
    setPerPage(newPerPage || 0);
    setPageNumber(pageNumber || 0);
  };

  const fetchData = async () => {
    dispatch(setLoading(true));
    const { data, count } = await api(getBalance, {
      size: perPage,
      page: pageNumber,
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["balances-container"]}>
      <div className={styles["container-title"]}>
        <h1>balance Page</h1>
      </div>
      <BalanceTable data={data} />
      <Pagination
        perPage={perPage}
        totalRows={totalRows}
        currentPage={pageNumber}
        handlePageChange={handlePageChange}
        handlePerRowsChange={handlePerRowsChange}
      />
    </div>
  );
};

export default BalancePage;
