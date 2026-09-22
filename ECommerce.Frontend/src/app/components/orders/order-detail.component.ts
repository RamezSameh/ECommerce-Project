import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order, OrderStatus } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({ selector: 'app-order-detail', template: `
  <section class="content-width page-section narrow" *ngIf="order"><div class="section-heading"><div><span class="eyebrow">تفاصيل الطلب</span><h1>{{ order.orderNumber }}</h1></div><span class="status" [class.cancelled]="order.status === OrderStatus.Cancelled">{{ order.statusName }}</span></div>
  <div class="card"><div class="order-head"><div><small>تاريخ الطلب</small><h3>{{ order.createdAt | date:'medium' }}</h3></div><div><small>طريقة الدفع</small><h3>{{ order.paymentMethod === 1 ? 'بطاقة بنكية' : 'الدفع عند الاستلام' }}</h3></div></div>
  <div class="order-items" *ngFor="let item of order.items">{{ item.quantity }} × {{ item.productName }} <span *ngIf="item.variantSummary">({{ item.variantSummary }})</span> — {{ item.lineTotal | currency:'USD' }}</div>
  <hr /><div class="summary"><div><span>المجموع الفرعي</span><b>{{ order.subtotal | currency:'USD' }}</b></div><div><span>الخصم</span><b class="discount">-{{ order.discount | currency:'USD' }}</b></div><div><span>الضريبة</span><b>{{ order.tax | currency:'USD' }}</b></div><div><span>الشحن</span><b>{{ order.shipping | currency:'USD' }}</b></div><hr /><div class="total"><span>الإجمالي</span><b>{{ order.total | currency:'USD' }}</b></div></div>
  <div class="order-actions"><button class="outline" (click)="download()">تحميل الفاتورة (PDF)</button><button class="text-button danger" *ngIf="order.status < OrderStatus.Shipped" (click)="cancel()">إلغاء الطلب</button></div></div>
  <a routerLink="/orders" class="text-button">← العودة إلى طلباتي</a></section>
`, standalone: false })
export class OrderDetailComponent implements OnInit {
  order?: Order; OrderStatus = OrderStatus;
  constructor(private route: ActivatedRoute, private os: OrderService, private router: Router, private notifications: NotificationService) {}
  ngOnInit(): void { const id = Number(this.route.snapshot.paramMap.get('id')); this.os.getById(id).subscribe({ next: r => this.order = r.data || undefined, error: () => this.router.navigate(['/orders']) }); }
  cancel(): void {
    if (!this.order) return;
    this.os.cancel(this.order.id).subscribe({
      next: r => { this.order = r.data || this.order; this.notifications.show('تم إلغاء الطلب'); },
      error: e => this.notifications.show(e?.error?.message || 'تعذر إلغاء الطلب', 'error')
    });
  }
  download(): void {
    if (!this.order) return;
    this.os.invoice(this.order.id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url; link.download = `invoice-${this.order!.id}.pdf`; link.click();
      URL.revokeObjectURL(url);
    });
  }
}
