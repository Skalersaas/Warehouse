import { 
  // del, 
  get, 
  // post,
  // put 
  } from "../utils/api";

// Shipment Documents
const getShipmentDocs = async ({}: //   dateFrom,
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
  return await get(`shipmentdocuments`);
};

// Client
const getClient = async ({}:
//   search,
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
  return await get(`clients`);
};

// Resource
const getResource = async ({}: 
//   search,
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
  return await get(`resouces`);
};

// Unit
const getUnit = async ({}: 
//   search,
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
  return await get(`units`);
};

export { getShipmentDocs, getClient, getResource, getUnit };
