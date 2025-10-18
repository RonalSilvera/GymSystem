import { Component, EventEmitter, Input, Output, OnInit, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxFormModule, DxButtonModule } from 'devextreme-angular';
import { Role } from '../../models/role.dto';

@Component({
  selector: 'app-form-role',
  standalone: true,
  imports: [DxFormModule, DxButtonModule, CommonModule],
  templateUrl: './form-role.component.html'
})
export class FormRoleComponent implements OnInit {
  @Input() crud: { form: Role; action: string } = { form: new Role(), action: 'INSERT' };
  @Output() crudEvent = new EventEmitter<any>();
  modoView = false;

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    this.modoView = this.crud.action === 'VIEW';
  }

  submit(e: Event): void {
    e.preventDefault();
    this.crudEvent.emit({ ...this.crud });
  }

  buttonOptionsSave = {
    text: 'Guardar',
    type: 'default',
    icon: 'fa fa-save',
    width: '200',
    useSubmitBehavior: true,
  };
}
