import { del, get, post, put, patch } from "../utils/api";

// Receipt
const getReceipt = async ({}: //   search,
//   sortField,
//   ascending,
//   page,
//   size,
{
  search: string;
  sortField: string;
  ascending: boolean;
  page: number;
  size: number;
}) => {
  return await post(`receiptdocument/query`, {});
};

const deleteReceipt = async (id: number) => await del(`receiptdocument/${id}`);
const getReceiptById = async (id: number) => await get(`receiptdocument/${id}`);
const updateReceipt = async (data: object) =>
  await put(`receiptdocument`, data);
const createReceipt = async (data: object) =>
  await post(`receiptdocument`, data);

// Shipment Documents
const getShipment = async ({}: //   dateFrom,
//   dateTo,
//   documentNumbers,
//   clientIds,
//   statuses,
//   resourceIds,
//   unitIds,
//   search,
//   sortField,
//   ascending,
//   page,
//   size,
{
  dateFrom: string;
  dateTo: string;
  documentNumbers: string[];

  clientIds: number[];
  statuses: number[];

  resourceIds: number[];
  unitIds: number[];

  search: string;
  sortField: string;
  ascending: boolean;
  page: number;
  size: number;
}) => {
  return await post(`shipmentdocument/query`, {});
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
const getClient = async ({ page, size }: { page: number; size: number }) => {
  return await post(`client/query`, {
    page,
    size,
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
const getResource = async ({ page, size }: { page: number; size: number }) => {
  return await post(`resource/query`, {
    page,
    size
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
const getUnit = async ({ page, size }: { page: number; size: number }) => {
  return await post(`unit/query`, {
    page,
    size,
  });
};

const deleteUnit = async (id: number) => await del(`unit/${id}`);
const archiveUnit = async (id: number) => await patch(`unit/${id}/archive`);
const unArchiveUnit = async (id: number) => await patch(`unit/${id}/unarchive`);
const getUnitById = async (id: number) => await get(`unit/${id}`);
const updateUnit = async (data: object) => await put(`unit`, data);
const createUnit = async (data: object) => await post(`unit`, data);

// Unit
const getBalance = async ({}: //   search,
//   sortField,
//   ascending,
//   page,
//   size,
{
  search: string;
  sortField: string;
  ascending: boolean;
  page: number;
  size: number;
}) => {
  return await post(`balance/query`, {});
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
