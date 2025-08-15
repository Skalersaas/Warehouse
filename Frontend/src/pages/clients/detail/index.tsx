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
  archiveClient,
  getClientById,
  unArchiveClient,
  updateClient,
} from "../../../services";
import type { IClient } from "../../../types/common.type";

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

const ClientDetail = () => {
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const dispatch = useAppDispatch();
  const { id } = useParams();

  const apiCall = async (data: IClient) => {
    const res = await api(updateClient, data);
    if (res.data) {
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
      await api(unArchiveClient, id);
      fetchClient();
    } else {
      await api(archiveClient, id);
      fetchClient();
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchClient();
  }, []);

  return (
    <div className={styles["detail-client-container"]}>
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
          <Button type="button" onClick={handleArchive} style={{backgroundColor: "#d19b13"}}>
            {formData.isArchived ? "Unarchive" : "Archive"} Client
          </Button>
        </div>
      </form>
    </div>
  );
};

export default ClientDetail;
