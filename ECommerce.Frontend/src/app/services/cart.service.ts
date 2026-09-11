import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class CartService {
  private api = `${environment.apiUrl}/cart`;
  constructor(private http: HttpClient, private auth: AuthService) {}

  getCart(): Observable<any> {
    const h = this.auth.getHeaders();
    return this.http.get(this.api, { headers: h });
  }

  addItem(dto: any): Observable<any> {
    return this.http.post(`${this.api}/items`, dto, { headers: this.auth.getHeaders() });
  }

  removeItem(id: number): Observable<any> {
    return this.http.delete(`${this.api}/items/${id}`, { headers: this.auth.getHeaders() });
  }

  applyCoupon(code: string): Observable<any> {
    return this.http.post(`${this.api}/coupon`, { code }, { headers: this.auth.getHeaders() });
  }
}
