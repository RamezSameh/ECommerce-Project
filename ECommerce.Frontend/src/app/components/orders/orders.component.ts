import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { Order, OrderStatus, PagedResult } from '../../models/api.models';

@Component({ selector: 'app-orders', template: `
  <section class="content-width page-section"><div class="section-heading"><div><span class="eyebrow">حسابي</span><h1>طلباتي</h1></div></div><div class="orders-list"><article class="card order-card" *ngFor="let order of orders"><div class="order-head"><div><small>{{ order.createdAt | date:'mediumDate' }}</small><h3>{{ order.orderNumber }}</h3></div><span class="status" [class.cancelled]="order.status === OrderStatus.Cancelled">{{ order.statusName }}</span><strong>{{ order.total | currency:'USD' }}</strong></div><div class="order-items">{{ order.items.length }} منتجات · {{ paymentName(order.paymentMethod) }}</div><div class="order-actions"><button class="outline" (click)="download(order.id)">تحميل الفاتورة</button><button class="text-button danger" *ngIf="order.status < OrderStatus.Shipped" (click)="cancel(order.id)">إلغاء الطلب</button></div></article><div class="empty card" *ngIf="!orders.length">لا توجد طلبات حتى الآن.</div></div></section>
`, standalone: false })
export class OrdersComponent implements OnInit {
  orders: Order[] = []; OrderStatus = OrderStatus;
  constructor(private os: OrderService) {}
  ngOnInit(): void { this.os.myOrders().subscribe(r => this.orders = (r.data as PagedResult<Order> | null)?.items || []); }
  cancel(id: number): void { this.os.cancel(id).subscribe(() => this.orders = this.orders.filter(order => order.id !== id)); }
  download(id: number): void { this.os.invoice(id).subscribe(blob => { const url = URL.createObjectURL(blob); const link = document.createElement('a'); link.href = url; link.download = `invoice-${id}.pdf`; link.click(); URL.revokeObjectURL(url); }); }
  paymentName(method: number): string { return method === 1 ? 'بطاقة' : 'الدفع عند الاستلام'; }
}
