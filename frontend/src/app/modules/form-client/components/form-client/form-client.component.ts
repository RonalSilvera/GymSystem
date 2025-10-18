import { Component, EventEmitter, Input, Output, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxPopupModule, DxFormModule, DxButtonModule, DxFormComponent, DxSelectBoxModule } from 'devextreme-angular';
import { Client, MembershipType, ServicesService } from '../../../../core/services/services.service';

@Component({
  selector: 'app-form-client',
  standalone: true,
  imports: [CommonModule, DxPopupModule, DxFormModule, DxButtonModule, DxSelectBoxModule],
  templateUrl: './form-client.component.html',
  styleUrls: ['./form-client.component.scss']
})
export class FormClientComponent implements OnInit {
  @ViewChild(DxFormComponent) form!: DxFormComponent;
  @Input() visible = false;
  @Input() title = '';
  @Input() formData: Partial<Client> = {};
  @Output() saveForm = new EventEmitter<Partial<Client>>();
  @Output() cancel = new EventEmitter<void>();

  membershipTypes: MembershipType[] = [];

  constructor(private services: ServicesService) {}

  ngOnInit(): void {
    this.services.getMembershipTypes().subscribe((items: MembershipType[]) => (this.membershipTypes = items));
  }

  onSave(): void {
    const validation = this.form.instance.validate();
    if (!validation.isValid) {
      return;
    }
    if (this.formData.membershipTypeId != null) {
      this.formData.clearMembership = false;
    }
    this.saveForm.emit({ ...this.formData });
  }

  clearMembership(): void {
    this.formData.membershipTypeId = null;
    this.formData.clearMembership = true;
    this.formData.membershipStatusId = null;
    const editor = this.form?.instance.getEditor('membershipTypeId');
    editor?.option('value', null);
  }

  onMembershipChange = (e: { value: number | null }): void => {
    if (e.value != null) {
      this.formData.clearMembership = false;
      this.formData.membershipStatusId = null;
    }
  };
}
