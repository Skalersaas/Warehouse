import { useEffect, useState } from "react";
import styles from "./style.module.scss";
import {
  deleteReceipt,
  getReceipt,
  getResource,
  getUnit,
} from "../../services";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { setLoading } from "../../store/features/app/appSlice";
import type {
  IReceiptDocument,
  IResource,
  IUnit,
} from "../../types/common.type";
import { Link } from "react-router-dom";
import Alert from "../../components/ui/alert";
import Pagination from "../../components/ui/pagination";
import DocumentTable from "../../components/ui/documentTable";
import { successAlert } from "../../utils/toaster";
import Select from "../../components/ui/select";
import CustomCalendar from "../../components/ui/calendar";
import Button from "../../components/ui/button";

const ReceiptPage = () => {
  const api = useApi();
  const dispatch = useDispatch();
  const [data, setData] = useState<IReceiptDocument[]>([]);
  const [alertIsOpen, setAlertIsOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number>();
  const [totalRows, setTotalRows] = useState<number>(0);
  const [perPage, setPerPage] = useState<number>(10);
  const [pageNumber, setPageNumber] = useState<number>(1);

  const [date, setDate] = useState<{
    startDate: Date | null;
    endDate: Date | null;
  }>({
    startDate: null,
    endDate: null,
  });
  const [formattedDate, setFormattedDate] = useState<{
    startDate: string;
    endDate: string;
  }>({
    startDate: "Select Date",
    endDate: "Select Date",
  });
  const [activeCalendar, setActiveCalendar] = useState<"start" | "end" | null>(
    null
  );

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

  const [otherData, setOtherData] = useState<{
    resourceData: IResource[];
    unitData: IUnit[];
    numberData: {
      id: number;
      name: string;
      isArchived: boolean;
    }[];
  }>({
    resourceData: [],
    unitData: [],
    numberData: [],
  });

  const [value, setValue] = useState<{
    resourceValue: {
      id: string;
      name: string;
      isArchived: boolean | null;
    };
    unitValue: {
      id: string;
      name: string;
      isArchived: boolean | null;
    };
    numberValue: {
      id: string;
      name: string;
      isArchived: boolean | null;
    };
  }>({
    resourceValue: {
      id: "",
      name: "",
      isArchived: false,
    },
    unitValue: {
      id: "",
      name: "",
      isArchived: false,
    },
    numberValue: {
      id: "",
      name: "",
      isArchived: false,
    },
  });

  const [modal, setModal] = useState<{
    resourceModal: boolean;
    unitModal: boolean;
    numberModal: boolean;
  }>({
    resourceModal: false,
    unitModal: false,
    numberModal: false,
  });

  useEffect(() => {
    setFormattedDate({
      startDate: date.startDate
        ? date.startDate.toLocaleDateString("en-GB", {
            day: "2-digit",
            month: "long",
            year: "numeric",
          })
        : "Select Date",
      endDate: date.endDate
        ? date.endDate.toLocaleDateString("en-GB", {
            day: "2-digit",
            month: "long",
            year: "numeric",
          })
        : "Select Date",
    });
  }, [date.startDate, date.endDate]);

  const fetchData = async () => {
    dispatch(setLoading(true));
    const { data, count } = await api(getReceipt, {
      size: perPage,
      page: pageNumber,
      filters: {
        "date.from": date.startDate || "",
        "date.to": date.endDate || "",
        number: value?.numberValue?.name || "",
        "items.resourceId":
          (value?.resourceValue?.id && String(value?.resourceValue?.id)) || "",
        "items.unitId":
          (value?.unitValue?.id && String(value?.unitValue?.id)) || "",
      },
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
    dispatch(setLoading(false));
  };
  const handleDelete = async () => {
    dispatch(setLoading(true));
    const res = await api(deleteReceipt, selectedId);
    if (res?.success) {
      const filteredData = data.filter((receipt) => receipt.id !== selectedId);
      setData(filteredData);
      successAlert("Successfully deleted!");
    }
    dispatch(setLoading(false));
  };

  const fetchOtherDatas = async () => {
    dispatch(setLoading(true));
    const resourceData = await api(getResource, {
      filters: {
        isArchived: "false",
      },
    });
    const unitData = await api(getUnit, {
      filters: {
        isArchived: "false",
      },
    });

    const numberResponse = await api(getReceipt, {});
    const numberData = numberResponse?.data.map((doc: IReceiptDocument) => ({
      id: doc.id,
      name: doc.number,
      isArchived: false,
    }));

    setOtherData((prev) => ({
      ...prev,
      resourceData: resourceData?.data ?? [],
      unitData: unitData?.data ?? [],
      numberData: numberData ?? [],
    }));
    dispatch(setLoading(false));
  };

  const handleModal = (
    key: "resourceModal" | "unitModal" | "numberModal",
    isOpen: boolean
  ) => {
    setModal({
      resourceModal: false,
      unitModal: false,
      numberModal: false,
      [key]: isOpen,
    });
  };

  useEffect(() => {
    fetchData();
    fetchOtherDatas();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["receipts-container"]}>
      <div className={styles["container-title"]}>
        <h1>Receipts Page</h1>
        <div className={styles["create-button"]}>
          <Link to="/receipts/create">Create</Link>
        </div>
      </div>

      <div className={styles["receipts-container-search"]}>
        <div className={styles["receipts-container-search-wrapper"]}>
          <div className={styles["receipts-container-date-time"]}>
            <div className={styles["receipts-container-calendar-wrapper"]}>
              <label
                className={styles["receipts-container-calendar-wrapper-label"]}
              >
                Choose Period
              </label>
              <button
                onClick={() =>
                  setActiveCalendar(activeCalendar === "start" ? null : "start")
                }
                className={styles["receipts-container-calendar-button"]}
              >
                {formattedDate.startDate}
              </button>

              {activeCalendar === "start" && (
                <div className={styles["receipts-container-calendar-popup"]}>
                  <CustomCalendar
                    selectedDate={date.startDate}
                    onSelectDate={(val) =>
                      setDate((prev) => ({ ...prev, startDate: val }))
                    }
                    onClose={() => setActiveCalendar(null)}
                  />
                </div>
              )}
            </div>

            <div className={styles["receipts-container-calendar-wrapper"]}>
              <button
                onClick={() =>
                  setActiveCalendar(activeCalendar === "end" ? null : "end")
                }
                className={styles["receipts-container-calendar-button"]}
              >
                {formattedDate.endDate}
              </button>

              {activeCalendar === "end" && (
                <div className={styles["receipts-container-calendar-popup"]}>
                  <CustomCalendar
                    selectedDate={date.endDate}
                    onSelectDate={(val) =>
                      setDate((prev) => ({ ...prev, endDate: val }))
                    }
                    onClose={() => setActiveCalendar(null)}
                  />
                </div>
              )}
            </div>
          </div>
          <div className={styles["receipts-container-search-select-box"]}>
            <Select
              label="Number"
              data={otherData?.numberData}
              value={value?.numberValue}
              setValue={(val) =>
                setValue((prev) => {
                  const currentIds = prev.numberValue?.id
                    ? prev.numberValue.id
                        .toString()
                        .split(",")
                        .map((c) => c.trim())
                    : [];

                  const currentNames = prev.numberValue?.name
                    ? prev.numberValue.name.split(",").map((c) => c.trim())
                    : [];
                  const exists = currentIds.includes(val.id.toString());
                  let newIds: string[];
                  let newNames: string[];
                  if (exists) {
                    newIds = currentIds.filter((c) => c !== val.id.toString());
                    newNames = currentNames.filter((c) => c !== val.name);
                  } else {
                    newIds = [...currentIds, val.id.toString()];
                    newNames = [...currentNames, val.name];
                  }
                  return {
                    ...prev,
                    numberValue: {
                      ...val,
                      id: newIds.join(", "),
                      name: newNames.join(", "),
                    },
                  };
                })
              }
              setModal={(isOpen) => handleModal("numberModal", isOpen)}
              isOpen={modal.numberModal}
            />

            <Select
              label="Resource"
              data={otherData?.resourceData}
              value={value?.resourceValue}
              setValue={(val) =>
                setValue((prev) => {
                  const currentIds = prev.resourceValue?.id
                    ? prev.resourceValue.id
                        .toString()
                        .split(",")
                        .map((c) => c.trim())
                    : [];

                  const currentNames = prev.resourceValue?.name
                    ? prev.resourceValue.name.split(",").map((c) => c.trim())
                    : [];
                  const exists = currentIds.includes(val.id.toString());
                  let newIds: string[];
                  let newNames: string[];
                  if (exists) {
                    newIds = currentIds.filter((c) => c !== val.id.toString());
                    newNames = currentNames.filter((c) => c !== val.name);
                  } else {
                    newIds = [...currentIds, val.id.toString()];
                    newNames = [...currentNames, val.name];
                  }
                  return {
                    ...prev,
                    resourceValue: {
                      ...val,
                      id: newIds.join(", "),
                      name: newNames.join(", "),
                    },
                  };
                })
              }
              setModal={(isOpen) => handleModal("resourceModal", isOpen)}
              isOpen={modal.resourceModal}
            />

            <Select
              label="Unit"
              data={otherData?.unitData}
              value={value?.unitValue}
              setValue={(val) =>
                setValue((prev) => {
                  const currentIds = prev.unitValue?.id
                    ? prev.unitValue.id
                        .toString()
                        .split(",")
                        .map((c) => c.trim())
                    : [];

                  const currentNames = prev.unitValue?.name
                    ? prev.unitValue.name.split(",").map((c) => c.trim())
                    : [];
                  const exists = currentIds.includes(val.id.toString());
                  let newIds: string[];
                  let newNames: string[];
                  if (exists) {
                    newIds = currentIds.filter((c) => c !== val.id.toString());
                    newNames = currentNames.filter((c) => c !== val.name);
                  } else {
                    newIds = [...currentIds, val.id.toString()];
                    newNames = [...currentNames, val.name];
                  }
                  return {
                    ...prev,
                    unitValue: {
                      ...val,
                      id: newIds.join(", "),
                      name: newNames.join(", "),
                    },
                  };
                })
              }
              setModal={(isOpen) => handleModal("unitModal", isOpen)}
              isOpen={modal.unitModal}
            />
          </div>
        </div>

        <Button onClick={fetchData}>Search</Button>
      </div>

      <DocumentTable
        data={data}
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
