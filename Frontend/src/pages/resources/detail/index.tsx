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
  archiveResource,
  getResourceById,
  unArchiveResource,
  updateResource,
} from "../../../services";
import type { IResource } from "../../../types/common.type";
import { X } from "lucide-react";

interface IProps {
  isOpen: boolean;
  setModal: (isOpen: boolean) => void;
}

interface initialStateType {
  name: string;
  isArchived: boolean;
}
const initialState = {
  name: "",
  isArchived: false,
};

const ResourceDetail = ({ isOpen, setModal }: IProps) => {
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = useParams();

  const apiCall = async (data: IResource) => {
    const res = await api(updateResource, data);
    if (res?.data) {
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
    const res = await api(getResourceById, id);

    setFormData({
      name: res.data.name || "",
      isArchived: res.data.isArchived,
    });
    dispatch(setLoading(false));
  };

  const handleArchive = async () => {
    dispatch(setLoading(true));
    if (formData.isArchived) {
      const res = await api(unArchiveResource, id);
      if (res.success) {
        setFormData((prev) => ({
          ...prev,
          isArchived: false,
        }));
        successAlert(res.message);
      }
    } else {
      const res = await api(archiveResource, id);
      if (res.success) {
        setFormData((prev) => ({
          ...prev,
          isArchived: true,
        }));
        successAlert(res.message);
      }
    }
    dispatch(setLoading(false));
  };

  useEffect(() => {
    if (location.pathname === `/resources/${id}`) {
      fetchResource();
    }
  }, [location.pathname]);

  useEffect(() => {
    if (location.pathname === `/resources/${id}`) {
      setModal(true);
    }
  }, [location.pathname]);

  return (
    <div
      className={`${styles["detail-resource-container"]} ${
        isOpen && styles["active"]
      }`}
    >
      <div className={styles["detail-resource-container-box"]}>
        <div
          className={styles["detail-resource-container-close"]}
          onClick={() => {
            navigate("/resources");
          }}
        >
          <X width={26} />
        </div>
        <h1>Detail Resource</h1>
        <form
          onSubmit={handleSubmit}
          className={styles["detail-resource-form"]}
        >
          <Input
            label="Resource Name"
            placeholder="name"
            value={formData?.name}
            name="name"
            onChange={handleChange}
          />

          <div className={styles["detail-resource-form-buttons"]}>
            <Button type="Submit">Update Resource</Button>
            <Button
              type="button"
              onClick={handleArchive}
              style={{ backgroundColor: "#d19b13" }}
            >
              {formData.isArchived ? "Unarchive" : "Archive"} Resource
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ResourceDetail;
