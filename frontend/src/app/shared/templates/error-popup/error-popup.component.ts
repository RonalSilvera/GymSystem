import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule } from 'devextreme-angular';

@Component({
  selector: 'app-error-popup',
  standalone: true,
  imports: [CommonModule, DxPopupModule],
  templateUrl: './error-popup.component.html',
  styleUrls: ['./error-popup.component.scss'],
})
export class ErrorPopupComponent {
  @Input() visible = false;
  @Input() title = '¡Oooops!';
  @Input() message = 'Algo salió mal.<br>Inténtalo de nuevo.';
  @Input() buttonText = 'Try Again';
  @Output() close = new EventEmitter<void>();

  onClose(): void {
    this.close.emit();
  }
}
