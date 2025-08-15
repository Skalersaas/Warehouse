import { useEffect, useState } from "react";
import Table from "../../components/ui/table";
import styles from "./style.module.scss";
import {
  deleteShipment,
  getShipment,
  revokeShipment,
  signShipment,
} from "../../services";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { setLoading } from "../../store/features/app/appSlice";
import type { ICommonType } from "../../types/common.type";
import { Link } from "react-router-dom";
import Alert from "../../components/ui/alert";
import Pagination from "../../components/ui/pagination";

const ShipmentPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<ICommonType[]>([]);
  const [alertIsOpen, setAlertIsOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number>();
  const [alertAction, setAlertAction] = useState<string>("");
  const [signStatus, setSignStatus] = useState<number>();

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
    const { data, count } = await api(getShipment, {
      size: perPage,
      page: pageNumber,
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  const handleDelete = async () => {
    dispatch(setLoading(true));
    await api(deleteShipment, selectedId);
    const filteredData = data.filter((shipment) => shipment.id !== selectedId);
    setData(filteredData);
    dispatch(setLoading(false));
  };

  const handleSignRevoke = async () => {
    dispatch(setLoading(true));
    if (signStatus) {
      await api(revokeShipment, selectedId);
      fetchData();
    } else {
      await api(signShipment, selectedId);
      fetchData();
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchData();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["shipments-container"]}>
      <div className={styles["container-title"]}>
        <h1>Shipments Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/shipments/create">Create</Link>
        </div>
      </div>
      <Table
        data={data}
        isClient={false}
        page="shipments"
        setSelectedId={setSelectedId}
        setAlertIsOpen={setAlertIsOpen}
        setSignStatus={setSignStatus}
        setAlertAction={setAlertAction}
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
        onSignSuccess={handleSignRevoke}
        onDeleteSuccess={handleDelete}
      />
    </div>
  );
};

export default ShipmentPage;
