import { useEffect, useState } from "react";
import Table from "../../components/ui/table";
import styles from "./style.module.scss";
import { deleteReceipt, getReceipt } from "../../services";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";
import { Link } from "react-router-dom";
import Alert from "../../components/ui/alert";
import Pagination from "../../components/ui/pagination";

const ReceiptPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);
  const [alertIsOpen, setAlertIsOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number>();

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
    const { data, count } = await api(getReceipt, {
      size: perPage,
      page: pageNumber,
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  const handleDelete = async () => {
    dispatch(setLoading(true));
    await api(deleteReceipt, selectedId);
    const filteredData = data.filter((receipt) => receipt.id !== selectedId);
    setData(filteredData);
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["receipts-container"]}>
      <div className={styles["container-title"]}>
        <h1>Receipts Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/receipts/create">Create</Link>
        </div>
      </div>
      <Table
        data={data}
        isClient={false}
        page="receipts"
        setSelectedId={setSelectedId}
        setAlertIsOpen={setAlertIsOpen}
      />
      <Pagination
        perPage={perPage}
        totalRows={totalRows}
        currentPage={pageNumber}
        handlePageChange={handlePageChange}
        handlePerRowsChange={handlePerRowsChange}
      />
      <Alert
        isOpen={alertIsOpen}
        setOpen={setAlertIsOpen}
        alertAction="delete"
        onDeleteSuccess={handleDelete}
      />
    </div>
  );
};

export default ReceiptPage;
