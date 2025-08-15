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
  archiveUnit,
  getUnitById,
  unArchiveUnit,
  updateUnit,
} from "../../../services";
import type { IUnit } from "../../../types/common.type";

interface initialStateType {
  name: string;
  isArchived: boolean;
}
const initialState = {
  name: "",
  isArchived: false,
};

const UnitDetail = () => {
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const dispatch = useAppDispatch();
  const { id } = useParams();

  const apiCall = async (data: IUnit) => {
    const res = await api(updateUnit, data);
    if (res.data) {
      fetchResource();
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

  const fetchResource = async () => {
    dispatch(setLoading(true));
    const res = await api(getUnitById, id);
    setFormData({
      name: res.data.name || "",
      isArchived: res.data.isArchived,
    });
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (formData.isArchived) {
      await api(unArchiveUnit, id);
      fetchResource();
    } else {
      await api(archiveUnit, id);
      fetchResource();
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    fetchResource();
  }, []);

  return (
    <div className={styles["detail-unit-container"]}>
      <h1>Detail Unit</h1>
      <form onSubmit={handleSubmit} className={styles["detail-unit-form"]}>
        <Input
          label="Unit Name"
          placeholder="name"
          value={formData?.name}
          name="name"
          onChange={handleChange}
        />

        <div className={styles["detail-unit-form-buttons"]}>
          <Button type="Submit">Update Unit</Button>
          <Button
            type="button"
            onClick={handleArchive}
            style={{ backgroundColor: "#d19b13" }}
          >
            {formData.isArchived ? "Unarchive" : "Archive"} Unit
          </Button>
        </div>
      </form>
    </div>
  );
};

export default UnitDetail;
