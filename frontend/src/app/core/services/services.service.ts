import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import Swal from 'sweetalert2';
import { CookieService } from 'ngx-cookie-service';
import * as reduxActions from '../store/actions';
import { AppState } from '../store/app.reducers';
import { Store } from '@ngrx/store';
import { JwtHelperService } from '@auth0/angular-jwt';
import notify from 'devextreme/ui/notify';
import { environment } from '../../../environments/environment';

export interface Client {
  clientId: number;
  fullName: string;
  documentNumber: string;
  email?: string;
  phone?: string;
  address?: string;
  refId: string;
  statusId: number;
  membershipStatusId?: number | null;
  membershipStatusName?: string | null;
  membershipTypeId?: number | null;
  membershipTypeName?: string | null;
  lastPaymentAmount?: number | null;
  amountPaid?: number | null;
  amountDue?: number | null;
  clearMembership?: boolean;
}

export interface MembershipType {
  id: number;
  name: string;
  price: number;
}

export interface PaymentMethod {
  id: number;
  name: string;
}

export interface MonthlyMetric {
  year: number;
  month: number;
  label: string;
  count: number;
}

export interface MembershipTypeDistribution {
  membershipTypeId: number;
  name: string;
  count: number;
}

export interface DashboardSummary {
  totalClients: number;
  activeClients: number;
  expiringMemberships: number;
  todaysAccesses: number;
  monthlyNewClients: MonthlyMetric[];
  membershipTypeDistribution: MembershipTypeDistribution[];
}

@Injectable({
  providedIn: 'root'
})

export class ServicesService {
  API_URL: string = environment.API_URL;
  APIPRODUCT_URL: string = environment.APIPRODUCT_URL;

  constructor(
    private http: HttpClient,
    private cookies: CookieService,
    private store: Store<AppState>,
    private helper: JwtHelperService
  ) {}

  setLoadingVisible(loadingVisible: boolean): void {
    this.store.dispatch(reduxActions.setLoadingVisible({ loadingVisible }));
  }

  isAuthenticated(): boolean {
    const token = this.getToken()?.token;
    return token ? !this.helper.isTokenExpired(token) : false;
  }

  getToken(): any {
    const token = this.cookies.get('access_token');
    return token ? JSON.parse(token) : null;
  }

  getUserRole(): string | null {
    const token = this.getToken()?.token;
    if (!token) {
      return null;
    }
    const decoded = this.helper.decodeToken(token);
    const claim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const role = decoded?.[claim] || decoded?.role || null;
    return role ? role.toUpperCase() : null;
  }

  notify(message: string, type: 'info' | 'warning' | 'error' | 'success'): void {
    notify(
      {
        message,
        type,
        displayTime: 3500,
        animation: {
          show: { type: 'fade', duration: 400, from: 0, to: 1 },
          hide: { type: 'fade', duration: 40, to: 0 }
        }
      },
      {
        position: 'bottom center',
        direction: 'up-push'
      }
    );
  }

  notifyConfirmation(params: any = {}): Promise<boolean> {
    return new Promise(resolve => {
      Swal.fire(params).then(result => resolve(result.isConfirmed));
    });
  }

  notifyPrompt(title: string, inputType: 'text' | 'email' | 'password'): Promise<string | null> {
    return new Promise(resolve => {
      Swal.fire({
        title,
        input: inputType,
        inputAttributes: {
          autocapitalize: 'off'
        },
        showCancelButton: true,
        confirmButtonText: 'Aceptar',
        cancelButtonText: 'Cancelar',
        showLoaderOnConfirm: true,
        preConfirm: (inputValue) => {
          if (inputValue) return inputValue;
          else Swal.showValidationMessage('El campo no puede estar vacío');
        }
      }).then(result => {
        if (result.isConfirmed) resolve(result.value);
        else resolve(null);
      });
    });
  }

  // 👉 Ejemplo de función de dashboard que puedes activar si decides usarla
  public getDashboardCount(): Observable<any> {
    return this.http.get(`${this.API_URL}tablero`);
  }

  public getRevenueToday(): Observable<number> {
    const params = this.setHttpParams({ from: 'today', to: 'today' });
    return this.http
      .get<any>(`${this.API_URL}reports/revenue`, { params })
      .pipe(map(res => (typeof res === 'number' ? res : res?.total ?? res?.revenue ?? 0)));
  }

  public getAttendanceToday(): Observable<number> {
    const params = this.setHttpParams({ from: 'today', to: 'today' });
    return this.http
      .get<any>(`${this.API_URL}reports/attendance`, { params })
      .pipe(
        map(res => {
          if (Array.isArray(res)) {
            return res.reduce(
              (sum, item) =>
                sum + (item?.count ?? item?.total ?? item?.amount ?? 0),
              0
            );
          }
          return res?.sum ?? res?.count ?? res?.total ?? 0;
        })
      );
  }

