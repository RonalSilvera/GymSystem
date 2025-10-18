const API_BASE_URL = 'http://localhost:5004';
const API_BASE_PATH = '/gymsystem-back';
const API_PREFIX = '/api/';

export const environment = {
  production: true,
  ASSET_PATH: './assets/img/',
  API_BASE_URL,
  API_BASE_PATH,
  API_URL: `${API_BASE_URL}${API_BASE_PATH}${API_PREFIX}`,
  APIPRODUCT_URL: `${API_BASE_URL}${API_BASE_PATH}${API_PREFIX}`,
  API_TOKEN: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIyMjIyMjIyLTIyMjItMjIyMi0yMjIyLTIyMjIyMjIyMjIyMiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQG9yZy5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbmlzdHJhZG9yIiwiZXhwIjoxNzQ5MjY1MTg3LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjkyIiwiYXVkIjoiYXV0b3JpemFjacOzbiJ9.DF1eAvTNrDXo0GGVDDk_6LoS8UL_qlaK-1nIe6Aj6sU',
  TENANT: 'Tenant1'
};
