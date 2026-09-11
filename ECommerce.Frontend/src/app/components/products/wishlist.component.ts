import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { ProductSummary } from '../../models/api.models';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({ selector: 'app-wishlist', template: `
  <section class="content-width page-section"><div class="section-heading"><div><span class="eyebrow">اختياراتك</span><h1>المفضلة</h1></div></div><div class="product-grid"><article class="product-card" *ngFor="let product of products"><button class="image-box" (click)="open(product.id)"><span>✦</span></button><div class="product-body"><h3>{{ product.name }}</h3><div class="product-footer"><strong>{{ product.price | currency:'USD' }}</strong><button class="text-button" (click)="remove(product.id)">إزالة</button></div></div></article></div><div class="empty card" *ngIf="!products.length">لم تضف أي منتجات للمفضلة بعد.</div></section>
`, standalone: false })
export class WishlistComponent implements OnInit {
  products: ProductSummary[] = [];
  constructor(private ps: ProductService, private auth: AuthService, private router: Router) {}
  ngOnInit(): void { this.refresh(); }
  refresh(): void { this.ps.getWishlist(this.auth.getHeaders()).subscribe(r => this.products = r.data || []); }
  remove(id: number): void { this.ps.removeFromWishlist(id, this.auth.getHeaders()).subscribe(() => this.refresh()); }
  open(id: number): void { this.router.navigate(['/products', id]); }
}