  public getExpiringMemberships(days: number = 7): Observable<number> {
    const params = this.setHttpParams({ days });
    return this.http
      .get<any>(`${this.API_URL}reports/expiring`, { params })
      .pipe(map(res => (Array.isArray(res) ? res.length : res?.count ?? 0)));
  }

  public getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<Partial<DashboardSummary>>(`${this.API_URL}dashboard`).pipe(
      map(res => {
        const monthlyRaw = Array.isArray(res?.monthlyNewClients) ? res.monthlyNewClients : [];
        const membershipsRaw = Array.isArray(res?.membershipTypeDistribution)
          ? res.membershipTypeDistribution
          : [];
        return {
          totalClients: res?.totalClients ?? 0,
          activeClients: res?.activeClients ?? 0,
          expiringMemberships: res?.expiringMemberships ?? 0,
          todaysAccesses: res?.todaysAccesses ?? 0,
          monthlyNewClients: monthlyRaw.map(item => ({
            year: item?.year ?? 0,
            month: item?.month ?? 0,
            label: item?.label ?? '',
            count: item?.count ?? 0
          })),
          membershipTypeDistribution: membershipsRaw.map(item => ({
            membershipTypeId: item?.membershipTypeId ?? 0,
            name: item?.name ?? '',
            count: item?.count ?? 0
          }))
        };
      })
    );
  }

  public getClients(
    search: string = '',
    page: number = 1,
    pageSize: number = 10,
    sortField?: string,
    sortOrder?: 'asc' | 'desc',
    statusId?: number | null
  ): Observable<{ items: Client[]; total: number; active: number; inactive: number }> {
    const paramsObj: Record<string, any> = { search, page, pageSize };
    if (sortField) paramsObj['sortField'] = sortField;
    if (sortOrder) paramsObj['sortOrder'] = sortOrder;
    if (statusId != null) paramsObj['statusId'] = statusId;
    const params = this.setHttpParams(paramsObj);
    return this.http.get<{ total: number; active: number; inactive: number; items: Client[] }>(`${this.API_URL}clients`, { params });
  }

  public getClientStats(statusId?: number | null): Observable<{ total: number; active: number; inactive: number }> {
    const paramsObj: Record<string, any> = { page: 1, pageSize: 0 };
    if (statusId != null) paramsObj['statusId'] = statusId;
    const params = this.setHttpParams(paramsObj);
    return this.http
      .get<{ total: number; active: number; inactive: number }>(`${this.API_URL}clients`, { params })
      .pipe(map(res => ({ total: res.total, active: res.active, inactive: res.inactive })));
  }

  public createClient(dto: Partial<Client>): Observable<Client> {
    return this.http.post<Client>(`${this.API_URL}clients`, dto);
  }

  public updateClient(id: number, dto: Partial<Client>): Observable<Client> {
    return this.http.put<Client>(`${this.API_URL}clients/${id}`, dto);
  }

  public getClientById(id: number): Observable<Client> {
    return this.http.get<Client>(`${this.API_URL}clients/${id}`);
  }

  public getMembershipTypes(): Observable<MembershipType[]> {
    return this.http.get<MembershipType[]>(`${this.API_URL}membershiptypes`);
  }

  public getPaymentMethods(): Observable<PaymentMethod[]> {
    return this.http.get<PaymentMethod[]>(`${this.API_URL}paymentmethods`);
  }

  public payMembership(
    clientId: number,
    membershipTypeId: number,
    paymentMethodId: number,
    discountAmount?: number,
    couponCode?: string,
    referenceText?: string
  ): Observable<any> {
    return this.http.post(`${this.API_URL}payments`, {
      clientId,
      membershipTypeId,
      paymentMethodId,
      discountAmount,
      couponCode,
      referenceText
    });
  }

  public payPartialMembership(
    clientId: number,
    membershipTypeId: number,
    paymentMethodId: number,
    amount: number,
    referenceText?: string
  ): Observable<any> {
    return this.http.post(`${this.API_URL}payments/partial`, {
      clientId,
      membershipTypeId,
      paymentMethodId,
      amount,
      referenceText
    });
  }

  public deleteClient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}clients/${id}`);
  }

  public reactivateClient(id: number): Observable<void> {
    return this.http.patch<void>(`${this.API_URL}clients/${id}/reactivate`, {});
  }

  // 👉 Función utilitaria opcional para convertir objetos en HttpParams
  public setHttpParams(params: Record<string, any>): HttpParams {
    let queryParams = new HttpParams();
    for (const key in params) {
      if (params.hasOwnProperty(key)) {
        queryParams = queryParams.set(key, params[key]);
      }
    }
    return queryParams;
  }
}
