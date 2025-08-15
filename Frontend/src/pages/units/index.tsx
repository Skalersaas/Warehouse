import { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import Table from "../../components/ui/table";
import useApi from "../../hooks/useApi";
import {
  archiveUnit,
  deleteUnit,
  getUnit,
  unArchiveUnit,
} from "../../services";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";
import styles from "./style.module.scss";
import { Link } from "react-router-dom";
import Alert from "../../components/ui/alert";
import Pagination from "../../components/ui/pagination";

const UnitPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);
  const [alertIsOpen, setAlertIsOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number>();
  const [archiveStatus, setArchiveStatus] = useState<boolean>();
  const [alertAction, setAlertAction] = useState<string>("");

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
    const { data, count } = await api(getUnit, {
      size: perPage,
      page: pageNumber,
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  const handleDelete = async () => {
    dispatch(setLoading(true));
    await api(deleteUnit, selectedId);
    const filteredData = data.filter((unit) => unit.id !== selectedId);
    setData(filteredData);
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (archiveStatus) {
      await api(unArchiveUnit, selectedId);
      fetchData();
    } else {
      await api(archiveUnit, selectedId);
      fetchData();
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["units-container"]}>
      <div className={styles["container-title"]}>
        <h1>Unit Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/units/create">Create</Link>
        </div>
      </div>
      <Table
        data={data}
        isClient={false}
        page="units"
        setArchiveStatus={setArchiveStatus}
        setAlertAction={setAlertAction}
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
        alertAction={alertAction}
        onArchiveSuccess={handleArchive}
        onDeleteSuccess={handleDelete}
      />
    </div>
  );
};

export default UnitPage;
