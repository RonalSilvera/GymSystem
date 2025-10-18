import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule, DxFormModule, DxButtonModule, DxSelectBoxModule, DxLoadPanelModule, DxTextBoxModule, DxNumberBoxModule } from 'devextreme-angular';
import { Client, PaymentMethod, MembershipType, ServicesService } from '../../../core/services/services.service';

@Component({
  selector: 'app-partial-payment-popup',
  standalone: true,
  imports: [CommonModule, DxPopupModule, DxFormModule, DxButtonModule, DxSelectBoxModule, DxLoadPanelModule, DxTextBoxModule, DxNumberBoxModule],
  templateUrl: './partial-payment-popup.component.html',
  styleUrls: ['./partial-payment-popup.component.scss']
})
export class PartialPaymentPopupComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() client: Client | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();
  @Output() error = new EventEmitter<void>();

  formData: {
    clientName?: string;
    membershipTypeName?: string;
    membershipPrice?: number | null;
    amount?: number | null;
    paymentMethodId: number | null;
    referenceText?: string;
    amountPaid?: number | null;
    amountDue?: number | null;
    lastPaymentAmount?: number | null;
  } = {
    clientName: '',
    membershipTypeName: '',
    membershipPrice: null,
    amount: null,
    paymentMethodId: null,
    referenceText: '',
    amountPaid: null,
    amountDue: null,
    lastPaymentAmount: null
  };

  paymentMethods: PaymentMethod[] = [];
  membershipTypes: MembershipType[] = [];
  loading = false;
  amountEditorOptions: {
    format: string;
    min: number;
    showSpinButtons: boolean;
    placeholder: string;
    disabled: boolean;
    max?: number;
  } = {
    format: '$ #,##0',
    min: 0,
    showSpinButtons: false,
    placeholder: 'Ingrese el monto a abonar',
    disabled: false
  };

  constructor(private services: ServicesService) {}

  ngOnInit(): void {
    this.services.getPaymentMethods().subscribe(items => (this.paymentMethods = items));
    this.services.getMembershipTypes().subscribe(types => {
      this.membershipTypes = types;
      this.updateFormData();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['client']) {
      this.updateFormData();
    }
  }

  private updateFormData(): void {
    if (this.client) {
      this.formData.clientName = this.client.fullName;
      this.formData.membershipTypeName = this.client.membershipTypeName || '';
      const type = this.membershipTypes.find(mt => mt.id === this.client!.membershipTypeId);
      this.formData.membershipPrice = type?.price ?? null;
      this.formData.amount = null;
      this.formData.paymentMethodId = null;
      this.formData.referenceText = '';
      const amountPaid = this.client.amountPaid ?? 0;
      this.formData.amountPaid = amountPaid;
      const clientDue = this.client.amountDue ?? null;
      if (clientDue != null) {
        this.formData.amountDue = clientDue;
      } else if (this.formData.membershipPrice != null) {
        const price = this.formData.membershipPrice;
        const due = price - amountPaid;
        this.formData.amountDue = due > 0 ? due : 0;
      } else {
        this.formData.amountDue = null;
      }
      this.formData.lastPaymentAmount = this.client.lastPaymentAmount ?? null;
    } else {
      this.formData.clientName = '';
      this.formData.membershipTypeName = '';
      this.formData.membershipPrice = null;
      this.formData.amount = null;
      this.formData.paymentMethodId = null;
      this.formData.referenceText = '';
      this.formData.amountPaid = null;
      this.formData.amountDue = null;
      this.formData.lastPaymentAmount = null;
    }
    this.updateAmountOptions();
  }

  onPay(): void {
    if (!this.canSubmit) {
      return;
    }
    this.loading = true;
    this.services
      .payPartialMembership(
        this.client!.clientId,
        this.client!.membershipTypeId!,
        this.formData.paymentMethodId!,
        this.formData.amount!,
        this.formData.referenceText || undefined
      )
      .subscribe({
        next: () => {
          this.loading = false;
          this.success.emit();
        },
        error: () => {
          this.loading = false;
          this.error.emit();
        }
      });
  }

  onCancel(): void {
    this.close.emit();
  }

  formatCurrency(value: number | null | undefined): string {
    if (value == null || Number.isNaN(Number(value))) {
      return '$ 0';
    }
    const amount = Number(value);
    return `$ ${amount.toLocaleString('es-CO')}`;
  }

  get hasOutstanding(): boolean {
    const due = this.formData.amountDue;
    if (due != null) {
      return due > 0;
    }
    return !!this.formData.membershipPrice && this.formData.membershipPrice > 0;
  }

  get canSubmit(): boolean {
    if (this.loading || !this.client || !this.client.membershipTypeId) {
      return false;
    }
    const amount = this.formData.amount ?? 0;
    const max = this.maxAmount ?? Number.POSITIVE_INFINITY;
    return (
      this.hasOutstanding &&
      !!this.formData.paymentMethodId &&
      amount > 0 &&
      amount <= max
    );
  }

  private get maxAmount(): number | null {
    if (this.formData.amountDue != null) {
      return this.formData.amountDue;
    }
    return this.formData.membershipPrice ?? null;
  }

  private updateAmountOptions(): void {
    const options: typeof this.amountEditorOptions = {
      format: '$ #,##0',
      min: 0,
      showSpinButtons: false,
      placeholder: 'Ingrese el monto a abonar',
      disabled: !this.hasOutstanding
    };
    const max = this.maxAmount;
    if (max != null && max > 0) {
      options.max = max;
    }
    this.amountEditorOptions = options;
    if (options.disabled) {
      this.formData.amount = null;
      this.formData.paymentMethodId = null;
    }
  }
}
