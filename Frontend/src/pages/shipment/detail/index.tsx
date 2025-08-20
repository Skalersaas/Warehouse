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
  getClient,
  getClientById,
  getResource,
  getShipmentById,
  getUnit,
  revokeShipment,
  signShipment,
  updateShipment,
} from "../../../services";
import { Plus, Trash } from "lucide-react";
import CustomCalendar from "../../../components/ui/calendar";
import Select from "../../../components/ui/select";
import type {
  IClient,
  IResource,
  IShipmentDocument,
  IUnit,
} from "../../../types/common.type";
import { toLocalISOString } from "../../../utils/dateFormatter";

interface initialStateType {
  number: string;
  clientId: number;
  clientName?: string;
  date: string;
  status: number;
  items: {
    id?: number;
    resourceId: number;
    resourceName?: string;
    unitId: number;
    unitName?: string;
    quantity: number;
  }[];
}
const initialState = {
  number: "",
  clientId: 0,
  clientName: "",
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
    resourceData: IResource[];
    clientData: IClient[];
    unitData: IUnit[];
  }>({
    resourceData: [],
    clientData: [],
    unitData: [],
  });

  const [value, setValue] = useState<{
    resourceValue: {
      id: string;
      name: string;
      isArchived: boolean | null;
    };
    clientValue: {
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
    clientValue: {
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
    clientModal: boolean;
    unitModal: boolean;
  }>({
    resourceModal: false,
    clientModal: false,
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
    fetchShipment();
    if (res?.data) {
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
          clientId: Number(value.clientValue.id),
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
    const clientData = await getClientById(res.data.clientId);
    const clientValue = {
      id: String(clientData.data.id),
      name: clientData.data.name,
      isArchived: clientData.data.isArchived,
    };
    setValue((prev) => ({ ...prev, clientValue: clientValue }));
    setFormattedDate(formatDate(res.data.date));
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
      resourceData: response.data ?? [],
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
      unitData: response.data ?? [],
    }));
    dispatch(setLoading(false));
  };
  const fetchClient = async () => {
    dispatch(setLoading(true));
    const response = await api(getClient, {
      filters: {
        isArchived: "false",
      },
    });
    setData((prev) => ({
      ...prev,
      clientData: response.data ?? [],
    }));
    dispatch(setLoading(false));
  };

  const handleAddItem = () => {
    if (
      Number(value.resourceValue.id) > 0 &&
      Number(value.unitValue.id) > 0 &&
      quantity > 0
    ) {
      setFormData((prev) => {
        const exists = prev.items.some(
          (item) =>
            item.resourceId === Number(value.resourceValue.id) &&
            item.unitId === Number(value.unitValue.id)
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
              resourceId: Number(value.resourceValue.id),
              resourceName: value.resourceValue.name,
              unitId: Number(value.unitValue.id),
              unitName: value.unitValue.name,
              quantity: quantity,
            },
          ],
        };
      });

      setValue((prev) => ({
        ...prev,
        resourceValue: { id: "", name: "", isArchived: false },
        unitValue: { id: "", name: "", isArchived: false },
      }));
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

  const handleSignRevoke = async () => {
    dispatch(setLoading(true));
    if (formData.status) {
      const res = await api(revokeShipment, id);
      if (res.success) {
        successAlert(res.message);
        fetchShipment();
      }
    } else {
      const res = await api(signShipment, id);
      if (res.success) {
        successAlert(res.message);
        fetchShipment();
      }
    }
    dispatch(setLoading(false));
  };

  const handleModal = (
    key: "clientModal" | "resourceModal" | "unitModal",
    isOpen: boolean
  ) => {
    setModal({
      clientModal: false,
      resourceModal: false,
      unitModal: false,
      [key]: isOpen,
    });
  };

  useEffect(() => {
    fetchShipment();
    fetchResource();
    fetchUnit();
    fetchClient();
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
        <Select
          label="Client"
          data={data?.clientData}
          value={value?.clientValue}
          setValue={(val) =>
            setValue((prev) => ({ ...prev, clientValue: val }))
          }
          setModal={(isOpen) => handleModal("clientModal", isOpen)}
          isOpen={modal.clientModal}
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

          <div className={styles["detail-shipment-multiple-wrapper-quantity"]}>
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
        </div>

        {formData.items.length > 0 && (
          <div className={styles["detail-shipment-form-items"]}>
            <div className={styles["detail-shipment-form-items-head"]}>
              <div className={styles["detail-shipment-form-items-head-row"]}>
                Resource
              </div>
              <div className={styles["detail-shipment-form-items-head-row"]}>
                Unit
              </div>
              <div className={styles["detail-shipment-form-items-head-row"]}>
                Quantity
              </div>
              <div>Action</div>
            </div>
            {formData.items.map((item) => (
              <div
                key={`${item.resourceId}-${item.unitId}`}
                className={styles["detail-shipment-form-items-per"]}
              >
                <div className={styles["detail-shipment-form-items-per-row"]}>
                  {item.resourceName}
                </div>
                <div className={styles["detail-shipment-form-items-per-row"]}>
                  {item.unitName}
                </div>
                <div className={styles["detail-shipment-form-items-per-row"]}>
                  {item.quantity}
                </div>
                <button
                  className={styles["detail-shipment-form-items-per-delete"]}
                  onClick={() => handleRemoveItem(item.resourceId, item.unitId)}
                >
                  <Trash width={14} />
                </button>
              </div>
            ))}
          </div>
        )}

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
