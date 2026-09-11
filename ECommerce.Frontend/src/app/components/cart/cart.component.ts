import { Component, OnInit } from '@angular/core';
import { CartService } from '../../services/cart.service';
import { Cart } from '../../models/api.models';

@Component({ selector: 'app-cart', template: `
  <section class="content-width page-section"><div class="section-heading"><div><span class="eyebrow">حقيبتك</span><h1>سلة التسوق</h1></div><span>{{ cart?.items?.length || 0 }} منتجات</span></div><div class="cart-layout" *ngIf="cart?.items?.length; else empty">
  <div class="card cart-items"><article *ngFor="let item of cart.items" class="cart-row"><div class="cart-icon">✦</div><div class="cart-product"><h3>{{ item.productName }}</h3><small>{{ item.variantSummary || 'المنتج الأساسي' }}</small></div><div class="quantity"><button (click)="update(item.productId, item.quantity - 1)" [disabled]="item.quantity <= 1">−</button><span>{{ item.quantity }}</span><button (click)="update(item.productId, item.quantity + 1)">+</button></div><strong>{{ item.lineTotal | currency:'USD' }}</strong><button class="remove" (click)="remove(item.productId)">×</button></article><div class="coupon"><input [(ngModel)]="coupon" placeholder="رمز الخصم" /><button class="outline" (click)="applyCoupon()">تطبيق</button><button class="text-button" *ngIf="cart.couponCode" (click)="removeCoupon()">إزالة الكوبون</button></div></div>
  <aside class="card summary"><h2>ملخص الطلب</h2><div><span>المجموع الفرعي</span><b>{{ cart.subtotal | currency:'USD' }}</b></div><div><span>الخصم</span><b class="discount">-{{ cart.discount | currency:'USD' }}</b></div><div><span>الضريبة</span><b>{{ cart.tax | currency:'USD' }}</b></div><hr /><div class="total"><span>الإجمالي</span><b>{{ cart.total | currency:'USD' }}</b></div><a routerLink="/checkout" class="primary full">إتمام الطلب</a></aside></div><ng-template #empty><div class="card empty"><h2>السلة فارغة</h2><p>أضف منتجاتك المفضلة لتظهر هنا.</p><a routerLink="/" class="primary">تصفح المنتجات</a></div></ng-template></section>
`, standalone: false })
export class CartComponent implements OnInit {
  cart?: Cart; coupon = '';
  constructor(private cs: CartService) {}
  ngOnInit(): void { this.refresh(); }
  refresh(): void { this.cs.getCart().subscribe(r => this.cart = r.data || undefined); }
  update(id: number, quantity: number): void { this.cs.updateItem(id, quantity).subscribe(r => this.cart = r.data || undefined); }
  remove(id: number): void { this.cs.removeItem(id).subscribe(r => this.cart = r.data || undefined); }
  applyCoupon(): void { if (this.coupon) this.cs.applyCoupon(this.coupon).subscribe(r => this.cart = r.data || undefined); }
  removeCoupon(): void { this.cs.removeCoupon().subscribe(r => this.cart = r.data || undefined); }
}
