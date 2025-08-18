import { RouterProvider } from "react-router-dom";
import router from "./routes/root";
import { Toaster } from "react-hot-toast";

function App() {
  return (
    <>
      <Toaster position={"bottom-right"} reverseOrder={false} />
      <RouterProvider router={router} />
    </>
  );
}

export default App;
