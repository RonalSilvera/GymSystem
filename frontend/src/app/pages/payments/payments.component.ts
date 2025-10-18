import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { ServicesService, Client } from '../../core/services/services.service';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, DxButtonModule],
  templateUrl: './payments.component.html',
  styleUrls: ['./payments.component.scss']
})
export class PaymentsComponent implements OnInit {
  client?: Client;

  constructor(private route: ActivatedRoute, private services: ServicesService) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('clientId'));
    if (id) {
      this.services.getClientById(id).subscribe(c => (this.client = c));
    }
  }

  pay(): void {
    this.services.notify('Membresía pagada', 'success');
  }
}
