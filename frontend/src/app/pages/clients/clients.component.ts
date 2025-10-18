import { ChangeDetectionStrategy, Component, DestroyRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxDataGridComponent, DxDataGridModule, DxButtonModule, DxLoadPanelModule, DxTemplateModule, DxSelectBoxModule } from 'devextreme-angular';
import CustomStore from 'devextreme/data/custom_store';
import { ServicesService, Client } from '../../core/services/services.service';
import { Router } from '@angular/router';
import { OptionsPopupComponent } from '../../shared/templates/options-popup/options-popup.component';
import { Subject, EMPTY, Observable, interval, startWith } from 'rxjs';
import { switchMap, tap, catchError, finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ErrorPopupComponent } from '../../shared/templates/error-popup/error-popup.component';
import { SuccessPopupComponent } from '../../shared/templates/success-popup/success-popup.component';
import { ConfirmPopupComponent } from '../../shared/templates/confirm-popup/confirm-popup.component';
import { FormClientComponent } from '../../modules/form-client/components/form-client/form-client.component';
import { PaymentPopupComponent } from '../../shared/templates/payment-popup/payment-popup.component';
import { PartialPaymentPopupComponent } from '../../shared/templates/partial-payment-popup/partial-payment-popup.component';

@Component({
  selector: 'app-clients',
  standalone: true,
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.scss'],
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    DxLoadPanelModule,
    DxTemplateModule,
    DxSelectBoxModule,
    OptionsPopupComponent,
    ErrorPopupComponent,
    SuccessPopupComponent,
    ConfirmPopupComponent,
    FormClientComponent,
    PaymentPopupComponent,
    PartialPaymentPopupComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class ClientsComponent {
  @ViewChild(DxDataGridComponent, { static: true }) grid!: DxDataGridComponent;

  dataSource: CustomStore;
  private load$ = new Subject<{ loadOptions: any; resolve: (value: any) => void; reject: (reason: any) => void }>();
  loading = false;

  popupVisible = false;
  popupTitle = 'Nuevo Cliente';
  formData: Partial<Client> = {};
  totalClients = 0;
  activeClients = 0;
  inactiveClients = 0;

  statusOptions = [
    { value: null, text: 'Todos los estados' },
    { value: 1, text: 'Activo' },
    { value: 2, text: 'Inactivo' }
  ];
  selectedStatus: number | null = null;

  statusLabel = (row: Client) => (row.statusId === 1 ? 'Activo' : 'Inactivo');
  membershipStatusLabel = (row: Client) => {
    if (row.membershipStatusName) {
      return row.membershipStatusName;
    }
    switch (row.membershipStatusId) {
      case 1:
        return 'Vigente';
      case 3:
        return 'Vencido';
      case 5:
        return 'Pendiente de pago';
      case 6:
        return 'Pago parcial';
      case 7:
        return 'Pagado completamente';
      default:
        return 'Sin membresía';
    }
  };
  membershipTypeLabel = (row: Client) => row.membershipTypeName || 'Sin membresía';

  errorPopupVisible = false;
  errorTitle = '';
  errorMessage = '';
  successPopupVisible = false;
  successTitle = '';
  successMessage = '';
  confirmPopupVisible = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmButtonText = '';
  cancelButtonText = '';
  private selectedClient: Client | null = null;
  private pendingAction: 'disable' | 'enable' | null = null;
  optionsPopupVisible = false;
  optionsClient: Client | null = null;
  optionsTarget: any = null;
  extraOptions = [
    { text: 'Pagar ahora', action: 'payMembership', icon: 'money' },
    { text: 'Pago parcial', action: 'partialPayment', icon: 'percent' }
  ];
  paymentPopupVisible = false;
  paymentClient: Client | null = null;
  partialPaymentPopupVisible = false;
  partialPaymentClient: Client | null = null;

  constructor(private services: ServicesService, private destroyRef: DestroyRef, private cdr: ChangeDetectorRef, private router: Router) {
    this.dataSource = new CustomStore({
      key: 'clientId',
      load: (loadOptions) =>
        new Promise((resolve, reject) => {
          this.load$.next({ loadOptions, resolve, reject });
        })
    });

    this.load$
      .pipe(
        switchMap(({ loadOptions, resolve, reject }) => {
          this.loading = true;
          const page = loadOptions.skip ? loadOptions.skip / loadOptions.take + 1 : 1;
          const pageSize = loadOptions.take || 10;
          const sort = loadOptions.sort?.[0];
          const sortField = sort?.selector;
          const sortOrder = sort?.desc ? 'desc' : 'asc';
          let search = '';
          const filter = loadOptions.filter;
          if (Array.isArray(filter) && typeof filter[0] === 'string') {
            search = filter[2] || '';
          }
          return this.services.getClients(search, page, pageSize, sortField, sortOrder, this.selectedStatus).pipe(
            tap(res => {
              this.totalClients = res.total;
              this.activeClients = res.active;
              this.inactiveClients = res.inactive;
              resolve({ data: res.items, totalCount: res.total });
              this.cdr.markForCheck();
            }),
            catchError(err => {
              reject(err);
              return EMPTY;
            }),
            finalize(() => (this.loading = false))
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();

    interval(10000)
      .pipe(
        startWith(0),
        switchMap(() => this.services.getClientStats(this.selectedStatus)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(stats => {
        this.totalClients = stats.total;
        this.activeClients = stats.active;
        this.inactiveClients = stats.inactive;
        this.cdr.markForCheck();
      });
  }

  private showError(title: string, message: string): void {
    this.errorTitle = title;
    this.errorMessage = message;
    this.errorPopupVisible = true;
  }

  private showSuccess(title: string, message: string): void {
    this.successTitle = title;
    this.successMessage = message;
    this.successPopupVisible = true;
  }

  reload(): void {
    this.grid.instance.refresh();
  }

  onStatusChange(): void {
    this.reload();
  }

  openForm(data?: any): void {
    const client: Client | undefined = data?.row?.data ?? data;
    this.popupTitle = client ? 'Editar Cliente' : 'Nuevo Cliente';
    this.formData = client ? { ...client } : {};
    this.popupVisible = true;
  }

  onSave(dto: Partial<Client>): void {
    const isEdit = !!dto.clientId;
    const request = isEdit
      ? this.services.updateClient(dto.clientId!, dto)
      : this.services.createClient(dto);
    this.loading = true;
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.loading = false;
        this.popupVisible = false;
        const msg = isEdit
          ? 'Cliente actualizado correctamente'
          : 'Cliente añadido correctamente';
        this.showSuccess('Éxito', msg);
        this.reload();
      },
      error: err => {
        this.loading = false;
        const msg = err.status === 409 ? 'Datos duplicados' : 'Error al guardar';
        this.showError('Error', msg);
      }
    });
  }

  confirmStatusChange(data: any, action: 'disable' | 'enable'): void {
    const client: Client = data?.data || data?.row?.data || data;
    this.selectedClient = client;
    this.pendingAction = action;
    this.confirmTitle = 'Confirmar';
    if (action === 'disable') {
      this.confirmMessage = '¿Deshabilitar cliente?';
      this.confirmButtonText = 'Deshabilitar';
    } else {
      this.confirmMessage = '¿Habilitar cliente?';
      this.confirmButtonText = 'Habilitar';
    }
    this.cancelButtonText = 'Cancelar';
    this.confirmPopupVisible = true;
  }

  onConfirmAction(): void {
    if (!this.selectedClient || !this.pendingAction) {
      return;
    }
    this.loading = true;
    const request: Observable<any> =
      this.pendingAction === 'disable'
        ? this.services.deleteClient(this.selectedClient.clientId)
        : this.services.reactivateClient(this.selectedClient.clientId);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.loading = false;
        this.confirmPopupVisible = false;
        const msg =
          this.pendingAction === 'disable'
            ? 'Cliente deshabilitado'
            : 'Cliente habilitado';
        this.showSuccess('Éxito', msg);
        this.selectedClient = null;
        this.pendingAction = null;
        this.reload();
      },
      error: () => {
        this.loading = false;
        this.confirmPopupVisible = false;
        this.showError('Error', 'Operación no completada');
        this.selectedClient = null;
        this.pendingAction = null;
      }
    });
  }

  onCancelAction(): void {
    this.confirmPopupVisible = false;
    this.selectedClient = null;
    this.pendingAction = null;
  }

  openOptions(event: any, client: Client): void {
    this.optionsClient = client;
    this.optionsTarget = event.element;
    this.optionsPopupVisible = true;
  }

  onOptionSelect(action: string): void {
    this.optionsPopupVisible = false;
    if (action === 'payMembership' && this.optionsClient) {
      this.paymentClient = this.optionsClient;
      this.paymentPopupVisible = true;
    } else if (action === 'partialPayment' && this.optionsClient) {
      this.partialPaymentClient = this.optionsClient;
      this.partialPaymentPopupVisible = true;
    } else {
      this.showSuccess('Opción', `Acción: ${action}`);
    }
  }

  onPaymentSuccess(): void {
    this.paymentPopupVisible = false;
    this.paymentClient = null;
    this.showSuccess('Éxito', 'Pago registrado correctamente');
    this.reload();
  }

  onPaymentError(): void {
    this.paymentPopupVisible = false;
    this.paymentClient = null;
    this.showError('Error', 'No se pudo registrar el pago');
  }

  onPartialPaymentSuccess(): void {
    this.partialPaymentPopupVisible = false;
    this.partialPaymentClient = null;
    this.showSuccess('Éxito', 'Pago parcial registrado correctamente');
    this.reload();
  }

  onPartialPaymentError(): void {
    this.partialPaymentPopupVisible = false;
    this.partialPaymentClient = null;
    this.showError('Error', 'No se pudo registrar el pago parcial');
  }
}

