type Schema = {
  items?: Schema;
  $ref?: string;
  additionalProperties?: Schema;
};

type TypeDef = {
  type?: string;
  format?: string;
  description?: string;
  "x-ms-enum"?: {
    name: string;
    values: {
      value: string;
      description: string;
    }[];
  };
} & Schema;

export type Parameter = {
  name: string;
  in: "path" | "query" | "body" | "header";
  required?: boolean;
  schema?: Schema;
} & TypeDef;

type Property = TypeDef;

type Definition =
  | {
      type: string;
      description: string;
      allOf?: Schema[];
      properties?: {
        [name: string]: Property;
      };
    }
  | "string";

type Operation = {
  operationId: string;
  description: string;
  "x-ms-vss-method": string;
  "x-ms-vss-resource": string;
  "x-ms-docs-override-version": string;
  parameters?: Parameter[];
  responses: {
    [code: string]: {
      description: string;
      schema?: Schema;
    };
  };
};

export type OpenAPI = {
  openapi: string;
  info: {
    title: string;
    version: string;
  };
  host: string;
  "x-ms-vss-area": string;
  paths: {
    [path: string]: {
      [method: string]: Operation;
    };
  };
  "x-ms-paths": {
    [path: string]: {
      [method: string]: Operation;
    };
  };
  definitions: {
    [name: string]: Definition;
  };
};
