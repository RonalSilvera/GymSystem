import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ValidationCallbackData } from 'devextreme-angular/common';
import { DxFormModule } from 'devextreme-angular/ui/form';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import notify from 'devextreme/ui/notify';
import { AuthService } from '../../../core/services/auth.service';
AuthService
@Component({
  selector: 'app-change-passsword-form',
  templateUrl: './change-password-form.component.html',
  standalone: true,  // El componente ahora es standalone
  imports: [  // Importa los módulos directamente aquí
    CommonModule,
    DxFormModule,
    DxLoadIndicatorModule
  ]
})
export class ChangePasswordFormComponent {
  loading = false;
  formData: any = {};
  recoveryCode: string = '';

  constructor(private authService: AuthService, private router: Router, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.recoveryCode = params.get('recoveryCode') || '';
    });
  }

  async onSubmit(e: Event) {
    e.preventDefault();
    const { password } = this.formData;
    this.loading = true;

    const result = await this.authService.changePassword(password, this.recoveryCode);
    this.loading = false;

    if (result.isOk) {
      this.router.navigate(['/login-form']);
    } else {
      notify(result.message, 'error', 2000);
    }
  }

  confirmPassword = (e: ValidationCallbackData) => {
    return e.value === this.formData.password;
  }
}
