import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export interface appStateType {
  isLoading: boolean;
  menu: boolean;
}

const initialState: appStateType = {
  isLoading: false,
  menu: false,
};

export const appSlice = createSlice({
  name: "app",
  initialState,
  reducers: {
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    setMenu: (state, action: PayloadAction<boolean>) => {
      state.menu = action.payload;
    },
  },
});

export const { setLoading, setMenu } = appSlice.actions;

export default appSlice.reducer;
