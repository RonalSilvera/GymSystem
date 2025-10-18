import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FooterComponent, HeaderComponent } from '../components';
import { SideNavInnerToolbarComponent } from './side-nav-inner-toolbar/side-nav-inner-toolbar.component';
import { SideNavOuterToolbarComponent } from './side-nav-outer-toolbar/side-nav-outer-toolbar.component';
import { ScreenService } from '../../core/services/screen.service';
import { AuthService } from '../../core/services/auth.service';
import { ServicesService } from '../../core/services/services.service';
import { filter } from 'rxjs';


@Component({
  selector: 'app-layout',
  standalone: true, 
  imports: [
    CommonModule,
    RouterOutlet,
    FooterComponent,
    SideNavOuterToolbarComponent
  ],
   providers: [ScreenService, AuthService],
  templateUrl: './app-layout.component.html',
  styleUrls: ['./app-layout.component.scss']
})
export class AppLayoutComponent {
   isAuthRoute = false;

  constructor(private router: Router, public services: ServicesService) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.isAuthRoute = this.router.url.includes('auth');
      });
  }
}