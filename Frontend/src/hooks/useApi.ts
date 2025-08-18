import { successAlert } from "../utils/toaster";

export default function useApi() {
  const newMethod = async (
    method: Function,
    data?: object | number | string,
    successMessage?: string
  ) => {
    try {
      const res = await method(data);
      successMessage && successAlert(successMessage);
      return res;
    } catch (err: any) {
      return;
    } finally {
    }
  };

  return newMethod;
}
