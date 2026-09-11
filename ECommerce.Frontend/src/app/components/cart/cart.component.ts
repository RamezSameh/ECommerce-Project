import { Component } from '@angular/core';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-cart',
  template: `
    <div class="p-4 max-w-4xl mx-auto">
      <h2 class="text-2xl font-bold mb-4">Cart</h2>
      <table class="w-full border-collapse">
        <thead><tr class="bg-gray-100"><th>Name</th><th>Qty</th><th>Action</th></tr></thead>
        <tbody>
          <tr *ngFor="let item of cart?.items || []" class="border-b">
            <td>{{ item.productName }}</td>
            <td>{{ item.quantity }}</td>
            <td><button (click)="remove(item.productId)" class="text-red-600">Remove</button></td>
          </tr>
        </tbody>
      </table>
      <div class="mt-4 font-bold">Subtotal: {{ cart?.subtotal || 0 | currency }}</div>
      <a routerLink="/checkout" class="inline-block mt-4 bg-green-600 text-white px-4 py-2 rounded">Checkout</a>
    </div>
  `
})
export class CartComponent {
  cart: any = {};
  constructor(private cs: CartService) {
    this.cs.getCart().subscribe(r => this.cart = (r as any)?.data || r);
  }
  remove(productId: number) {
    this.cs.removeItem(productId).subscribe(r => this.cart = (r as any)?.data || r);
  }
}
