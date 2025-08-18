import { useDispatch } from "react-redux";
import Table from "../../components/ui/table";
import useApi from "../../hooks/useApi";
import styles from "./style.module.scss";
import { setLoading } from "../../store/features/app/appSlice";
import type { IResource } from "../../types/common.type";
import { useEffect, useState } from "react";
import {
  archiveResource,
  deleteResource,
  getResource,
  unArchiveResource,
} from "../../services";
import Alert from "../../components/ui/alert";
import { Link, useLocation } from "react-router-dom";
import Pagination from "../../components/ui/pagination";
import Input from "../../components/ui/input";
import Select from "../../components/ui/select";
import Button from "../../components/ui/button";
import CreateResource from "./create";
import ResourceDetail from "./detail";
import { successAlert } from "../../utils/toaster";

const searchState = {
  name: "",
  isArchived: "",
};

const ResourcePage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<IResource[]>([]);
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
    const { data, count } = await api(getResource, {
      size: perPage,
      page: pageNumber,
      filters: {
        name: searchData.name,
        isArchived: String(value.isArchived),
      },
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };

  const handleDelete = async () => {
    dispatch(setLoading(true));
    const res = await api(deleteResource, selectedId);
    if (res?.success) {
      const filteredData = data.filter(
        (resource) => resource.id !== selectedId
      );
      setData(filteredData);
      successAlert("Successfully deleted!");
    }
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (archiveStatus) {
      const res = await api(unArchiveResource, selectedId);
      if (res.success) {
        successAlert(res.message);
        fetchData();
      }
    } else {
      const res = await api(archiveResource, selectedId);
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
    if (location.pathname === "/resources") {
      fetchData();
    }
  }, [perPage, pageNumber, location.pathname]);

  useEffect(() => {
    if (location.pathname === "/resources") {
      setModal((prev) => ({ ...prev, create: false, detail: false }));
    }
  }, [location.pathname]);

  return (
    <div className={styles["resources-container"]}>
      <div className={styles["resources-container-title"]}>
        <h1>Resource Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/resources/create">Create</Link>
        </div>
      </div>
      <div className={styles["resources-container-search"]}>
        <Input
          label="Resource Name"
          placeholder="name"
          name="name"
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
        isClient={false}
        page="resources"
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
      <CreateResource
        isOpen={modal.create}
        setModal={(isOpen) => setModal((prev) => ({ ...prev, create: isOpen }))}
      />
      <ResourceDetail
        isOpen={modal.detail}
        setModal={(isOpen) => setModal((prev) => ({ ...prev, detail: isOpen }))}
      />
    </div>
  );
};

export default ResourcePage;
