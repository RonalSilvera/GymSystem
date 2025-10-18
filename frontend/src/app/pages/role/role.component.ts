import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxButtonModule, DxDataGridModule, DxFormModule, DxPopupModule } from 'devextreme-angular';
import { Role } from '../../modules/role/models/role.dto';
import { RoleService } from '../../modules/role/services/role.service';
import { FormRoleComponent } from '../../modules/role/components/form-role/form-role.component';
import Swal from 'sweetalert2';
import { RouterModule } from '@angular/router';
import { ServicesService } from '../../core/services/services.service';

@Component({
  selector: 'app-role',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxButtonModule,
    DxDataGridModule,
    DxPopupModule,
    FormRoleComponent,
  ],
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  titulo = '';
  popupVisible = false;
  roles: Role[] = [];
  crud: { form: Role; action: string } = { form: new Role(), action: 'INSERT' };

  constructor(private servicesService: ServicesService, private roleService: RoleService) {}

  ngOnInit(): void {
    this.listRoles();
    this.initTitle();
  }

  private initTitle(): void {
    switch (this.crud.action) {
      case 'INSERT':
        this.titulo = 'Ingresar Rol';
        break;
      case 'EDIT':
        this.titulo = 'Editar Rol';
        break;
      case 'VIEW':
        this.titulo = 'Detalles del Rol';
        break;
    }
  }

  private listRoles(): void {
    this.servicesService.setLoadingVisible(true);
    this.roleService.getList().subscribe(data => {
      this.roles = data;
      this.servicesService.setLoadingVisible(false);
    });
  }

  private postInsert(crud: any): void {
    this.servicesService.setLoadingVisible(true);
    this.roleService.create(crud.form).subscribe(() => {
      this.servicesService.notify('Rol creado exitosamente', 'success');
      this.listRoles();
    });
  }

  private postUpdate(crud: any): void {
    if (!crud.form.roleId) return;
    this.servicesService.setLoadingVisible(true);
    this.roleService.update(crud.form.roleId, crud.form).subscribe(() => {
      this.servicesService.notify('Rol actualizado exitosamente', 'success');
      this.listRoles();
    });
  }

  private deleteRole(role: Role): void {
    if (!role.roleId) return;
    this.servicesService
      .notifyConfirmation({ title: '¿Eliminar rol?' })
      .then(confirm => {
        if (confirm) {
          this.servicesService.setLoadingVisible(true);
          this.roleService.delete(role.roleId!).subscribe(() => {
            this.servicesService.notify('Rol eliminado', 'success');
            this.listRoles();
          });
        }
      });
  }

  crudRole(crud: any): void {
    this.popupVisible = false;
    switch (crud.action) {
      case 'INSERT':
        this.postInsert(crud);
        break;
      case 'EDIT':
        this.postUpdate(crud);
        break;
    }
  }

  btnCrearRole(): void {
    this.crud = { form: new Role(), action: 'INSERT' };
    this.initTitle();
    this.popupVisible = true;
  }

  actionRole(role: Role, action: string): void {
    if (action === 'DELETE') {
      this.deleteRole(role);
      return;
    }
    this.crud = { form: { ...role }, action };
    this.initTitle();
    this.popupVisible = true;
  }
}
