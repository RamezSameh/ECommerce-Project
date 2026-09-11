import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';
import { Product, Review, PagedResult } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({ selector: 'app-product-detail', template: `
  <section class="detail-page" *ngIf="product"><div class="detail-image"><img *ngIf="mainImage" [src]="mainImage" [alt]="product.name" /><span *ngIf="!mainImage">✦</span></div>
  <div class="detail-info"><small>{{ product.brand || 'منتج مميز' }}</small><h1>{{ product.name }}</h1><div class="rating">★ {{ product.averageRating | number:'1.1-1' }} <span>{{ product.reviewCount }} تقييم</span></div><strong class="detail-price">{{ selectedVariant?.price || product.price | currency:'USD' }}</strong><p>{{ product.description }}</p><div class="variants" *ngIf="product.variants.length"><button *ngFor="let v of product.variants" [class.selected]="selectedVariant === v" (click)="selectedVariant = v" [disabled]="v.stock === 0">{{ v.name }}: {{ v.value }}</button></div><div class="purchase"><input type="number" min="1" [(ngModel)]="quantity" /><button class="primary" (click)="add()" [disabled]="!product.stock">أضف للسلة</button><button class="outline" (click)="wishlist()">♡</button></div></div></section>
  <section class="reviews content-width"><div class="section-heading"><h2>آراء العملاء</h2></div><article class="review card" *ngFor="let review of reviews"><b>{{ review.userName }}</b><span class="rating">★ {{ review.rating }}</span><p>{{ review.comment }}</p></article><form class="card review-form" *ngIf="auth.isAuthenticated()" (ngSubmit)="submitReview()"><h3>أضف تقييمك</h3><select [(ngModel)]="rating" name="rating"><option [ngValue]="5">5 نجوم</option><option [ngValue]="4">4 نجوم</option><option [ngValue]="3">3 نجوم</option><option [ngValue]="2">نجمتان</option><option [ngValue]="1">نجمة واحدة</option></select><textarea [(ngModel)]="comment" name="comment" placeholder="اكتب رأيك"></textarea><button class="primary">إرسال التقييم</button></form></section>
`, standalone: false })
export class ProductDetailComponent implements OnInit {
  product?: Product; reviews: Review[] = []; selectedVariant?: Product['variants'][number]; quantity = 1; rating = 5; comment = ''; mainImage = '';
  constructor(private route: ActivatedRoute, private ps: ProductService, private cs: CartService, public auth: AuthService, private notifications: NotificationService) {}
  ngOnInit(): void { const id = Number(this.route.snapshot.paramMap.get('id')); this.ps.getById(id).subscribe(r => { this.product = r.data || undefined; this.mainImage = this.product?.images.find(i => i.isMain)?.url || this.product?.images[0]?.url || ''; }); this.ps.getReviews(id).subscribe(r => this.reviews = (r.data as PagedResult<Review> | null)?.items || []); }
  add(): void { if (this.product) this.cs.addItem({ productId: this.product.id, variantId: this.selectedVariant?.id, quantity: this.quantity }).subscribe({ next: () => this.notifications.show(`تمت إضافة «${this.product?.name}» إلى السلة`), error: () => this.notifications.show('تعذرت إضافة المنتج إلى السلة', 'error') }); }
  wishlist(): void { if (this.product) this.ps.addToWishlist(this.product.id, this.auth.getHeaders()).subscribe({ next: () => this.notifications.show('تمت إضافة المنتج إلى المفضلة'), error: () => this.notifications.show('المنتج موجود بالفعل في المفضلة', 'info') }); }
  submitReview(): void { if (this.product) this.ps.addReview(this.product.id, { rating: this.rating, comment: this.comment }, this.auth.getHeaders()).subscribe(r => { if (r.data) this.reviews.unshift(r.data); this.comment = ''; }); }
}
