import { Component } from '@angular/core';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-orders',
  template: `
    <div class="p-4 max-w-4xl mx-auto">
      <h2>My Orders</h2>
      <div *ngFor="let o of orders" class="border p-3 mb-2 rounded shadow">
        <div>{{ o.orderNumber }} — {{ o.status }} — {{ o.total | currency }}</div>
        <button (click)="cancel(o.id)" class="text-red-600 text-sm">Cancel</button>
      </div>
    </div>
  `
})
export class OrdersComponent {
  orders: any[] = [];
  constructor(private os: OrderService) {
    this.os.myOrders().subscribe(r => this.orders = (r as any)?.data?.items || (r as any)?.items || []);
  }
  cancel(id: number) {
    this.os.cancel(id).subscribe(() => this.orders = this.orders.filter(o => o.id !== id));
  }
}
