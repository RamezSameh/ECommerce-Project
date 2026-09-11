import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;
  constructor(private http: HttpClient, private auth: AuthService) {}

  checkout(dto: any): Observable<any> {
    return this.http.post(`${this.api}/checkout`, dto, { headers: this.auth.getHeaders() });
  }

  myOrders(): Observable<any> {
    return this.http.get(`${this.api}/my-orders`, { headers: this.auth.getHeaders() });
  }

  cancel(id: number): Observable<any> {
    return this.http.post(`${this.api}/${id}/cancel`, {}, { headers: this.auth.getHeaders() });
  }
}
