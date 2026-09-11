import { Component } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-product-list',
  template: `
    <div class="p-4">
      <h2 class="text-2xl font-bold mb-4">Products</h2>
      <div *ngFor="let p of products" class="border p-3 mb-2 rounded shadow">
        <h3>{{ p.name }}</h3>
        <p>{{ p.price | currency }}</p>
        <button (click)="view(p.id)" class="bg-blue-500 text-white px-2 py-1 rounded">View</button>
      </div>
    </div>
  `
})
export class ProductListComponent {
  products: any[] = [];

  constructor(private ps: ProductService) {
    this.ps.getAll().subscribe(r => this.products = (r as any)?.data?.items || []);
  }

  view(id: number) {
    window.open(`${environment.apiUrl}/products/${id}`, '_blank');
  }
}
