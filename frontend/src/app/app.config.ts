import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter} from '@angular/router';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { routes } from './app.routes';
routes
export const appConfig = {
  providers: [
    provideRouter(routes)
  ]
};
