import { IUser } from './../../../core/services/auth.service';
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxToolbarModule } from 'devextreme-angular/ui/toolbar';
import { AuthService } from '../../../core/services/auth.service';
import { UserPanelComponent } from '../user-panel/user-panel.component';
import { ThemeSwitcherComponent } from '../theme-switcher/theme-switcher.component';
import { AppMenuLauncherComponent } from "../app-menu/app-menu-launcher.component";
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  templateUrl: 'header.component.html',
  styleUrls: ['./header.component.scss'],
  standalone: true,  
  imports: [
    CommonModule,
    DxButtonModule,
    UserPanelComponent,
    DxToolbarModule,
    ThemeSwitcherComponent,
    AppMenuLauncherComponent
]
})
export class HeaderComponent implements OnInit, OnDestroy {
  @Output()
  menuToggle = new EventEmitter<boolean>();

  @Input()
  menuToggleEnabled = false;

  @Input()
  title!: string;

  user: IUser | null = { email: '', avatarUrl: 'assets/img/default-avatar.png' };
  private userSub?: Subscription;

  userMenuItems = [{
    text: 'Profile',
    icon: 'user',
    onClick: () => {
      this.router.navigate(['/profile']);
    }
  },
  {
    text: 'Logout',
    icon: 'runner',
    onClick: () => {
      this.authService.logOut();
    }
  }];

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
    this.userSub = this.authService.user$.subscribe(u => {
      this.user = u || { email: '', avatarUrl: 'assets/img/default-avatar.png' };
    });
  }

  ngOnDestroy() {
    this.userSub?.unsubscribe();
  }

  toggleMenu = () => {
    this.menuToggle.emit();
  }

  
}
