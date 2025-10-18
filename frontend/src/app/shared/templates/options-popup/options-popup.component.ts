import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopoverModule } from 'devextreme-angular/ui/popover';
import { DxListModule } from 'devextreme-angular/ui/list';

@Component({
  selector: 'app-options-popup',
  standalone: true,
  imports: [CommonModule, DxPopoverModule, DxListModule],
  templateUrl: './options-popup.component.html',
  styleUrls: ['./options-popup.component.scss']
})
export class OptionsPopupComponent {
  @Input() target: any;
  @Input() visible = false;
  @Input() width = 180;
  @Input() items: { text: string; action: string; icon?: string }[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() select = new EventEmitter<string>();
}
