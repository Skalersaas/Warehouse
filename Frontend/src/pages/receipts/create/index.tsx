import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import useApi from "../../../hooks/useApi";
import { useAppDispatch } from "../../../store/hooks";
import { setLoading } from "../../../store/features/app/appSlice";
import { errorAlert, successAlert } from "../../../utils/toaster";
import { createReceipt, getResource, getUnit } from "../../../services";
import Input from "../../../components/ui/input";
import Button from "../../../components/ui/button";

import styles from "./styles.module.scss";
import CustomCalendar from "../../../components/ui/calendar";
import { toLocalISOString } from "../../../utils/dateFormatter";
import Select from "../../../components/ui/select";
import type { IResource, IUnit } from "../../../types/common.type";
import { Plus, Trash } from "lucide-react";

interface initialStateType {
  number: string;
  date: string;
  items: {
    resourceId: number;
    resourceName?: string;
    unitId: number;
    unitName?: string;
    quantity: number;
  }[];
}
const initialState = {
  number: "",
  date: "",
  items: [],
};

const CreateReceipt = () => {
  const navigate = useNavigate();
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const [formattedDate, setFormattedDate] = useState("Select Date");
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [activeCalendar, setActiveCalendar] = useState<boolean>(false);
  const dispatch = useAppDispatch();

  const [data, setData] = useState<{
    resourceData: IResource[];
    unitData: IUnit[];
  }>({
    resourceData: [],
    unitData: [],
  });

  const [value, setValue] = useState<{
    resourceValue: {
      id: number;
      name: string;
      isArchived: boolean | null;
    };
    unitValue: {
      id: number;
      name: string;
      isArchived: boolean | null;
    };
  }>({
    resourceValue: {
      id: 0,
      name: "",
      isArchived: false,
    },
    unitValue: {
      id: 0,
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

  const [quantity, setQuantity] = useState<number>(0);

  useEffect(() => {
    if (selectedDate) {
      setFormattedDate(
        selectedDate.toLocaleDateString("en-GB", {
          day: "2-digit",
          month: "long",
          year: "numeric",
        })
      );
    } else {
      setFormattedDate("Select Date");
    }
  }, [selectedDate]);

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    dispatch(setLoading(true));

    const res = await api(createReceipt, {
      ...formData,
      date: toLocalISOString(selectedDate),
    });
    if (res?.data) {
      successAlert(`Successfully created`);
      navigate("/receipts");
    }

    dispatch(setLoading(false));
  };
  const fetchResource = async () => {
    dispatch(setLoading(true));
    const response = await api(getResource, {
      filters: {
        isArchived: "false",
      },
    });
    setData((prev) => ({
      ...prev,
      resourceData: response?.data ?? [],
    }));
    dispatch(setLoading(false));
  };
  const fetchUnit = async () => {
    dispatch(setLoading(true));
    const response = await api(getUnit, {
      filters: {
        isArchived: "false",
      },
    });
    setData((prev) => ({
      ...prev,
      unitData: response?.data ?? [],
    }));
    dispatch(setLoading(false));
  };
  const handleChange = (e: any) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleAddItem = () => {
    if (value.resourceValue.id > 0 && value.unitValue.id > 0 && quantity > 0) {
      setFormData((prev) => {
        const exists = prev.items.some(
          (item) =>
            item.resourceId === value.resourceValue.id &&
            item.unitId === value.unitValue.id
        );

        if (exists) {
          errorAlert("This resource with the same unit already exists.");
          return prev;
        }

        return {
          ...prev,
          items: [
            ...prev.items,
            {
              resourceId: value.resourceValue.id,
              resourceName: value.resourceValue.name,
              unitId: value.unitValue.id,
              unitName: value.unitValue.name,
              quantity: quantity,
            },
          ],
        };
      });

      setValue({
        resourceValue: { id: 0, name: "", isArchived: false },
        unitValue: { id: 0, name: "", isArchived: false },
      });
      setQuantity(0);
    } else {
      errorAlert("Please fill resource, unit, and quantity before adding.");
    }
  };

  const handleRemoveItem = (resourceId: number, unitId: number) => {
    setFormData((prev) => ({
      ...prev,
      items: prev.items.filter(
        (item) => !(item.resourceId === resourceId && item.unitId === unitId)
      ),
    }));
  };

  const handleModal = (key: "resourceModal" | "unitModal", isOpen: boolean) => {
    setModal({
      resourceModal: false,
      unitModal: false,
      [key]: isOpen,
    });
  };

  useEffect(() => {
    fetchResource();
    fetchUnit();
  }, []);

  return (
    <div className={styles["create-receipt-container"]}>
      <h1>Create Receipt</h1>
      <form onSubmit={handleSubmit} className={styles["create-receipt-form"]}>
        <Input
          label="Receipt Number"
          placeholder="number"
          value={formData?.number}
          name="number"
          onChange={handleChange}
        />

        <div className={styles["create-receipt-calendar-wrapper"]}>
          <label className={styles["create-receipt-calendar-wrapper-label"]}>
            Select Date
          </label>
          <div
            onClick={() => setActiveCalendar((prev) => !prev)}
            className={styles["create-receipt-calendar-button"]}
          >
            {formattedDate}
          </div>

          {activeCalendar && (
            <div className={styles["create-receipt-calendar-popup"]}>
              <CustomCalendar
                selectedDate={selectedDate}
                onSelectDate={setSelectedDate}
                onClose={() => setActiveCalendar(false)}
              />
            </div>
          )}
        </div>

        <div className={styles["create-receipt-multiple-wrapper"]}>
          <Select
            label="Resource"
            data={data?.resourceData}
            value={value?.resourceValue}
            setValue={(val) =>
              setValue((prev) => ({ ...prev, resourceValue: val }))
            }
            setModal={(isOpen) => handleModal("resourceModal", isOpen)}
            isOpen={modal.resourceModal}
          />

          <Select
            label="Unit"
            data={data?.unitData}
            value={value?.unitValue}
            setValue={(val) =>
              setValue((prev) => ({ ...prev, unitValue: val }))
            }
            setModal={(isOpen) => handleModal("unitModal", isOpen)}
            isOpen={modal.unitModal}
          />
          <div className={styles["create-receipt-multiple-wrapper-quantity"]}>
            <Input
              label="Quantity"
              placeholder="quantity"
              value={quantity}
              name="quantity"
              onChange={(e: {
                target: { value: React.SetStateAction<number> };
              }) => setQuantity(Number(e.target.value))}
            />
            <div
              className={styles["create-receipt-multiple-wrapper-button"]}
              onClick={handleAddItem}
            >
              <Plus width={14} height={14} />
            </div>
          </div>
        </div>

        {formData.items.length > 0 && (
          <div className={styles["create-receipt-form-items"]}>
            <div className={styles["create-receipt-form-items-head"]}>
              <div className={styles["create-receipt-form-items-head-row"]}>
                Resource
              </div>
              <div className={styles["create-receipt-form-items-head-row"]}>
                Unit
              </div>
              <div className={styles["create-receipt-form-items-head-row"]}>
                Quantity
              </div>
              <div>Action</div>
            </div>
            {formData.items.map((item) => (
              <div
                key={`${item.resourceId}-${item.unitId}`}
                className={styles["create-receipt-form-items-per"]}
              >
                <div className={styles["create-receipt-form-items-per-row"]}>
                  {item.resourceName}
                </div>
                <div className={styles["create-receipt-form-items-per-row"]}>
                  {item.unitName}
                </div>
                <div className={styles["create-receipt-form-items-per-row"]}>
                  {item.quantity}
                </div>
                <button
                  className={styles["create-receipt-form-items-per-delete"]}
                  onClick={() => handleRemoveItem(item.resourceId, item.unitId)}
                >
                  <Trash width={14} />
                </button>
              </div>
            ))}
          </div>
        )}

        <Button type="Submit">Create Receipt</Button>
      </form>
    </div>
  );
};

export default CreateReceipt;
