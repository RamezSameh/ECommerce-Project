import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';

@Component({
  selector: 'app-root',
  template: `
    <header class="topbar">
      <a routerLink="/" class="brand"><span class="brand-mark">E</span> Market</a>
      <form class="search" (ngSubmit)="search()"><input [(ngModel)]="searchTerm" name="searchTerm" placeholder="ابحث عن منتج..." /><button>⌕</button></form>
      <nav class="nav-links">
        <a routerLink="/">المتجر</a><a routerLink="/wishlist" *ngIf="auth.isAuthenticated()">المفضلة</a>
        <a routerLink="/orders" *ngIf="auth.isAuthenticated()">طلباتي</a><a routerLink="/admin" *ngIf="auth.hasRole('Admin')">الإدارة</a>
        <a routerLink="/cart" class="cart-link">السلة <b>{{ cartCount }}</b></a>
        <a routerLink="/login" *ngIf="!auth.isAuthenticated()">دخول</a>
        <button class="link-button" (click)="logout()" *ngIf="auth.isAuthenticated()">خروج</button>
      </nav>
    </header>
    <main><router-outlet></router-outlet></main>
    <footer>© 2026 Market · تسوق بثقة، توصيل سريع</footer>
  `,
  standalone: false
})
export class AppComponent {
  searchTerm = '';
  cartCount = 0;
  constructor(public auth: AuthService, private cart: CartService, private router: Router) {
    this.cart.getCart().subscribe(response => this.cartCount = response.data?.items.reduce((sum, item) => sum + item.quantity, 0) || 0);
  }
  search(): void { this.router.navigate(['/'], { queryParams: { search: this.searchTerm } }); }
  logout(): void { this.auth.logout(); }
}
