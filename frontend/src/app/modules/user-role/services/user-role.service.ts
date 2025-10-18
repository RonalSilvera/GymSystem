import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserRoleDTO } from '../models/user-role.dto';

@Injectable({ providedIn: 'root' })
export class UserRoleService {
  private readonly API_URL = `${environment.API_URL}UserRoles`;

  constructor(private http: HttpClient) {}

  assignRole(userId: string, roleId: string): Observable<void> {
    const params = new HttpParams().set('userId', userId).set('roleId', roleId);
    return this.http.post<void>(`${this.API_URL}/assign`, {}, { params });
  }

  getByUser(userId: string): Observable<UserRoleDTO[]> {
    return this.http.get<UserRoleDTO[]>(`${this.API_URL}/${userId}`);
  }
}
