import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit
} from '@angular/core';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxPopupModule } from 'devextreme-angular/ui/popup';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-menu-launcher',
  templateUrl: './app-menu-launcher.component.html',
  styleUrls: ['./app-menu-launcher.component.scss'],
  standalone: true,
  imports: [DxButtonModule, DxPopupModule, CommonModule]
})
export class AppMenuLauncherComponent implements AfterViewInit {
  @ViewChild('launcherBtn', { static: false }) launcherButton!: ElementRef;

  popupVisible = false;
  popupPosition: any;

  apps = [
    { name: 'Gmail', icon: 'email' },
    { name: 'Drive', icon: 'folder' },
    { name: 'Calendar', icon: 'event' },
    { name: 'Docs', icon: 'doc' },
    { name: 'Photos', icon: 'image' },
    { name: 'Sheets', icon: 'card' },
    { name: 'Meet', icon: 'video' },
    { name: 'Search', icon: 'search' },
    { name: 'YouTube', icon: 'video' },
    { name: 'Maps', icon: 'map' },
    { name: 'News', icon: 'tips' },
    { name: 'Gemini', icon: 'favorites' }
  ];

  ngAfterViewInit(): void {
    // Espera a que el botón esté renderizado para usar su posición
    setTimeout(() => {
      if (this.launcherButton) {
        this.popupPosition = {
          my: 'left top',
          at: 'left bottom',
          of: this.launcherButton.nativeElement
        };
      }
    });
  }

  togglePopup(): void {
    this.popupVisible = !this.popupVisible;
  }
}
