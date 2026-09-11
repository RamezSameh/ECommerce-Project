import { Component } from '@angular/core';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-checkout',
  template: `
    <div class="max-w-md mx-auto p-6 shadow rounded mt-10">
      <h2>Checkout</h2>
      <form (ngSubmit)="checkout()" class="flex flex-col gap-2">
        <input [(ngModel)]="address" name="address" placeholder="Address" class="border p-2 rounded" />
        <button class="bg-green-600 text-white p-2 rounded">Place Order (COD)</button>
      </form>
    </div>
  `
})
export class CheckoutComponent {
  address = '';
  constructor(private os: OrderService) {}
  checkout() {
    this.os.checkout({ shippingAddress: this.address, paymentMethod: 0 }).subscribe(r => {
      alert('Order placed! ID: ' + (r as any)?.data?.id);
      window.location.href = '/orders';
    });
  }
}
