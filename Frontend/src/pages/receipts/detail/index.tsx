import React, { useEffect, useState } from "react";
import Input from "../../../components/ui/input";
import Button from "../../../components/ui/button";
import styles from "./styles.module.scss";
import { useParams } from "react-router-dom";
import useApi from "../../../hooks/useApi";
import { useAppDispatch } from "../../../store/hooks";
import { setLoading } from "../../../store/features/app/appSlice";
import { errorAlert, successAlert } from "../../../utils/toaster";
import {
  getReceiptById,
  getResource,
  getUnit,
  updateReceipt,
} from "../../../services";
import { Plus } from "lucide-react";
import CustomCalendar from "../../../components/ui/calendar";
import Select from "../../../components/ui/select";
import type { ICommonType, IReceiptDocument } from "../../../types/common.type";
import { toLocalISOString } from "../../../utils/dateFormatter";

interface initialStateType {
  number: string;
  date: string;
  items: {
    id?: number;
    resourceId: number;
    unitId: number;
    quantity: number;
  }[];
}
const initialState = {
  number: "",
  date: "",
  items: [],
};

const ReceiptDetail = () => {
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const [formattedDate, setFormattedDate] = useState("Select Date");
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [activeCalendar, setActiveCalendar] = useState<boolean>(false);

  const dispatch = useAppDispatch();
  const { id } = useParams();

  const [data, setData] = useState<{
    resourceData: ICommonType[];
    unitData: ICommonType[];
  }>({
    resourceData: [],
    unitData: [],
  });

  const [value, setValue] = useState<{
    resourceValue: {
      id: number;
      name: string;
    };
    unitValue: {
      id: number;
      name: string;
    };
  }>({
    resourceValue: {
      id: 0,
      name: "",
    },
    unitValue: {
      id: 0,
      name: "",
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

  const apiCall = async (data: IReceiptDocument) => {
    const res = await api(updateReceipt, data);
    if (res.data) {
      fetchReceipt();
      setFormData({ ...initialState });
      successAlert(`Successfully updated`);
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    dispatch(setLoading(true));

    try {
      setFormData((prev) => {
        const updatedFormData = {
          id: Number(id),
          ...prev,
          date: selectedDate ? toLocalISOString(selectedDate) : prev.date,
        };
        apiCall(updatedFormData);
        return updatedFormData;
      });
    } catch (error: any) {
      errorAlert(error.message);
    }
    dispatch(setLoading(false));
  };

  const handleChange = (e: any) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const fetchReceipt = async () => {
    dispatch(setLoading(true));
    const res = await api(getReceiptById, id);

    setFormData({
      number: res.data.number || "",
      date: res.data.date || "",
      items: res.data.items || [],
    });
    const formatDate = (dateString: string) => {
      if (!dateString) return "";
      const date = new Date(dateString);
      return date.toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "long",
        year: "numeric",
      });
    };
    setFormattedDate(formatDate(res.data.date));
    dispatch(setLoading(false));
  };
  const fetchResource = async () => {
    dispatch(setLoading(true));
    const response = await api(getResource, {});
    setData((prev) => ({
      ...prev,
      resourceData: response.data ?? [],
    }));
    dispatch(setLoading(false));
  };
  const fetchUnit = async () => {
    dispatch(setLoading(true));
    const response = await api(getUnit, {});
    setData((prev) => ({
      ...prev,
      unitData: response.data ?? [],
    }));
    dispatch(setLoading(false));
  };
  useEffect(() => {
    fetchReceipt();
    fetchResource();
    fetchUnit();
  }, []);

  const handleAddItem = () => {
    if (value.resourceValue.id > 0 && value.unitValue.id > 0 && quantity > 0) {
      setFormData((prev) => ({
        ...prev,
        items: [
          ...prev.items,
          {
            resourceId: value.resourceValue.id,
            unitId: value.unitValue.id,
            quantity: quantity,
          },
        ],
      }));

      setValue({
        resourceValue: { id: 0, name: "" },
        unitValue: { id: 0, name: "" },
      });
      setQuantity(0);
    } else {
      errorAlert("Please fill resource, unit, and quantity before adding.");
    }
  };

  return (
    <div className={styles["detail-receipt-container"]}>
      <h1>Detail Receipt</h1>
      <form onSubmit={handleSubmit} className={styles["detail-receipt-form"]}>
        <Input
          label="Receipt Number"
          placeholder="number"
          value={formData?.number}
          name="number"
          onChange={handleChange}
        />

        <div className={styles["detail-receipt-calendar-wrapper"]}>
          <label className={styles["detail-receipt-calendar-wrapper-label"]}>
            Select Date
          </label>
          <div
            onClick={() => setActiveCalendar((prev) => !prev)}
            className={styles["detail-receipt-calendar-button"]}
          >
            {formattedDate}
          </div>

          {activeCalendar && (
            <div className={styles["detail-receipt-calendar-popup"]}>
              <CustomCalendar
                selectedDate={selectedDate}
                onSelectDate={setSelectedDate}
                onClose={() => setActiveCalendar(false)}
              />
            </div>
          )}
        </div>

        <div className={styles["detail-receipt-multiple-wrapper"]}>
          <Select
            label="Resource"
            data={data.resourceData}
            value={value.resourceValue}
            setValue={(val) =>
              setValue((prev) => ({ ...prev, resourceValue: val }))
            }
            setModal={(isOpen) =>
              setModal((prev) => ({ ...prev, resourceModal: isOpen }))
            }
            isOpen={modal.resourceModal}
          />

          <Select
            label="Unit"
            data={data.unitData}
            value={value.unitValue}
            setValue={(val) =>
              setValue((prev) => ({ ...prev, unitValue: val }))
            }
            setModal={(isOpen) =>
              setModal((prev) => ({ ...prev, unitModal: isOpen }))
            }
            isOpen={modal.unitModal}
          />

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
            className={styles["detail-receipt-multiple-wrapper-button"]}
            onClick={handleAddItem}
          >
            <Plus width={14} height={14} />
          </div>
        </div>

        <Button type="Submit">Create Receipt</Button>
      </form>
    </div>
  );
};

export default ReceiptDetail;
