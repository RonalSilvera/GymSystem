import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth', pathMatch: 'full' },
  { path: 'auth', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [AuthGuard] },
  { path: 'clients', loadComponent: () => import('./pages/clients/clients.component').then(m => m.ClientsComponent), canActivate: [AuthGuard] },
  { path: 'memberships/manage', loadComponent: () => import('./pages/memberships-manage/memberships-manage.component').then(m => m.MembershipsManageComponent), canActivate: [AuthGuard] },
  { path: 'access/validate', loadComponent: () => import('./pages/access-validate/access-validate.component').then(m => m.AccessValidateComponent), canActivate: [AuthGuard] },
  { path: 'payments', loadComponent: () => import('./pages/payments/payments.component').then(m => m.PaymentsComponent), canActivate: [AuthGuard] },
  { path: 'membership-types', loadComponent: () => import('./pages/membership-types/membership-types.component').then(m => m.MembershipTypesComponent), canActivate: [AuthGuard] },
  { path: 'payment-methods', loadComponent: () => import('./pages/payment-methods/payment-methods.component').then(m => m.PaymentMethodsComponent), canActivate: [AuthGuard] },
  { path: 'coupons', loadComponent: () => import('./pages/coupons/coupons.component').then(m => m.CouponsComponent), canActivate: [AuthGuard] },
  { path: 'reports', loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent), canActivate: [AuthGuard] },
  { path: 'profile', loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent), canActivate: [AuthGuard] },
  { path: '**', redirectTo: 'auth' }
];
