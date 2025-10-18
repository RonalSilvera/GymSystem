import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { ServicesService } from '../services/services.service';


@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

  constructor( private service: ServicesService, private router: Router ){

  }

  canActivate(routeActive: ActivatedRouteSnapshot): boolean {

    if (!this.service.isAuthenticated()) {
      this.router.navigate(['auth']);
      return false;
    }

    const roles = routeActive.data['roles'] as Array<string>;
    const userRole = this.service.getUserRole();
    if (userRole && roles.map(r => r.toUpperCase()).includes(userRole.toUpperCase())) {
      return true;
    }
    this.service.notify('No tienes permisos o no has iniciado sesión', 'error');
    this.router.navigate(['/']);
    return false;
  }

}
