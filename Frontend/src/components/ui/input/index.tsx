import React from "react";
import styles from "./style.module.scss";
interface IProps {
  label?: string;
  id?: string;
  placeholder?: string;
  rest?: any[];
}

const Input: React.FC<IProps & Record<string, any>> = ({
  label,
  id,
  placeholder,
  ...rest
}) => {
  return (
    <div id={styles["input"]}>
      {label ? <label htmlFor={id}>{label}</label> : null}
      <input {...rest} placeholder={placeholder} />
    </div>
  );
};

export default Input;
