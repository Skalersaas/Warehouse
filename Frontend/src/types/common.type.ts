export interface IShipmentDocument {
  id: number;
  number: string;
  clientId: number;
  date: string;
  status: number;
  items: {
    id?: number;
    resourceId: number;
    unitId: number;
    quantity: number;
  }[];
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
}

export interface ICommonType {
  id: number;
  name: string;
  isArchived: boolean;

  address?: string;
  status?: number;
}

export interface IBalance {
  id: number;
  resourceId: number;
  unitId: number;
  quantity: number;
}

export interface IReceiptDocument {
  id: number;
  number: string;
  date: string;
  items: {
    id?: number;
    resourceId: number;
    unitId: number;
    quantity: number;
  }[];
}

export interface IResource {
  id: number;
  name: string;
  isArchived: boolean;
}

export interface IUnit {
  id: number;
  name: string;
  isArchived: boolean;
}
