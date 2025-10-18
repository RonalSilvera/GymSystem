import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxTreeViewTypes } from 'devextreme-angular/ui/tree-view';
import { DxDrawerModule, DxDrawerTypes } from 'devextreme-angular/ui/drawer';
import { DxScrollViewModule, DxScrollViewComponent } from 'devextreme-angular/ui/scroll-view';
import { DxToolbarModule, DxToolbarTypes } from 'devextreme-angular/ui/toolbar';
import { HeaderComponent } from '../../components/header/header.component'; // Actualiza con la ruta correcta
import { SideNavigationMenuComponent } from '../../components/side-navigation-menu/side-navigation-menu.component'; // Actualiza con la ruta correcta
import { ScreenService } from '../../../core/services/screen.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-side-nav-inner-toolbar',
  templateUrl: './side-nav-inner-toolbar.component.html',
  styleUrls: ['./side-nav-inner-toolbar.component.scss'],
  standalone: true,
  imports: [
    DxDrawerModule,
    DxScrollViewModule,
    DxToolbarModule,
    CommonModule,
    HeaderComponent,
    SideNavigationMenuComponent
  ]
})
export class SideNavInnerToolbarComponent implements OnInit {
  @ViewChild(DxScrollViewComponent, { static: true }) scrollView!: DxScrollViewComponent;

  menuOpened = false;
  temporaryMenuOpened = false;

  @Input()
  title!: string;

  menuMode: DxDrawerTypes.OpenedStateMode = 'shrink';
  menuRevealMode: DxDrawerTypes.RevealMode = 'expand';
  minMenuSize = 0;
  shaderEnabled = false;
  swatchClassName = 'dx-swatch-additional';

  constructor(protected themeService: ThemeService, private screen: ScreenService) {
    themeService.isDark.subscribe((isDark) => {
      this.swatchClassName = 'dx-swatch-additional' + (isDark ? '-dark' : '');
    })
  }

  ngOnInit() {
    this.screen.changed.subscribe(() => this.updateDrawer());
    this.updateDrawer();
  }

  updateDrawer() {
    const isXSmall = this.screen.sizes['screen-x-small'];
    const isBelow992 = window.innerWidth < 992;

    this.menuMode = isBelow992 ? 'overlap' : 'shrink';
    this.menuRevealMode = isXSmall ? 'slide' : 'expand';
    this.minMenuSize = isXSmall ? 0 : 60;
    this.shaderEnabled = isBelow992;
    this.menuOpened = !isBelow992;
  }

  toggleMenu = (e: DxToolbarTypes.ItemClickEvent) => {
    this.menuOpened = !this.menuOpened;
    e.event?.stopPropagation();
  }

  get hideMenuAfterNavigation() {
    return this.menuMode === 'overlap' || this.temporaryMenuOpened;
  }

  get showMenuAfterClick() {
    return !this.menuOpened;
  }

  navigationChanged(event: DxTreeViewTypes.ItemClickEvent) {
    if (this.hideMenuAfterNavigation) {
      this.temporaryMenuOpened = false;
      this.menuOpened = false;
      event.event?.stopPropagation();
    }
    this.scrollView.instance.scrollTo(0);
  }

  navigationClick() {
    if (this.showMenuAfterClick) {
      this.temporaryMenuOpened = true;
      this.menuOpened = true;
    }
  }
}
