import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule } from 'devextreme-angular';

@Component({
  selector: 'app-success-popup',
  standalone: true,
  imports: [CommonModule, DxPopupModule],
  templateUrl: './success-popup.component.html',
  styleUrls: ['./success-popup.component.scss'],
})
export class SuccessPopupComponent {
  @Input() visible = false;
  @Input() title = '¡Éxito!';
  @Input() message = 'La operación se completó correctamente.';
  @Input() buttonText = 'Aceptar';
  @Output() close = new EventEmitter<void>();

  onClose(): void {
    this.close.emit();
  }
}
