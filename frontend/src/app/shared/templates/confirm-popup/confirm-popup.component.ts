import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule } from 'devextreme-angular';

@Component({
  selector: 'app-confirm-popup',
  standalone: true,
  imports: [CommonModule, DxPopupModule],
  templateUrl: './confirm-popup.component.html',
  styleUrls: ['./confirm-popup.component.scss'],
})
export class ConfirmPopupComponent {
  @Input() visible = false;
  @Input() title = 'Confirmar';
  @Input() message = '¿Está seguro?';
  @Input() confirmButtonText = 'Aceptar';
  @Input() cancelButtonText = 'Cancelar';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onConfirm(): void {
    this.confirm.emit();
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
