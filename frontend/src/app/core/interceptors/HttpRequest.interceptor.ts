import { ServicesService } from './../services/services.service';
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import notify from 'devextreme/ui/notify';
import { Router } from '@angular/router';
@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {
  constructor(
    private servicesService: ServicesService,
    private router: Router
  ) {}
  errorList: string[] = []
  extractErrors(response: any): string[] {
    const errors = response.errors || {};
    const errorList: string[] = [];
    
    // Iterar sobre las entradas del objeto errors
    for (const [field, messages] of Object.entries(errors)) {
      if (Array.isArray(messages)) {
        messages.forEach((message: string[]) => {
          errorList.push(`${field}: ${message}`);
        });
      }
    }
    
    return errorList;
  }

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    
    return next.handle(request).pipe(
      map((res: any) => {
        return res;
      }),
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Error desconocido';
        // Error de red o cliente
        if (error.error instanceof ErrorEvent){
          errorMessage = `Error: ${error.error.message}`;
        }  
        if (!error.error || error.error.errors === undefined){
          if (error.error?.ErrorMessage){
            errorMessage = `${error.error.ErrorMessage}`;
          }
          else{
            // Error del servidor
            errorMessage = 'Error del servidor';
          }
        }
        else{
          errorMessage = this.extractErrors(error.error).join("  ,  ");
        }
        if (!request.url.toLowerCase().includes('auth/login')) {
          notify(`${errorMessage}`, 'error', 2000);
        }
        this.servicesService.setLoadingVisible(false);
        return throwError(errorMessage);
      })
    );
  }
}
