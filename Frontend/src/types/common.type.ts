export interface IShipmentDocument {
  id: number;
  number: string;
  date: string;

  clientId: number;
  clientName?: string;
  status: number;

  items: IDocumentItem[];
}

export interface IReceiptDocument {
  id: number;
  number: string;
  date: string;

  items: IDocumentItem[];
}

export interface ICommonDocument {
  id: number;
  number: string;
  date: string;

  clientId?: number;
  clientName?: string;
  status?: number;

  items: IDocumentItem[];
}

export interface IDocumentItem {
  id?: number;
  resourceId: number;
  resourceName?: string;
  unitId: number;
  unitName?: string;
  quantity: number;
}

export interface IBalance {
  id: number;
  resourceId: number;
  resourceName: string;
  unitId: number;
  unitName: string;
  quantity: number;
}

export interface ICommonType {
  id: number;
  name: string;
  isArchived: boolean | null;

  address?: string;
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

export interface IClient {
  id: number;
  name: string;
  address: string;
  isArchived: boolean;
}
