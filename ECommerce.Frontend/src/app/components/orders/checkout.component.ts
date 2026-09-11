import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { PaymentMethod } from '../../models/api.models';

@Component({ selector: 'app-checkout', template: `
  <section class="content-width page-section narrow"><div class="section-heading"><div><span class="eyebrow">الخطوة الأخيرة</span><h1>إتمام الطلب</h1></div></div><form class="card checkout-form" (ngSubmit)="submit()"><label>عنوان الشحن<textarea [(ngModel)]="address" name="address" required placeholder="العنوان بالتفصيل"></textarea></label><label>رقم الهاتف<input [(ngModel)]="phone" name="phone" required /></label><label>ملاحظات الطلب<textarea [(ngModel)]="notes" name="notes" placeholder="ملاحظات اختيارية"></textarea></label><h3>طريقة الدفع</h3><div class="payment-options"><label><input type="radio" [(ngModel)]="paymentMethod" name="payment" [value]="PaymentMethod.CashOnDelivery" /> الدفع عند الاستلام</label><label><input type="radio" [(ngModel)]="paymentMethod" name="payment" [value]="PaymentMethod.Card" /> بطاقة بنكية</label></div><button class="primary full" [disabled]="busy">تأكيد الطلب</button><p class="error" *ngIf="message">{{ message }}</p></form></section>
`, standalone: false })
export class CheckoutComponent {
  PaymentMethod = PaymentMethod; address = ''; phone = ''; notes = ''; paymentMethod = PaymentMethod.CashOnDelivery; busy = false; message = '';
  constructor(private os: OrderService, private router: Router) {}
  submit(): void { this.busy = true; this.os.checkout({ shippingAddress: this.address, phone: this.phone, notes: this.notes, paymentMethod: this.paymentMethod }).subscribe({ next: r => { const result = r.data; if (result && 'paymentUrl' in result) window.location.href = result.paymentUrl; else this.router.navigate(['/orders']); }, error: e => { this.busy = false; this.message = e?.error?.message || 'تعذر إنشاء الطلب'; } }); }
}
