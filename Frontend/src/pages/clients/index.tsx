import { useEffect, useState } from "react";
import Table from "../../components/ui/table";
import styles from "./style.module.scss";
import {
  archiveClient,
  deleteClient,
  getClient,
  unArchiveClient,
} from "../../services";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { setLoading } from "../../store/features/app/appSlice";
import type { IClient } from "../../types/common.type";
import { Link, useLocation } from "react-router-dom";
import Alert from "../../components/ui/alert";
import Pagination from "../../components/ui/pagination";
import Input from "../../components/ui/input";
import Select from "../../components/ui/select";
import Button from "../../components/ui/button";
import CreateClient from "./create";
import ClientDetail from "./detail";
import { successAlert } from "../../utils/toaster";

const searchState = {
  name: "",
  address: "",
  isArchived: "",
};

const ClientPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<IClient[]>([]);
  const [alertIsOpen, setAlertIsOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number>();
  const [archiveStatus, setArchiveStatus] = useState<boolean | null>(null);
  const [alertAction, setAlertAction] = useState<string>("");
  const location = useLocation();
  const [modal, setModal] = useState<{
    create: boolean;
    detail: boolean;
    select: boolean;
  }>({
    create: false,
    detail: false,
    select: false,
  });
  const [value, setValue] = useState<{
    id: string;
    name: string;
    isArchived: boolean | null;
  }>({
    id: "",
    name: "",
    isArchived: null,
  });

  const [totalRows, setTotalRows] = useState<number>(0);
  const [perPage, setPerPage] = useState<number>(10);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [searchData, setSearchData] = useState(searchState);

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
    const { data, count } = await api(getClient, {
      size: perPage,
      page: pageNumber,
      filters: {
        name: searchData.name,
        address: searchData.address,
        isArchived: String(value.isArchived),
      },
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  const handleDelete = async () => {
    dispatch(setLoading(true));
    const res = await api(deleteClient, selectedId);
    if (res?.success) {
      const filteredData = data.filter((client) => client.id !== selectedId);
      setData(filteredData);
      successAlert("Successfully deleted!");
    }
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (archiveStatus) {
      const res = await api(unArchiveClient, selectedId);
      if (res.success) {
        successAlert(res.message);
        fetchData();
      }
    } else {
      const res = await api(archiveClient, selectedId);
      if (res.success) {
        successAlert(res.message);
        fetchData();
      }
    }
    dispatch(setLoading(false));
  };

  const handleSearch = (e: any) => {
    const { name, value } = e.target;
    setSearchData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  useEffect(() => {
    if (location.pathname === "/clients") {
      fetchData();
    }
  }, [perPage, pageNumber, location.pathname]);

  useEffect(() => {
    if (location.pathname === "/clients") {
      setModal((prev) => ({ ...prev, create: false, detail: false }));
    }
  }, [location.pathname]);

  return (
    <div className={styles["clients-container"]}>
      <div className={styles["clients-container-title"]}>
        <h1>Clients Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/clients/create">Create</Link>
        </div>
      </div>

      <div className={styles["clients-container-search"]}>
        <Input
          label="Client Name"
          placeholder="name"
          name="name"
          onChange={handleSearch}
        />

        <Input
          label="Client Address"
          placeholder="address"
          name="address"
          onChange={handleSearch}
        />

        <Select
          label="Archive Status"
          data={[
            { id: 0, name: "Default", isArchived: null },
            { id: 1, name: "Archived", isArchived: true },
            { id: 2, name: "Not Archived", isArchived: false },
          ]}
          value={value}
          setValue={(val) => setValue(val)}
          setModal={(isOpen) =>
            setModal((prev) => ({ ...prev, select: isOpen }))
          }
          isOpen={modal.select}
        />

        <Button onClick={fetchData}>Search</Button>
      </div>

      <Table
        data={data}
        isClient={true}
        page="clients"
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
      <CreateClient
        isOpen={modal.create}
        setModal={(isOpen) => setModal((prev) => ({ ...prev, create: isOpen }))}
      />
      <ClientDetail
        isOpen={modal.detail}
        setModal={(isOpen) => setModal((prev) => ({ ...prev, detail: isOpen }))}
      />
    </div>
  );
};

export default ClientPage;
