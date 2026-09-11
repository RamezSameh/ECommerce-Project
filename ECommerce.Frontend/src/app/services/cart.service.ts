import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from 'src/environments/environment';
import { ApiResponse, Cart } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private api = `${environment.apiUrl}/cart`;
  constructor(private http: HttpClient, private auth: AuthService) {}

  getCart(): Observable<ApiResponse<Cart>> {
    return this.http.get<ApiResponse<Cart>>(this.api, { headers: this.auth.getHeaders() });
  }
  addItem(dto: object): Observable<ApiResponse<Cart>> {
    return this.http.post<ApiResponse<Cart>>(`${this.api}/items`, dto, { headers: this.auth.getHeaders() });
  }
  updateItem(id: number, quantity: number): Observable<ApiResponse<Cart>> {
    return this.http.put<ApiResponse<Cart>>(`${this.api}/items/${id}`, { quantity }, { headers: this.auth.getHeaders() });
  }
  removeItem(id: number): Observable<ApiResponse<Cart>> {
    return this.http.delete<ApiResponse<Cart>>(`${this.api}/items/${id}`, { headers: this.auth.getHeaders() });
  }
  applyCoupon(code: string): Observable<ApiResponse<Cart>> {
    return this.http.post<ApiResponse<Cart>>(`${this.api}/coupon`, { code }, { headers: this.auth.getHeaders() });
  }
  removeCoupon(): Observable<ApiResponse<Cart>> {
    return this.http.delete<ApiResponse<Cart>>(`${this.api}/coupon`, { headers: this.auth.getHeaders() });
  }
}
