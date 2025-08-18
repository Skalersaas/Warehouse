import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import useApi from "../../../hooks/useApi";
import { useAppDispatch } from "../../../store/hooks";
import { setLoading } from "../../../store/features/app/appSlice";
import { successAlert } from "../../../utils/toaster";
import { createResource } from "../../../services";
import Input from "../../../components/ui/input";
import Button from "../../../components/ui/button";
import styles from "./styles.module.scss";
import { X } from "lucide-react";

interface IProps {
  isOpen: boolean;
  setModal: (isOpen: boolean) => void;
}

interface initialStateType {
  name: string;
}
const initialState = {
  name: "",
};

const CreateResource = ({ isOpen, setModal }: IProps) => {
  const navigate = useNavigate();
  const api = useApi();
  const [formData, setFormData] = useState<initialStateType>(initialState);
  const dispatch = useAppDispatch();
  const location = useLocation();

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    dispatch(setLoading(true));

    const res = await api(createResource, formData);
    if (res?.data) {
      setFormData({ ...initialState });
      successAlert(`Successfully created`);
      navigate("/resources");
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

  useEffect(() => {
    if (location.pathname === "/resources/create") {
      setModal(true);
    }
  }, [location.pathname]);

  return (
    <div
      className={`${styles["create-resource-container"]} ${
        isOpen && styles["active"]
      }`}
    >
      <div className={styles["create-resource-container-box"]}>
        <div
          className={styles["create-resource-container-close"]}
          onClick={() => {
            navigate("/resources");
          }}
        >
          <X width={26} />
        </div>
        <h1>Create Resource</h1>
        <form
          onSubmit={handleSubmit}
          className={styles["create-resource-form"]}
        >
          <Input
            label="Resource Name"
            placeholder="name"
            value={formData?.name}
            name="name"
            onChange={handleChange}
          />

          <Button type="Submit">Create Resource</Button>
        </form>
      </div>
    </div>
  );
};

export default CreateResource;
