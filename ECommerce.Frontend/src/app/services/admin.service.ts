import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse, AdminUser, Coupon, Order, PagedResult, ProductSummary, SalesReport } from '../models/api.models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private api = environment.apiUrl;
  constructor(private http: HttpClient, private auth: AuthService) {}
  private options = () => ({ headers: this.auth.getHeaders() });
  users(search = ''): Observable<ApiResponse<PagedResult<AdminUser>>> {
    return this.http.get<ApiResponse<PagedResult<AdminUser>>>(`${this.api}/admin/users`, { ...this.options(), params: new HttpParams().set('page', 1).set('pageSize', 20).set('search', search) });
  }
  ban(id: string, banned: boolean): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.api}/admin/users/${id}/${banned ? 'ban' : 'unban'}`, {}, this.options());
  }
  promoteVendor(id: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.api}/admin/users/${id}/promote-vendor`, {}, this.options());
  }
  deleteUser(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/admin/users/${id}`, this.options());
  }
  sales(): Observable<ApiResponse<SalesReport>> { return this.http.get<ApiResponse<SalesReport>>(`${this.api}/admin/reports/sales`, this.options()); }
  orders(status?: number): Observable<ApiResponse<PagedResult<Order>>> {
    let params = new HttpParams().set('page', 1).set('pageSize', 20);
    if (status !== undefined) params = params.set('status', status);
    return this.http.get<ApiResponse<PagedResult<Order>>>(`${this.api}/orders`, { ...this.options(), params });
  }
  updateOrderStatus(id: number, status: number): Observable<ApiResponse<Order>> {
    return this.http.put<ApiResponse<Order>>(`${this.api}/orders/${id}/status`, { status }, this.options());
  }
  coupons(): Observable<ApiResponse<PagedResult<Coupon>>> { return this.http.get<ApiResponse<PagedResult<Coupon>>>(`${this.api}/coupons`, this.options()); }
  createCoupon(data: object): Observable<ApiResponse<Coupon>> { return this.http.post<ApiResponse<Coupon>>(`${this.api}/coupons`, data, this.options()); }
  toggleCoupon(id: number): Observable<ApiResponse<null>> { return this.http.post<ApiResponse<null>>(`${this.api}/coupons/${id}/toggle`, {}, this.options()); }
  deleteCoupon(id: number): Observable<ApiResponse<null>> { return this.http.delete<ApiResponse<null>>(`${this.api}/coupons/${id}`, this.options()); }
  lowStock(threshold = 5, page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<ProductSummary>>> {
    const params = new HttpParams().set('threshold', threshold).set('page', page).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<ProductSummary>>>(`${environment.apiUrl}/products/low-stock`, { ...this.options(), params });
  }
}
