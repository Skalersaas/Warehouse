export interface IShipmentDocument {
  id: number;
  number: string;
  clientId: number;
  clientName: string;
  date: string;
  status: number;
  statusName: string;
  createdAt: string;
  updatedAt: string;
}

export interface IShipmentResource {
  id: number;
  resourceId: number;
  resourceName: string;
  unitId: number;
  unitName: string;
  quantity: number;
}

export interface IShipment {
  id: number;
  number: string;
  clientId: number;
  clientName: string;
  date: string;
  status: number;
  statusName: string;
  createdAt: string;
  updatedAt: string;
  items: IShipmentResource[];
}

export interface IClient {
  id: number;
  name: string;
  address: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ICommonType {
  id: number;
  name: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;

  address?: string;
}
