import { del, get, post, put, patch } from "../utils/api";

// Receipt
const getReceipt = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`receiptdocument/query`, {
    page,
    size,
    filters,
  });
};

const deleteReceipt = async (id: number) => await del(`receiptdocument/${id}`);
const getReceiptById = async (id: number) => await get(`receiptdocument/${id}`);
const updateReceipt = async (data: object) =>
  await put(`receiptdocument`, data);
const createReceipt = async (data: object) =>
  await post(`receiptdocument`, data);

// Shipment Documents
const getShipment = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`shipmentdocument/query`, {
    page,
    size,
    filters,
  });
};

const deleteShipment = async (id: number) =>
  await del(`shipmentdocument/${id}`);
const revokeShipment = async (id: number) =>
  await patch(`shipmentdocument/${id}/revoke`);
const signShipment = async (id: number) =>
  await patch(`shipmentdocument/${id}/sign`);
const getShipmentById = async (id: number) =>
  await get(`shipmentdocument/${id}`);
const updateShipment = async (data: object) =>
  await put(`shipmentdocument`, data);
const createShipment = async (data: object) =>
  await post(`shipmentdocument`, data);

// Client
const getClient = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`client/query`, {
    page,
    size,
    filters,
  });
};

const deleteClient = async (id: number) => await del(`client/${id}`);
const archiveClient = async (id: number) => await patch(`client/${id}/archive`);
const unArchiveClient = async (id: number) =>
  await patch(`client/${id}/unarchive`);
const getClientById = async (id: number) => await get(`client/${id}`);
const updateClient = async (data: object) => await put(`client`, data);
const createClient = async (data: object) => await post(`client`, data);

// Resource
const getResource = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`resource/query`, {
    page,
    size,
    filters,
  });
};

const deleteResource = async (id: number) => await del(`resource/${id}`);
const archiveResource = async (id: number) =>
  await patch(`resource/${id}/archive`);
const unArchiveResource = async (id: number) =>
  await patch(`resource/${id}/unarchive`);
const getResourceById = async (id: number) => await get(`resource/${id}`);
const updateResource = async (data: object) => await put(`resource`, data);
const createResource = async (data: object) => await post(`resource`, data);

// Unit
const getUnit = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`unit/query`, {
    page,
    size,
    filters,
  });
};

const deleteUnit = async (id: number) => await del(`unit/${id}`);
const archiveUnit = async (id: number) => await patch(`unit/${id}/archive`);
const unArchiveUnit = async (id: number) => await patch(`unit/${id}/unarchive`);
const getUnitById = async (id: number) => await get(`unit/${id}`);
const updateUnit = async (data: object) => await put(`unit`, data);
const createUnit = async (data: object) => await post(`unit`, data);

// Balance
const getBalance = async ({
  page,
  size,
  filters,
}: {
  page: number;
  size: number;
  filters: object;
}) => {
  return await post(`balance/query`, {
    page,
    size,
    filters,
  });
};

export {
  getBalance,
  getShipment,
  revokeShipment,
  signShipment,
  getClient,
  getResource,
  getUnit,
  deleteClient,
  archiveClient,
  unArchiveClient,
  createClient,
  getClientById,
  updateClient,
  deleteResource,
  archiveResource,
  unArchiveResource,
  getResourceById,
  updateResource,
  createResource,
  deleteUnit,
  archiveUnit,
  unArchiveUnit,
  getUnitById,
  updateUnit,
  createUnit,
  getReceipt,
  deleteReceipt,
  getReceiptById,
  updateReceipt,
  createReceipt,
  deleteShipment,
  getShipmentById,
  updateShipment,
  createShipment,
};
