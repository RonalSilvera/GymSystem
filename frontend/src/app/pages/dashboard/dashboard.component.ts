import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { DxChartModule, DxLoadPanelModule, DxPieChartModule } from 'devextreme-angular';
import { firstValueFrom } from 'rxjs';
import {
  DashboardSummary,
  MembershipTypeDistribution,
  MonthlyMetric,
  ServicesService
} from '../../core/services/services.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule, DxChartModule, DxPieChartModule, DxLoadPanelModule]
})
export class DashboardComponent implements OnInit {
  loading = false;
  summary: DashboardSummary = this.createEmptySummary();
  monthlySeries: MonthlyMetric[] = [...this.summary.monthlyNewClients];
  membershipDistribution: MembershipTypeDistribution[] = [];
  pieHasData = false;
  readonly lineSeries: any = [{ argumentField: 'label', valueField: 'count', type: 'spline', color: '#4C6FFF' }];
  readonly lineTooltipOptions: any = {
    enabled: true,
    customizeTooltip: (info: any) => ({
      text: `${info.argumentText}: ${info.valueText} clientes`
    })
  };
  readonly pieSeries: any = [{ argumentField: 'name', valueField: 'count', innerRadius: 0.55 }];
  readonly pieLegend: any = {
    visible: true,
    orientation: 'horizontal',
    itemTextPosition: 'right',
    columnCount: 1
  };
  readonly pieTooltipOptions: any = {
    enabled: true,
    customizeTooltip: (info: any) => ({
      text: `${info.argumentText}: ${info.valueText} membresías`
    })
  };

  constructor(private services: ServicesService) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  private async loadSummary(): Promise<void> {
    this.loading = true;
    try {
      const summary = await firstValueFrom(this.services.getDashboardSummary());
      this.applySummary(summary);
    } catch {
      this.applySummary(this.createEmptySummary());
    } finally {
      this.loading = false;
    }
  }

  private applySummary(summary: DashboardSummary): void {
    const monthly = summary.monthlyNewClients?.length
      ? summary.monthlyNewClients
      : this.createEmptyMonthlySeries();
    const membership = summary.membershipTypeDistribution ?? [];
    this.summary = {
      ...summary,
      monthlyNewClients: monthly,
      membershipTypeDistribution: membership
    };
    this.monthlySeries = [...monthly];
    this.membershipDistribution = [...membership];
    this.pieHasData = this.membershipDistribution.some(item => item.count > 0);
  }

  private createEmptySummary(): DashboardSummary {
    return {
      totalClients: 0,
      activeClients: 0,
      expiringMemberships: 0,
      todaysAccesses: 0,
      monthlyNewClients: this.createEmptyMonthlySeries(),
      membershipTypeDistribution: []
    };
  }

  private createEmptyMonthlySeries(): MonthlyMetric[] {
    const now = new Date();
    const series: MonthlyMetric[] = [];
    for (let offset = 5; offset >= 0; offset--) {
      const date = new Date(now.getFullYear(), now.getMonth() - offset, 1);
      series.push({
        year: date.getFullYear(),
        month: date.getMonth() + 1,
        label: this.formatMonthLabel(date),
        count: 0
      });
    }
    return series;
  }

  private formatMonthLabel(date: Date): string {
    const label = date.toLocaleDateString('es-ES', { month: 'long' });
    return label.charAt(0).toUpperCase() + label.slice(1);
  }
}
