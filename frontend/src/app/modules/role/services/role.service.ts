import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RoleDTO } from '../models/role.dto';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly API_URL = `${environment.API_URL}Roles`;

  constructor(private http: HttpClient) {}

  private setParams(params: Record<string, any>): HttpParams {
    let httpParams = new HttpParams();
    for (const key of Object.keys(params)) {
      if (params[key] !== undefined && params[key] !== null) {
        httpParams = httpParams.set(key, params[key]);
      }
    }
    return httpParams;
  }

  getList(params: Record<string, any> = {}): Observable<RoleDTO[]> {
    return this.http.get<RoleDTO[]>(this.API_URL, { params: this.setParams(params) });
  }

  getById(id: string): Observable<RoleDTO> {
    return this.http.get<RoleDTO>(`${this.API_URL}/${id}`);
  }

  create(payload: RoleDTO): Observable<RoleDTO> {
    return this.http.post<RoleDTO>(this.API_URL, payload);
  }

  update(id: string, payload: RoleDTO): Observable<RoleDTO> {
    return this.http.put<RoleDTO>(`${this.API_URL}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}
