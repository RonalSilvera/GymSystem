import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule, DxSelectBoxModule, DxLoadPanelModule, DxTextBoxModule } from 'devextreme-angular';
import { Client, PaymentMethod, MembershipType, ServicesService } from '../../../core/services/services.service';

interface PaymentFormData {
  clientName: string;
  membershipTypeName: string;
  price: number | null;
  paymentMethodId: number | null;
  referenceText: string;
}

@Component({
  selector: 'app-payment-popup',
  standalone: true,
  imports: [CommonModule, DxPopupModule, DxSelectBoxModule, DxLoadPanelModule, DxTextBoxModule],
  templateUrl: './payment-popup.component.html',
  styleUrls: ['./payment-popup.component.scss']
})
export class PaymentPopupComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() client: Client | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();
  @Output() error = new EventEmitter<void>();

  formData: PaymentFormData = {
    clientName: '',
    membershipTypeName: '',
    price: null,
    paymentMethodId: null,
    referenceText: ''
  };

  paymentMethods: PaymentMethod[] = [];
  membershipTypes: MembershipType[] = [];
  loading = false;

  private readonly priceFormatter = new Intl.NumberFormat('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

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

  get priceValue(): string {
    if (this.formData.price === null || this.formData.price === undefined) {
      return '--';
    }
    return this.priceFormatter.format(this.formData.price);
  }

  private updateFormData(): void {
    if (this.client) {
      this.formData = {
        clientName: this.client.fullName,
        membershipTypeName: this.client.membershipTypeName || '',
        price: this.membershipTypes.find(mt => mt.id === this.client!.membershipTypeId)?.price ?? null,
        paymentMethodId: null,
        referenceText: ''
      };
    } else {
      this.formData = {
        clientName: '',
        membershipTypeName: '',
        price: null,
        paymentMethodId: null,
        referenceText: ''
      };
    }
  }

  onPay(): void {
    if (!this.client || !this.client.membershipTypeId || !this.formData.paymentMethodId) {
      return;
    }
    this.loading = true;
    this.services
      .payMembership(
        this.client.clientId,
        this.client.membershipTypeId,
        this.formData.paymentMethodId,
        undefined,
        undefined,
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
}
