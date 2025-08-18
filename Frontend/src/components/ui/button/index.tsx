import React, { type ReactNode } from "react";
import styles from "./style.module.scss";

interface IProps {
  children?: ReactNode;
  rest?: any;
}

const Button: React.FC<IProps & Record<string, any>> = ({ children, ...rest }) => {
  return <button id={styles["button"]} {...rest}> {children}</button>;
};

export default Button;
