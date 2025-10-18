import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DxFormModule, DxButtonModule } from 'devextreme-angular';
import { ErrorPopupComponent } from '../../shared/templates';
import { AuthService, LoginRequest } from '../../core/services/auth.service';
import { UserService } from '../../modules/user/services/user.service';
import { ServicesService } from '../../core/services/services.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DxFormModule, DxButtonModule, ErrorPopupComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class LoginComponent {
  credentials: LoginRequest = { email: '', password: '' };
  isPopupVisible = false; // Controla la visibilidad del popup de error

  // Propiedades para personalizar el modal de error
  errorTitle: string = '¡Oooops!';
  errorMessage: string = 'Credenciales incorrectas.<br>Inténtalo de nuevo.';
  private readonly apiBase = environment.API_URL.replace('api/', '');

  constructor(
    private auth: AuthService,
    private router: Router,
    private services: ServicesService,
    private userService: UserService
  ) {}

  submit(e: Event): void {
    e.preventDefault();

    this.services.setLoadingVisible(true);
    this.auth.login(this.credentials).subscribe({
      next: (res: any) => {
        if (res && res.token) {
          this.auth.saveToken(res.token);
          const payload = JSON.parse(atob(res.token.split('.')[1]));
          const idClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
          const emailClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
          const userId = payload[idClaim] || payload['sub'];
          const email = payload[emailClaim];

          this.userService.getById(userId).subscribe({
            next: u => {
              const avatar = this.getImageUrl(u.profileImageUrl) || 'assets/img/default-avatar.png';
              this.auth.setUser({ email, avatarUrl: avatar });
              this.services.setLoadingVisible(false);
              this.router.navigate(['/dashboard']);
            },
            error: () => {
              this.auth.setUser({ email, avatarUrl: 'assets/img/default-avatar.png' });
              this.services.setLoadingVisible(false);
              this.router.navigate(['/dashboard']);
            }
          });
        } else {
          this.services.setLoadingVisible(false);
          this.showErrorModal('¡Error de Autenticación!', 'El correo electrónico o la contraseña<br>son incorrectos. Inténtalo de nuevo.');
        }
      },
      error: () => {
        this.services.setLoadingVisible(false);
        // Personalizar mensaje según el tipo de error
        this.showErrorModal('¡Error de Autenticación!', 'El correo electrónico o la contraseña<br>son incorrectos. Inténtalo de nuevo.');
      }
    });
  }

  private getImageUrl(fileName?: string | null): string | null {
    return fileName ? `${this.apiBase}profile-images/${fileName}` : null;
  }

  // Método para mostrar el modal de error con mensajes personalizados
  showErrorModal(title: string = '¡Oooops!', message: string = 'Algo salió mal.<br>Inténtalo de nuevo.') {
    this.errorTitle = title;
    this.errorMessage = message;
    this.isPopupVisible = true;
  }

  // Método para cerrar el modal de error
  closeErrorModal(): void {
    this.isPopupVisible = false;
    // Opcional: limpiar los campos del formulario
    // this.credentials = { email: '', password: '' };
  }

  buttonOptions = {
    text: 'Ingresar',
    type: 'default',
    icon: 'fas fa-sign-in-alt',
    useSubmitBehavior: true,
    width: 150
  };
}