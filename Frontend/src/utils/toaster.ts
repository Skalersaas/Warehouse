import { toast } from "react-hot-toast";

const successAlert = (message: string) => toast.success(message);
const errorAlert = (message: string) => toast.error(message);

export { successAlert, errorAlert };
