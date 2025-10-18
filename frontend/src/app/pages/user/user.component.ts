import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DxFormModule, DxButtonModule, DxDataGridModule, DxPopupModule, DxScrollViewModule, DxTemplateModule } from 'devextreme-angular';
import { User, UserDTO } from '../../modules/user/models/user.dto';
import { UserService } from '../../modules/user/services/user.service';
import { FormUserComponent } from '../../modules/user/components/form-user/form-user.component';
import { ServicesService } from '../../core/services/services.service';
import { environment } from '../../../environments/environment';
import CustomStore from 'devextreme/data/custom_store';
import DataSource from 'devextreme/data/data_source';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ErrorPopupComponent } from '../../shared/templates/error-popup/error-popup.component';
import { SuccessPopupComponent } from '../../shared/templates/success-popup/success-popup.component';
import { ConfirmPopupComponent } from '../../shared/templates/confirm-popup/confirm-popup.component';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxButtonModule,
    DxDataGridModule,
    DxPopupModule,
    DxScrollViewModule,
    DxTemplateModule,
    FormUserComponent,
    ErrorPopupComponent,
    SuccessPopupComponent,
    ConfirmPopupComponent,
  ],
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  titulo = '';
  popupVisible = false;

  users!: DataSource;
  crud: { form: UserDTO; action: string } = { form: new User(), action: 'INSERT' };
  allowedRoles: string[] = [];
  currentRole: string | null = null;
  errorPopupVisible = false;
  errorTitle = '';
  errorMessage = '';
  successPopupVisible = false;
  successTitle = '';
  successMessage = '';
  confirmPopupVisible = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmButtonText = '';
  cancelButtonText = '';
  private userToDelete: any = null;

  constructor(
    private servicesService: ServicesService,
    private userService: UserService,
    private http: HttpClient,
  ) {
    this.initDataSource();
    this.currentRole = this.servicesService.getUserRole();
    this.setAllowedRoles();
  }

  ngOnInit(): void {
    this.initTitle();
  }

  private initDataSource(): void {
    const getToken = () => localStorage.getItem('token') ?? '';

    const store = new CustomStore({
      key: 'id',
      load: () => {
        const headers = new HttpHeaders({
          Authorization: `Bearer ${getToken()}`,
          Tenant: 'Tenant1',
          Accept: 'application/json;odata=verbose'
        });

        const url = `${environment.API_URL}Users`;

        return this.http.get<any>(url, { headers })
          .toPromise()
          .then(resp => {
            const items = Array.isArray(resp?.value) ? resp.value : resp;
            return items.map((item: any) => ({
              id: item?.userId,
              ...item
            }));
          });
      }
    });

    this.users = new DataSource({ store });
  }

  private initTitle(): void {
    switch (this.crud.action) {
      case 'INSERT':
        this.titulo = 'Ingresar Usuario';
        break;
      case 'EDIT':
        this.titulo = 'Editar Usuario';
        break;
      case 'VIEW':
        this.titulo = 'Detalles del Usuario';
        break;
    }
  }

  private listUsers(): void {
    this.users.reload();
  }

  private postInsert(crud: any): void {
    this.servicesService.setLoadingVisible(true);
    this.userService.register(crud.form).subscribe(() => {
      this.servicesService.notify('Usuario creado exitosamente', 'success');
      this.listUsers();
    });
  }

  crudUser(crud: any): void {
    this.popupVisible = false;
    switch (crud.action) {
      case 'INSERT':
        this.postInsert(crud);
        break;
    }
  }

  btnCrearUser(): void {
    if (!this.allowedRoles.length) {
      return;
    }
    this.crud = { form: new User(), action: 'INSERT' };
    this.initTitle();
    this.popupVisible = true;
  }

  private setAllowedRoles(): void {
    switch (this.currentRole) {
      case 'SUPERADMIN':
        this.allowedRoles = ['Admin', 'Operator'];
        break;
      case 'ADMIN':
        this.allowedRoles = ['Operator'];
        break;
      default:
        this.allowedRoles = [];
        break;
    }
  }

  actionUser(user: any, action: string): void {
    this.crud = { form: { ...user }, action };
    this.initTitle();
    this.popupVisible = true;
  }

  deleteUser(user: any): void {
    if (!user.id) return;

    const targetRole = (user.role || '').toUpperCase();

    if (this.currentRole === 'OPERATOR') {
      this.showError('Acceso denegado', 'No está autorizado para realizar esta acción.');
      return;
    }

    if (this.currentRole === 'ADMIN' && targetRole !== 'OPERATOR') {
      this.showError('Acceso denegado', 'No está autorizado para realizar esta acción.');
      return;
    }

    this.userToDelete = user;
    this.confirmTitle = 'Confirmación';
    this.confirmMessage = '¿Estás seguro que deseas eliminar este usuario?';
    this.confirmButtonText = 'Sí, eliminar';
    this.cancelButtonText = 'Mejor no';
    this.confirmPopupVisible = true;
  }

  onConfirmDelete(): void {
    if (!this.userToDelete?.id) return;
    this.confirmPopupVisible = false;
    this.servicesService.setLoadingVisible(true);
    this.userService.delete(this.userToDelete.id).subscribe(() => {
      this.successTitle = '¡Éxito!';
      this.successMessage = 'Usuario eliminado';
      this.successPopupVisible = true;
      this.listUsers();
      this.userToDelete = null;
    });
  }

  private showError(title: string, message: string): void {
    this.errorTitle = title;
    this.errorMessage = message;
    this.errorPopupVisible = true;
  }
}

