import { Component, EventEmitter, Input, Output, OnInit, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxFormModule, DxButtonModule, DxSelectBoxModule } from 'devextreme-angular';
import { Form } from '../../models/form.dto';

@Component({
  selector: 'app-form-form',
  standalone: true,
  imports: [DxFormModule, DxButtonModule, DxSelectBoxModule, CommonModule],
  templateUrl: './form-form.component.html',
  styleUrls: ['./form-form.component.scss']
})
export class FormFormComponent implements OnInit {
  @Input() crud: { form: Form; action: string } = { form: new Form(), action: 'INSERT' };
  @Input() formTypes: { value: string; text: string }[] = [];
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
