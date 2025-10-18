import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DxButtonModule, DxDataGridModule, DxFormModule, DxPopupModule, DxScrollViewModule } from 'devextreme-angular';
import { Form } from '../../modules/form/models/form.dto';
import { FormService } from '../../modules/form/services/form.service';
import { FormFormComponent } from '../../modules/form/components/form-form/form-form.component';
import Swal from 'sweetalert2';
import { ServicesService } from '../../core/services/services.service';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxButtonModule,
    DxDataGridModule,
    DxPopupModule,
    DxScrollViewModule,
    FormFormComponent,
  ],
})
export class FormComponent implements OnInit {
  titulo = '';
  popupVisible = false;
  entities: Form[] = [];
  crud: { form: Form; action: string } = { form: new Form(), action: 'INSERT' };
  formTypes: { value: string; text: string }[] = [];

  constructor(
    private servicesService: ServicesService,
    private formService: FormService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.listEntities();
    this.formService.getFormTypes().subscribe(types => (this.formTypes = types));
    this.initTitle();
  }

  private initTitle(): void {
    switch (this.crud.action) {
      case 'INSERT':
        this.titulo = 'Ingresar Formulario';
        break;
      case 'EDIT':
        this.titulo = 'Editar Formulario';
        break;
      case 'VIEW':
        this.titulo = 'Detalles del Formulario';
        break;
    }
  }

  private listEntities(): void {
    this.servicesService.setLoadingVisible(true);
    this.formService.getList().subscribe(data => {
      this.entities = data;
      this.servicesService.setLoadingVisible(false);
    });
  }

  private postInsert(crud: any): void {
    this.servicesService.setLoadingVisible(true);
    this.formService.create(crud.form).subscribe(() => {
      this.servicesService.notify('Formulario creado exitosamente', 'success');
      this.listEntities();
    });
  }

  private putUpdate(crud: any): void {
    if (!crud.form.formId) return;
    this.servicesService.setLoadingVisible(true);
    this.formService.update(crud.form.formId, crud.form).subscribe(() => {
      this.servicesService.notify('Formulario editado exitosamente', 'success');
      this.listEntities();
    });
  }

  crudForm(crud: any): void {
    this.popupVisible = false;
    switch (crud.action) {
      case 'INSERT':
        this.postInsert(crud);
        break;
      case 'EDIT':
        this.putUpdate(crud);
        break;
    }
  }

  deleteForm(form: Form): void {
    if (!form.formId) return;
    Swal.fire({
      title: `¿Estás seguro que deseas eliminar este formulario ${form.name}?`,
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Mejor no',
    }).then(result => {
      if (result.isConfirmed) {
        this.servicesService.setLoadingVisible(true);
        this.formService.delete(form.formId!).subscribe(() => {
          this.servicesService.notify('Formulario eliminado', 'success');
          this.listEntities();
        });
      }
    });
  }

  btnCrearForm(): void {
    this.router.navigate(['/dashboard']);
  }

  actionForm(form: Form, action: string): void {
    this.crud = { form: { ...form }, action };
    this.initTitle();
    this.popupVisible = true;
  }
}
