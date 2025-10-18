import { routes } from './app/app.routes';
import { provideRouter } from '@angular/router';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { appReducers } from './app/core/store/app.reducers';
import { config } from 'devextreme/common';
import { locale } from 'devextreme/localization';
import * as esMessages from 'devextreme/localization/messages/es.json';
import { loadMessages } from 'devextreme/localization';
import { licenseKey } from './devextreme-license';
import { AppLayoutComponent } from './app/shared/layouts/app-layout.component';
import { JWT_OPTIONS, JwtHelperService } from '@auth0/angular-jwt';
import { SideNavOuterToolbarComponent } from './app/shared/layouts';
import { AuthInterceptor } from './app/core/interceptors/auth.interceptor';
import { HttpRequestInterceptor } from './app/core/interceptors/HttpRequest.interceptor';

bootstrapApplication(AppLayoutComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideStore(appReducers),
    JwtHelperService,
    { provide: JWT_OPTIONS, useValue: {} },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: HttpRequestInterceptor, multi: true },
  ]
});
 
config({ licenseKey }); 


loadMessages(esMessages);

locale('es');