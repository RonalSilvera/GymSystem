import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserDTO } from '../models/user.dto';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly API_URL = `${environment.API_URL}Users`;

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

  getList(params: Record<string, any> = {}): Observable<UserDTO[]> {
    return this.http.get<UserDTO[]>(this.API_URL, { params: this.setParams(params) });
  }

  register(payload: UserDTO): Observable<UserDTO> {
    return this.http.post<UserDTO>(`${this.API_URL}/register`, payload);
  }

  getById(id: string): Observable<UserDTO> {
    return this.http.get<UserDTO>(`${this.API_URL}/${id}`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  update(id: string, payload: Partial<UserDTO>): Observable<UserDTO> {
    return this.http.put<UserDTO>(`${this.API_URL}/${id}`, payload);
  }
}
