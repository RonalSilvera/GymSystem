// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

const API_BASE_URL = 'http://localhost:5004';
const API_BASE_PATH = '';
const API_PREFIX = '/api/';

export const environment = {
  production: false,
  API_BASE_URL,
  API_BASE_PATH,
  API_URL: `${API_BASE_URL}${API_BASE_PATH}${API_PREFIX}`,
  APIPRODUCT_URL: `${API_BASE_URL}${API_BASE_PATH}${API_PREFIX}`,
  ASSET_PATH: './assets/img/',
  API_TOKEN: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIyMjIyMjIyLTIyMjItMjIyMi0yMjIyLTIyMjIyMjIyMjIyMiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQG9yZy5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbmlzdHJhZG9yIiwiZXhwIjoxNzQ5MjY1MTg3LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjkyIiwiYXVkIjoiYXV0b3JpemFjacOzbiJ9.DF1eAvTNrDXo0GGVDDk_6LoS8UL_qlaK-1nIe6Aj6sU',
  TENANT: 'Tenant1'
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
