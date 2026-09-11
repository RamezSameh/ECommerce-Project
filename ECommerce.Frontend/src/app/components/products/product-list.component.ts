import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { Category, ProductSummary, PagedResult } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-product-list',
  template: `
    <section class="hero"><div><span class="eyebrow">متجر إلكتروني متكامل</span><h1>اكتشف ما يناسبك</h1><p>منتجات مختارة، أسعار واضحة وتجربة شراء سهلة من أول نقرة.</p><button class="primary" (click)="scrollToProducts()">تسوق الآن</button></div><div class="hero-art">✦</div></section>
    <section class="catalog" id="products">
      <aside class="filters card">
        <h3>تصفية النتائج</h3><label>بحث<input [(ngModel)]="search" (ngModelChange)="load(1)" placeholder="اسم المنتج أو العلامة" /></label>
        <label>التصنيف<select [(ngModel)]="categoryId" (ngModelChange)="load(1)"><option [ngValue]="undefined">كل التصنيفات</option><option *ngFor="let c of categories" [ngValue]="c.id">{{ c.name }}</option></select></label>
        <div class="two-columns"><label>من<input type="number" [(ngModel)]="minPrice" (change)="load(1)" /></label><label>إلى<input type="number" [(ngModel)]="maxPrice" (change)="load(1)" /></label></div>
        <label>الترتيب<select [(ngModel)]="sortBy" (ngModelChange)="load(1)"><option value="">الأحدث</option><option value="price_asc">الأقل سعراً</option><option value="price_desc">الأعلى سعراً</option><option value="popularity">الأكثر شعبية</option></select></label>
      </aside>
      <div class="results"><div class="section-heading"><div><span class="eyebrow">المنتجات</span><h2>كل المنتجات</h2></div><span>{{ totalCount }} منتج</span></div>
        <div class="product-grid"><article class="product-card" *ngFor="let p of products"><button class="image-box" (click)="open(p.id)"><img *ngIf="p.imageUrl" [src]="p.imageUrl" [alt]="p.name" /><span *ngIf="!p.imageUrl">✦</span></button><div class="product-body"><small>{{ p.brand || 'اختيار المحررين' }}</small><h3>{{ p.name }}</h3><div class="rating">★ {{ p.averageRating | number:'1.1-1' }} <span>({{ p.reviewCount }})</span></div><div class="product-footer"><strong>{{ p.price | currency:'USD' }}</strong><button class="add-button" [disabled]="p.stock === 0" (click)="add(p)">{{ p.stock ? 'أضف للسلة' : 'نفد المخزون' }}</button></div></div></article></div>
        <div class="empty card" *ngIf="!loading && !products.length">لا توجد منتجات مطابقة للفلاتر.</div>
        <div class="pager" *ngIf="totalPages > 1"><button (click)="load(page - 1)" [disabled]="page === 1">السابق</button><span>صفحة {{ page }} من {{ totalPages }}</span><button (click)="load(page + 1)" [disabled]="page === totalPages">التالي</button></div>
      </div>
    </section>
  `,
  standalone: false
})
export class ProductListComponent implements OnInit {
  products: ProductSummary[] = []; categories: Category[] = []; search = ''; categoryId?: number; minPrice?: number; maxPrice?: number; sortBy = ''; page = 1; totalPages = 0; totalCount = 0; loading = false;
  constructor(private ps: ProductService, private cs: CartService, private router: Router, private route: ActivatedRoute, private notifications: NotificationService) {}
  ngOnInit(): void { this.route.queryParams.subscribe(params => { this.search = params['search'] || ''; this.load(1); }); this.ps.getCategories().subscribe(r => this.categories = r.data || []); }
  load(page: number): void { this.loading = true; this.page = page; this.ps.getAll({ search: this.search, categoryId: this.categoryId, minPrice: this.minPrice, maxPrice: this.maxPrice, sortBy: this.sortBy, page }).subscribe(r => { const data = r.data as PagedResult<ProductSummary> | null; this.products = data?.items || []; this.totalPages = data?.totalPages || 0; this.totalCount = data?.totalCount || 0; this.loading = false; }); }
  add(product: ProductSummary): void { this.cs.addItem({ productId: product.id, quantity: 1 }).subscribe({ next: () => this.notifications.show(`تمت إضافة «${product.name}» إلى السلة`), error: () => this.notifications.show('تعذرت إضافة المنتج إلى السلة', 'error') }); }
  open(id: number): void { this.router.navigate(['/products', id]); }
  scrollToProducts(): void { document.getElementById('products')?.scrollIntoView({ behavior: 'smooth' }); }
}
