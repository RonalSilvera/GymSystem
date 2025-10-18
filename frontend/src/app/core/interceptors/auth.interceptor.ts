import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private cookies: CookieService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const stored = this.cookies.get('access_token');
    const parsed = stored ? JSON.parse(stored) : null;
    const token = parsed?.token || environment.API_TOKEN;
    const tenant = environment.TENANT;

    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
          tenant: tenant
        }
      });
    }

    return next.handle(req);
  }
}
