import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { Cart, PaymentMethod } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({ selector: 'app-checkout', template: `
  <section class="content-width page-section"><div class="section-heading"><div><span class="eyebrow">الخطوة الأخيرة</span><h1>إتمام الطلب</h1></div></div>
  <div class="cart-layout" *ngIf="cart?.items?.length; else empty">
  <div>
  <form class="card checkout-form" (ngSubmit)="submit()"><label>عنوان الشحن<textarea [(ngModel)]="address" name="address" required placeholder="العنوان بالتفصيل"></textarea></label><label>رقم الهاتف<input [(ngModel)]="phone" name="phone" required /></label><label>ملاحظات الطلب<textarea [(ngModel)]="notes" name="notes" placeholder="ملاحظات اختيارية"></textarea></label><h3>طريقة الدفع</h3><div class="payment-options"><label><input type="radio" [(ngModel)]="paymentMethod" name="payment" [value]="PaymentMethod.CashOnDelivery" /> الدفع عند الاستلام</label><label><input type="radio" [(ngModel)]="paymentMethod" name="payment" [value]="PaymentMethod.Card" /> بطاقة بنكية (Stripe)</label></div><button class="primary full" [disabled]="busy">تأكيد الطلب · {{ cart.total | currency:'USD' }}</button><p class="error" *ngIf="message">{{ message }}</p></form>
  </div>
  <aside class="card summary"><h2>طلبك</h2><article *ngFor="let item of cart.items" class="cart-row"><div class="cart-product"><h3>{{ item.productName }}</h3><small>{{ item.quantity }} × {{ item.unitPrice | currency:'USD' }}</small></div><strong>{{ item.lineTotal | currency:'USD' }}</strong></article><div class="coupon"><input [(ngModel)]="coupon" placeholder="رمز الخصم" /><button class="outline" (click)="applyCoupon()">تطبيق</button></div><button class="text-button" *ngIf="cart.couponCode" (click)="removeCoupon()">إزالة الكوبون ({{ cart.couponCode }})</button><hr /><div><span>المجموع الفرعي</span><b>{{ cart.subtotal | currency:'USD' }}</b></div><div><span>الخصم</span><b class="discount">-{{ cart.discount | currency:'USD' }}</b></div><div><span>الضريبة</span><b>{{ cart.tax | currency:'USD' }}</b></div><hr /><div class="total"><span>الإجمالي</span><b>{{ cart.total | currency:'USD' }}</b></div></aside>
  </div><ng-template #empty><div class="card empty"><h2>السلة فارغة</h2><p>أضف منتجاتك أولاً قبل إتمام الطلب.</p><a routerLink="/" class="primary">تصفح المنتجات</a></div></ng-template></section>
`, standalone: false })
export class CheckoutComponent implements OnInit {
  PaymentMethod = PaymentMethod; cart?: Cart;
  address = ''; phone = ''; notes = ''; paymentMethod = PaymentMethod.CashOnDelivery;
  coupon = ''; busy = false; message = '';
  constructor(private os: OrderService, private cs: CartService, private router: Router, private notifications: NotificationService) {}
  ngOnInit(): void { this.refresh(); }
  refresh(): void { this.cs.getCart().subscribe(r => { this.cart = r.data || undefined; if (this.cart && !this.cart.items.length) this.router.navigate(['/cart']); }); }
  applyCoupon(): void { if (this.coupon) this.cs.applyCoupon(this.coupon).subscribe({ next: r => { this.cart = r.data || undefined; this.notifications.show('تم تطبيق الكوبون'); }, error: e => this.notifications.show(e?.error?.message || 'الكوبون غير صالح', 'error') }); }
  removeCoupon(): void { this.cs.removeCoupon().subscribe(r => this.cart = r.data || undefined); }
  submit(): void {
    this.busy = true; this.message = '';
    this.os.checkout({ shippingAddress: this.address, phone: this.phone, notes: this.notes, paymentMethod: this.paymentMethod }).subscribe({
      next: r => {
        const result = r.data;
        if (result && 'paymentUrl' in result) window.location.href = (result as { paymentUrl: string }).paymentUrl;
        else this.router.navigate(['/orders']);
      },
      error: e => { this.busy = false; this.message = e?.error?.message || 'تعذر إنشاء الطلب'; }
    });
  }
}
