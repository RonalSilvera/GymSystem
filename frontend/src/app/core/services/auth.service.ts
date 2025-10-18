import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../../../environments/environment';

export interface IUser {
  email: string;
  avatarUrl?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

const defaultPath = '/';
const DEFAULT_AVATAR = 'assets/img/default-avatar.png';
const defaultUser = {
  email: 'sandra@example.com',
  avatarUrl: DEFAULT_AVATAR
};

@Injectable()
export class AuthService {
  private readonly LOGIN_URL = `${environment.API_URL}Auth/login`;
  private readonly LOGOUT_URL = `${environment.API_URL}Auth/logout`;
  private _user: IUser | null = defaultUser;
  private userSubject = new BehaviorSubject<IUser | null>(this._user);
  user$ = this.userSubject.asObservable();
  get loggedIn(): boolean {
    return !!this._user;
  }

  private _lastAuthenticatedPath: string = defaultPath;
  set lastAuthenticatedPath(value: string) {
    this._lastAuthenticatedPath = value;
  }

  constructor(private http: HttpClient, private cookies: CookieService, private router: Router) {
    const stored = localStorage.getItem('user');
    if (stored) {
      this._user = JSON.parse(stored);
    }
    this.userSubject.next(this._user);
  }

  getToken(): string | null {
    const token = this.cookies.get('access_token');
    if (token) {
      const parsedToken = JSON.parse(token);
      return parsedToken.token;
    }
    return null;
  }

  login(payload: LoginRequest): Observable<any> {
    const headers = new HttpHeaders({
      tenant: environment.TENANT,
      'Content-Type': 'application/json',
      Accept: '*/*'
    });

    return this.http.post<any>(this.LOGIN_URL, payload, { headers });
  }

  logout(): void {
    this.http.post<any>(this.LOGOUT_URL, {}).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  private clearSession(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.clearToken();
    this._user = null;
    this.userSubject.next(this._user);
    this.router.navigate(['/auth']);
  }

  saveToken(token: string): void {
    this.cookies.set('access_token', JSON.stringify({ token }));
  }

  clearToken(): void {
    this.cookies.delete('access_token');
  }

  async logIn(email: string, password: string) {

    try {
      // Send request
      this._user = { ...defaultUser, email };
      localStorage.setItem('user', JSON.stringify(this._user));
      this.userSubject.next(this._user);
      this.router.navigate([this._lastAuthenticatedPath]);

      return {
        isOk: true,
        data: this._user
      };
    }
    catch {
      return {
        isOk: false,
        message: "Authentication failed"
      };
    }
  }

  async getUser() {
    try {
      // Send request

      return {
        isOk: true,
        data: this._user
      };
    }
    catch {
      return {
        isOk: false,
        data: null
      };
    }
  }

  updateAvatar(avatarUrl: string) {
    if (this._user) {
      this._user.avatarUrl = avatarUrl;
    } else {
      this._user = { email: '', avatarUrl };
    }

    localStorage.setItem('user', JSON.stringify(this._user));
    this.userSubject.next(this._user);
  }

  setUser(user: IUser) {
    this._user = user;
    localStorage.setItem('user', JSON.stringify(this._user));
    this.userSubject.next(this._user);
  }

  async createAccount(email: string, password: string) {
    try {
      // Send request

      this.router.navigate(['/create-account']);
      return {
        isOk: true
      };
    }
    catch {
      return {
        isOk: false,
        message: "Failed to create account"
      };
    }
  }

  async changePassword(email: string, recoveryCode: string) {
    try {
      // Send request

      return {
        isOk: true
      };
    }
    catch {
      return {
        isOk: false,
        message: "Failed to change password"
      }
    }
  }

  async resetPassword(email: string) {
    try {
      // Send request

      return {
        isOk: true
      };
    }
    catch {
      return {
        isOk: false,
        message: "Failed to reset password"
      };
    }
  }

  logOut(): void {
    this.logout();
  }
}

@Injectable()
export class AuthGuardService implements CanActivate {
  constructor(private router: Router, private authService: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const isLoggedIn = this.authService.loggedIn;
    const isAuthForm = [
      'login-form',
      'reset-password',
      'create-account',
      'change-password/:recoveryCode'
    ].includes(route.routeConfig?.path || defaultPath);

    if (isLoggedIn && isAuthForm) {
      this.authService.lastAuthenticatedPath = defaultPath;
      this.router.navigate([defaultPath]);
      return false;
    }

    if (!isLoggedIn && !isAuthForm) {
      this.router.navigate(['/login-form']);
    }

    if (isLoggedIn) {
      this.authService.lastAuthenticatedPath = route.routeConfig?.path || defaultPath;
    }

    return isLoggedIn || isAuthForm;
  }
}
