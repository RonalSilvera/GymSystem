
import {
  Component,
  Output,
  Input,
  EventEmitter,
  ViewChild,
  ElementRef,
  AfterViewInit,
  OnDestroy,
  OnInit
} from '@angular/core';

import {
  DxTreeViewModule,
  DxTreeViewComponent,
  DxTreeViewTypes
} from 'devextreme-angular/ui/tree-view';
import * as events from 'devextreme/events';
import { RouterModule } from '@angular/router';

interface NavigationItem {
  text: string;
  path: string;
  icon?: string;
  expanded?: boolean;
}

@Component({
  selector: 'app-side-navigation-menu',
  templateUrl: './side-navigation-menu.component.html',
  styleUrls: ['./side-navigation-menu.component.scss'],
  standalone: true,
  imports: [DxTreeViewModule, RouterModule],
  host: {
    '[class.compact]': 'compactMode'  
  }
})
export class SideNavigationMenuComponent implements OnInit, AfterViewInit, OnDestroy {

  navigation: NavigationItem[] = [
    { text: 'Dashboard', path: '/dashboard', icon: 'home' },
    { text: 'Clients', path: '/clients', icon: 'group' },
  ];

  filteredItems: NavigationItem[] = [];

  @ViewChild(DxTreeViewComponent, { static: true })
  menu!: DxTreeViewComponent;

  @Input()
  selectedItem?: string;

  @Output()
  selectedItemChanged = new EventEmitter<DxTreeViewTypes.ItemClickEvent>();

  @Output()
  openMenu = new EventEmitter<any>();

  private _items!: NavigationItem[];
  get items(): NavigationItem[] {
    if (!this._items) {
      this._items = this.navigation.map((item) => {
        if (item.path && !item.path.startsWith('/')) {
          item.path = `/${item.path}`;
        }
        return { ...item, expanded: !this._compactMode };
      });
    }
    return this._items;
  }

  private _compactMode = false;
  @Input()
  get compactMode() {
    return this._compactMode;
  }
  set compactMode(val) {
    this._compactMode = val;

    if (!this.menu?.instance) return;

    if (val) {
      this.menu.instance.collapseAll();
    } else {
      this.menu.instance.expandAll();
    }
  }

  constructor(private elementRef: ElementRef) {}

  ngOnInit(): void {
    this.filteredItems = [...this.items];  // Asigna los elementos del menú recibidos
  }

  onSearchChange(event: any) {
    const searchTerm = event.target.value.toLowerCase();
    this.filteredItems = this.items.filter((item: NavigationItem) =>
      item.text.toLowerCase().includes(searchTerm)
    );
  }

  onItemClick(event: DxTreeViewTypes.ItemClickEvent) {
    this.selectedItemChanged.emit(event);
  }

  ngAfterViewInit() {
    events.on(this.elementRef.nativeElement, 'dxclick', (e: Event) => {
      this.openMenu.next(e);
    });
  }

  ngOnDestroy() {
    events.off(this.elementRef.nativeElement, 'dxclick');
  }

  expandIfCompact() {
  if (this.compactMode) {
    this.compactMode = false;

    // ⚠️ IMPORTANTE: notificar al padre si lo controla externamente
    this.openMenu.emit(); 
  }
}
}

