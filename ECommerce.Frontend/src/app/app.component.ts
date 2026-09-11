import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';
import { NotificationService, AppNotification } from './services/notification.service';

@Component({
  selector: 'app-root',
  template: `
    <header class="topbar">
      <a routerLink="/" class="brand"><span class="brand-mark">E</span> Market</a>
      <form class="search" (ngSubmit)="search()"><input [(ngModel)]="searchTerm" name="searchTerm" placeholder="ابحث عن منتج..." /><button>⌕</button></form>
      <nav class="nav-links">
        <a routerLink="/">المتجر</a><a routerLink="/wishlist" *ngIf="auth.isAuthenticated()">♡ المفضلة</a>
        <a routerLink="/orders" *ngIf="auth.isAuthenticated()">طلباتي</a><a routerLink="/admin" *ngIf="auth.hasRole('Admin')">الإدارة</a>
        <a routerLink="/cart" class="cart-link">السلة <b>{{ cartCount }}</b></a>
        <a routerLink="/profile" class="profile-link" *ngIf="auth.isAuthenticated()">حسابي</a><a routerLink="/login" *ngIf="!auth.isAuthenticated()">دخول</a>
        <button class="link-button" (click)="logout()" *ngIf="auth.isAuthenticated()">خروج</button>
      </nav>
    </header>
    <section class="notifications" aria-live="polite"><button *ngFor="let item of notifications" class="toast" [class.error]="item.type === 'error'" (click)="dismiss(item.id)"><span>{{ item.type === 'success' ? '✓' : '!' }}</span>{{ item.message }}<b>×</b></button></section>
    <main><router-outlet></router-outlet></main>
    <footer>© 2026 Market · تسوق بثقة، توصيل سريع</footer>
  `,
  standalone: false
})
export class AppComponent {
  searchTerm = '';
  cartCount = 0;
  notifications: AppNotification[] = [];
  constructor(public auth: AuthService, private cart: CartService, private router: Router, private notificationService: NotificationService) {
    this.cart.getCart().subscribe(response => this.cartCount = response.data?.items.reduce((sum, item) => sum + item.quantity, 0) || 0);
    this.notificationService.notifications$.subscribe(items => this.notifications = items);
  }
  search(): void { this.router.navigate(['/'], { queryParams: { search: this.searchTerm } }); }
  logout(): void { this.auth.logout(); }
  dismiss(id: number): void { this.notificationService.dismiss(id); }
}
