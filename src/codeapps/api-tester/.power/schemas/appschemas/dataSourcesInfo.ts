/*!
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * This file is auto-generated. Do not modify it manually.
 * Changes to this file may be overwritten.
 */

export const dataSourcesInfo = {
  "csp_other_runcsharp": {
    "tableId": "",
    "version": "",
    "primaryKey": "",
    "dataSourceType": "Dataverse",
    "apis": {
      "csp_Other_RunCSharp": {
        "path": "/api/data/v9.2/csp_Other_RunCSharp",
        "method": "POST",
        "parameters": [
          {
            "name": "Code",
            "in": "body",
            "required": true,
            "type": "string"
          },
          {
            "name": "InputParameters",
            "in": "body",
            "required": false,
            "type": "string"
          },
          {
            "name": "ReferencedAssemblies",
            "in": "body",
            "required": false,
            "type": "string"
          },
          {
            "name": "UsingStatements",
            "in": "body",
            "required": false,
            "type": "string"
          },
          {
            "name": "Types",
            "in": "body",
            "required": false,
            "type": "string"
          },
          {
            "name": "InputTypeName",
            "in": "body",
            "required": false,
            "type": "string"
          },
          {
            "name": "OutputTypeName",
            "in": "body",
            "required": false,
            "type": "string"
          }
        ],
        "responseInfo": {
          "200": {
            "type": "object"
          }
        }
      }
    }
  }
};
