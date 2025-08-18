import { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import useApi from "../../hooks/useApi";
import { getBalance, getResource, getUnit } from "../../services";
import { setLoading } from "../../store/features/app/appSlice";
import type { IBalance, IResource, IUnit } from "../../types/common.type";
import styles from "./style.module.scss";
import BalanceTable from "../../components/ui/balanceTable";
import Pagination from "../../components/ui/pagination";
import Select from "../../components/ui/select";
import Button from "../../components/ui/button";

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

  const [otherData, setOtherData] = useState<{
    resourceData: IResource[];
    unitData: IUnit[];
  }>({
    resourceData: [],
    unitData: [],
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
  });

  const [modal, setModal] = useState<{
    resourceModal: boolean;
    unitModal: boolean;
  }>({
    resourceModal: false,
    unitModal: false,
  });

  const fetchData = async () => {
    dispatch(setLoading(true));
    const { data, count } = await api(getBalance, {
      size: perPage,
      page: pageNumber,
      filters: {
        resourceId:
          (value?.resourceValue?.id && String(value?.resourceValue?.id)) || "",
        unitId: (value?.unitValue?.id && String(value?.unitValue?.id)) || "",
      },
    });
    setTotalRows(count ?? 0);
    setData(data ?? []);
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

    setOtherData((prev) => ({
      ...prev,
      resourceData: resourceData?.data ?? [],
      unitData: unitData?.data ?? [],
    }));
    dispatch(setLoading(false));
  };

  const handleModal = (key: "resourceModal" | "unitModal", isOpen: boolean) => {
    setModal({
      resourceModal: false,
      unitModal: false,
      [key]: isOpen,
    });
  };

  useEffect(() => {
    fetchData();
    fetchOtherDatas();
  }, [perPage, pageNumber]);

  return (
    <div className={styles["balances-container"]}>
      <div className={styles["container-title"]}>
        <h1>Warehouse Items</h1>
      </div>

      <div className={styles["balances-container-search"]}>
        <div className={styles["balances-container-search-wrapper"]}>
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

        <Button onClick={fetchData}>Search</Button>
      </div>

      <BalanceTable data={data || []} />
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
