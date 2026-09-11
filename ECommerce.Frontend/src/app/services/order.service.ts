import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse, Order, PagedResult } from '../models/api.models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;
  constructor(private http: HttpClient, private auth: AuthService) {}
  checkout(dto: object): Observable<ApiResponse<Order | { order: Order; paymentUrl: string }>> {
    return this.http.post<ApiResponse<Order | { order: Order; paymentUrl: string }>>(`${this.api}/checkout`, dto, { headers: this.auth.getHeaders() });
  }
  myOrders(page = 1): Observable<ApiResponse<PagedResult<Order>>> {
    return this.http.get<ApiResponse<PagedResult<Order>>>(`${this.api}/my-orders?page=${page}&pageSize=10`, { headers: this.auth.getHeaders() });
  }
  getById(id: number): Observable<ApiResponse<Order>> {
    return this.http.get<ApiResponse<Order>>(`${this.api}/${id}`, { headers: this.auth.getHeaders() });
  }
  cancel(id: number): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(`${this.api}/${id}/cancel`, {}, { headers: this.auth.getHeaders() });
  }
  invoice(id: number): Observable<Blob> {
    return this.http.get(`${this.api}/${id}/invoice`, { headers: this.auth.getHeaders(), responseType: 'blob' });
  }
}
