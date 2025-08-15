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
  getResource,
  getShipmentById,
  getUnit,
  revokeShipment,
  signShipment,
  updateShipment,
} from "../../../services";
import { Plus } from "lucide-react";
import CustomCalendar from "../../../components/ui/calendar";
import Select from "../../../components/ui/select";
import type { ICommonType, IShipmentDocument } from "../../../types/common.type";
import { toLocalISOString } from "../../../utils/dateFormatter";

interface initialStateType {
  number: string;
  clientId: number;
  date: string;
  status: number;
  items: {
    id?: number;
    resourceId: number;
    unitId: number;
    quantity: number;
  }[];
}
const initialState = {
  number: "",
  clientId: 0,
  date: "",
  status: 0,
  items: [],
};

const ShipmentDetail = () => {
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

  const apiCall = async (data: IShipmentDocument) => {
    const res = await api(updateShipment, data);
    if (res.data) {
      fetchShipment();
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

  const fetchShipment = async () => {
    dispatch(setLoading(true));
    const res = await api(getShipmentById, id);

    setFormData({
      number: res.data.number || "",
      clientId: res.data.clientId || 0,
      date: res.data.date || "",
      status: res.data.status || 0,
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

  const handleSignRevoke = async () => {
    dispatch(setLoading(true));
    if (formData.status) {
      await api(revokeShipment, id);
      fetchShipment();
    } else {
      await api(signShipment, id);
      fetchShipment();
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchShipment();
    fetchResource();
    fetchUnit();
  }, []);

  return (
    <div className={styles["detail-shipment-container"]}>
      <h1>Detail Shipment</h1>
      <form onSubmit={handleSubmit} className={styles["detail-shipment-form"]}>
        <Input
          label="Shipment Number"
          placeholder="number"
          value={formData?.number}
          name="number"
          onChange={handleChange}
        />

        <div className={styles["detail-shipment-calendar-wrapper"]}>
          <label className={styles["detail-shipment-calendar-wrapper-label"]}>
            Select Date
          </label>
          <div
            onClick={() => setActiveCalendar((prev) => !prev)}
            className={styles["detail-shipment-calendar-button"]}
          >
            {formattedDate}
          </div>

          {activeCalendar && (
            <div className={styles["detail-shipment-calendar-popup"]}>
              <CustomCalendar
                selectedDate={selectedDate}
                onSelectDate={setSelectedDate}
                onClose={() => setActiveCalendar(false)}
              />
            </div>
          )}
        </div>

        <div className={styles["detail-shipment-multiple-wrapper"]}>
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
            className={styles["detail-shipment-multiple-wrapper-button"]}
            onClick={handleAddItem}
          >
            <Plus width={14} height={14} />
          </div>
        </div>

        <div className={styles["detail-shipment-form-buttons"]}>
          <Button type="Submit">Update Shipment</Button>
          <Button
            type="button"
            onClick={handleSignRevoke}
            style={{ backgroundColor: "#d19b13" }}
          >
            {formData.status === 1 ? "Revoke" : "Sign"} Shipment
          </Button>
        </div>
      </form>
    </div>
  );
};

export default ShipmentDetail;
