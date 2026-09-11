import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private api = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getAll(query?: any): Observable<any> {
    return this.http.get(`${this.api}?search=${query?.search || ''}&page=${query?.page || 1}`);
  }

  getById(id: number): Observable<any> {
    return this.http.get(`${this.api}/${id}`);
  }

  getReviews(id: number): Observable<any> {
    return this.http.get(`${this.api}/${id}/reviews`);
  }
}
