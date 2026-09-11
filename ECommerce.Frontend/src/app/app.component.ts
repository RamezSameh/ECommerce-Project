import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <nav class="bg-gray-900 text-white p-4 flex gap-4 flex-wrap">
      <a routerLink="/" class="font-bold">ECommerce API Preview</a>
      <a routerLink="/cart">Cart</a>
      <a routerLink="/orders">Orders</a>
      <a routerLink="/checkout">Checkout</a>
      <a routerLink="/admin">Admin</a>
      <a routerLink="/login" class="ml-auto">Login</a>
    </nav>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {}
