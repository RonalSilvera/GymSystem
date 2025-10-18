import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { DxFormModule, DxButtonModule } from 'devextreme-angular';
import { User } from '../../models/user.dto';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-user',
  standalone: true,
  imports: [DxFormModule, DxButtonModule, CommonModule],
  templateUrl: './form-user.component.html',
  styleUrl: './form-user.component.scss'
})
export class FormUserComponent implements OnInit {
  @Output() crudEvent = new EventEmitter<any>();
  @Input() crud: { form: User; action: string } = { form: new User(), action: 'INSERT' };
  @Input() allowedRoles: string[] = [];
  modoView = false;
  roles: { value: string; text: string }[] = [];

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    this.modoView = this.crud.action === 'VIEW';
    this.roles = this.allowedRoles.map(r => ({ value: r, text: r }));
  }

  submit(event: Event): void {
    event.preventDefault();
    this.crudEvent.emit({ ...this.crud });
    this.crud.form = new User();
  }

  buttonOptionsSave = {
    text: 'Guardar',
    type: 'default',
    icon: 'fa fa-save',
    width: '200',
    useSubmitBehavior: true,
  };
}
