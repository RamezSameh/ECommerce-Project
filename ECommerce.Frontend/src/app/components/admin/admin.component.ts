import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  template: `
    <div class="p-4 max-w-4xl mx-auto">
      <h2>Admin Dashboard</h2>
      <a routerLink="/orders" class="text-blue-600">Orders</a>
      <p>Admin endpoints available at /api/admin/* — see backend Swagger.</p>
      <p class="text-sm text-gray-600">Use Bearer token from login to access admin resources.</p>
    </div>
  `
})
export class AdminComponent {}
