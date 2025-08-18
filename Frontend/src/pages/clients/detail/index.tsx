import React, { useEffect, useState } from "react";
import Input from "../../../components/ui/input";
import Button from "../../../components/ui/button";
import styles from "./styles.module.scss";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import useApi from "../../../hooks/useApi";
import { useAppDispatch } from "../../../store/hooks";
import { setLoading } from "../../../store/features/app/appSlice";
import { errorAlert, successAlert } from "../../../utils/toaster";
import {
  archiveClient,
  getClientById,
  unArchiveClient,
  updateClient,
} from "../../../services";
import type { IClient } from "../../../types/common.type";
import { X } from "lucide-react";

interface IProps {
  isOpen: boolean;
  setModal: (isOpen: boolean) => void;
}

interface initialStateType {
  name: string;
  address: string;
  isArchived: boolean;
}
const initialState = {
  name: "",
  address: "",
  isArchived: false,
};

const ClientDetail = ({ isOpen, setModal }: IProps) => {
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = useParams();

  const apiCall = async (data: IClient) => {
    const res = await api(updateClient, data);
    if (res?.data) {
      fetchClient();
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

  const fetchClient = async () => {
    dispatch(setLoading(true));
    const res = await api(getClientById, id);

    setFormData({
      name: res.data.name || "",
      address: res.data.address || "",
      isArchived: res.data.isArchived,
    });
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (formData.isArchived) {
      const res = await api(unArchiveClient, id);
      if (res.success) {
        successAlert(res.message);
        setFormData((prev) => ({
          ...prev,
          isArchived: false,
        }));
      }
    } else {
      const res = await api(archiveClient, id);
      if (res.success) {
        successAlert(res.message);
        setFormData((prev) => ({
          ...prev,
          isArchived: true,
        }));
      }
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    if (location.pathname === `/clients/${id}`) {
      fetchClient();
    }
  }, [location.pathname]);

  useEffect(() => {
    if (location.pathname === `/clients/${id}`) {
      setModal(true);
    }
  }, [location.pathname]);

  return (
    <div
      className={`${styles["detail-client-container"]} ${
        isOpen && styles["active"]
      }`}
    >
      <div className={styles["detail-client-container-box"]}>
        <div
          className={styles["detail-client-container-close"]}
          onClick={() => {
            navigate("/clients");
          }}
        >
          <X width={26} />
        </div>
        <h1>Detail Client</h1>
        <form onSubmit={handleSubmit} className={styles["detail-client-form"]}>
          <Input
            label="Client Name"
            placeholder="name"
            value={formData?.name}
            name="name"
            onChange={handleChange}
          />

          <Input
            label="Client Address"
            placeholder="address"
            value={formData?.address}
            name="address"
            onChange={handleChange}
          />

          <div className={styles["detail-client-form-buttons"]}>
            <Button type="Submit">Update Client</Button>
            <Button
              type="button"
              onClick={handleArchive}
              style={{ backgroundColor: "#d19b13" }}
            >
              {formData.isArchived ? "Unarchive" : "Archive"} Client
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ClientDetail;
