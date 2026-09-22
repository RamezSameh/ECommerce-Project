import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { AdminUser, Coupon, Order, OrderStatus, PagedResult, SalesReport } from '../../models/api.models';

@Component({ selector: 'app-admin', template: `
  <section class="content-width page-section"><div class="section-heading"><div><span class="eyebrow">لوحة التحكم</span><h1>إدارة المتجر</h1></div></div><div class="admin-tabs"><button [class.active]="tab==='overview'" (click)="tab='overview'; loadReport()">نظرة عامة</button><button [class.active]="tab==='orders'" (click)="tab='orders'; loadOrders()">الطلبات</button><button [class.active]="tab==='users'" (click)="tab='users'; loadUsers()">المستخدمون</button><button [class.active]="tab==='coupons'" (click)="tab='coupons'; loadCoupons()">الكوبونات</button></div>
  <div class="stats-grid" *ngIf="tab==='overview' && report"><div class="stat card"><small>الإيرادات</small><b>{{ report.totalRevenue | currency:'USD' }}</b></div><div class="stat card"><small>الطلبات</small><b>{{ report.totalOrders }}</b></div><div class="stat card"><small>المنتجات</small><b>{{ report.totalProducts }}</b></div><div class="stat card"><small>المستخدمون</small><b>{{ report.totalUsers }}</b></div></div>
  <div *ngIf="tab==='overview' && report" class="card table-card"><h2>أفضل المنتجات</h2><table><tr><th>المنتج</th><th>الوحدات</th><th>الإيرادات</th></tr><tr *ngFor="let p of report.topProducts"><td>{{ p.productName }}</td><td>{{ p.unitsSold }}</td><td>{{ p.revenue | currency:'USD' }}</td></tr></table></div>
  <div *ngIf="tab==='orders'" class="card table-card"><h2>إدارة الطلبات</h2><table><tr><th>الطلب</th><th>التاريخ</th><th>الإجمالي</th><th>الحالة</th></tr><tr *ngFor="let order of orders"><td>{{ order.orderNumber }}</td><td>{{ order.createdAt | date:'shortDate' }}</td><td>{{ order.total | currency:'USD' }}</td><td><select [ngModel]="order.status" (ngModelChange)="changeStatus(order, $event)"><option *ngFor="let status of statuses" [ngValue]="status.value">{{ status.label }}</option></select></td></tr></table></div>
  <div *ngIf="tab==='users'" class="card table-card"><div class="table-toolbar"><h2>المستخدمون</h2><input [(ngModel)]="userSearch" (keyup.enter)="loadUsers()" placeholder="بحث بالبريد..." /></div><table><tr><th>المستخدم</th><th>البريد</th><th>الأدوار</th><th>إجراء</th></tr><tr *ngFor="let user of users"><td>{{ user.fullName || user.userName }}</td><td>{{ user.email }}</td><td>{{ user.roles.join(', ') }}</td><td><button class="text-button" (click)="toggleBan(user)">{{ user.isBanned ? 'إلغاء الحظر' : 'حظر' }}</button><button class="text-button" (click)="promote(user)">Vendor</button><button class="text-button danger" (click)="deleteUser(user)">حذف</button></td></tr></table></div>
  <div *ngIf="tab==='coupons'" class="card table-card"><div class="table-toolbar"><h2>الكوبونات</h2><form (ngSubmit)="createCoupon()"><input [(ngModel)]="newCode" name="code" placeholder="رمز جديد" required /><input [(ngModel)]="newPercent" name="percent" type="number" placeholder="% خصم" /><button class="primary">إضافة</button></form></div><table><tr><th>الرمز</th><th>الخصم</th><th>الحالة</th><th>الاستخدام</th><th></th></tr><tr *ngFor="let coupon of coupons"><td>{{ coupon.code }}</td><td>{{ coupon.discountPercent || coupon.discountAmount }}{{ coupon.discountPercent ? '%' : '$' }}</td><td>{{ coupon.isActive ? 'فعال' : 'متوقف' }}</td><td>{{ coupon.usageCount }}</td><td><button class="text-button" (click)="toggleCoupon(coupon)">{{ coupon.isActive ? 'إيقاف' : 'تفعيل' }}</button></td></tr></table></div>
  </section>
`, standalone: false })
export class AdminComponent implements OnInit {
  tab = 'overview'; report?: SalesReport; orders: Order[] = []; users: AdminUser[] = []; coupons: Coupon[] = []; userSearch = ''; newCode = ''; newPercent?: number; OrderStatus = OrderStatus;
  statuses = [{ value: 0, label: 'قيد الانتظار' }, { value: 1, label: 'قيد التجهيز' }, { value: 2, label: 'تم الشحن' }, { value: 3, label: 'تم التسليم' }, { value: 4, label: 'ملغي' }];
  constructor(private admin: AdminService) {}
  ngOnInit(): void { this.loadReport(); }
  loadReport(): void { this.admin.sales().subscribe(r => this.report = r.data || undefined); }
  loadOrders(): void { this.admin.orders().subscribe(r => this.orders = (r.data as PagedResult<Order> | null)?.items || []); }
  loadUsers(): void { this.admin.users(this.userSearch).subscribe(r => this.users = (r.data as PagedResult<AdminUser> | null)?.items || []); }
  loadCoupons(): void { this.admin.coupons().subscribe(r => this.coupons = (r.data as PagedResult<Coupon> | null)?.items || []); }
  changeStatus(order: Order, status: number): void { this.admin.updateOrderStatus(order.id, status).subscribe(r => { if (r.data) order.statusName = r.data.statusName; }); }
  toggleBan(user: AdminUser): void { this.admin.ban(user.id, !user.isBanned).subscribe(() => user.isBanned = !user.isBanned); }
  promote(user: AdminUser): void { this.admin.promoteVendor(user.id).subscribe(() => user.roles.push('Vendor')); }
  createCoupon(): void { this.admin.createCoupon({ code: this.newCode, discountPercent: this.newPercent }).subscribe(r => { if (r.data) this.coupons.unshift(r.data); this.newCode = ''; this.newPercent = undefined; }); }
  toggleCoupon(coupon: Coupon): void { this.admin.toggleCoupon(coupon.id).subscribe(() => coupon.isActive = !coupon.isActive); }
  deleteUser(user: AdminUser): void { if (window.confirm('حذف هذا المستخدم نهائياً؟')) this.admin.deleteUser(user.id).subscribe(() => this.users = this.users.filter(u => u.id !== user.id)); }
}
