import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { FormDTO } from '../models/form.dto';

@Injectable({ providedIn: 'root' })
export class FormService {
  private readonly API_URL = `${environment.API_URL}Form`;

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

  getList(params: Record<string, any> = {}): Observable<FormDTO[]> {
    return this.http.get<FormDTO[]>(this.API_URL, { params: this.setParams(params) });
  }

  getFormTypes(): Observable<{ value: string; text: string }[]> {
    return new Observable(observer => {
      this.getList().subscribe({
        next: data => {
          const unique = Array.from(new Set(data.map(e => e.formType).filter(Boolean)));
          const types = unique.map(t => ({ value: t as string, text: t as string }));
          observer.next(types);
          observer.complete();
        },
        error: err => observer.error(err),
      });
    });
  }

  getById(id: string): Observable<FormDTO> {
    return this.http.get<FormDTO>(`${this.API_URL}/${id}`);
  }

  create(payload: FormDTO): Observable<FormDTO> {
    return this.http.post<FormDTO>(this.API_URL, payload);
  }

  update(id: string, payload: FormDTO): Observable<FormDTO> {
    return this.http.put<FormDTO>(`${this.API_URL}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}
