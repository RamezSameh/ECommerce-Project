import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse, Category, PagedResult, Product, ProductSummary, Review } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private api = `${environment.apiUrl}/products`;
  constructor(private http: HttpClient) {}
  getAll(query: Record<string, string | number | undefined> = {}): Observable<ApiResponse<PagedResult<ProductSummary>>> {
    const params = Object.entries({ page: 1, pageSize: 12, ...query })
      .filter(([, value]) => value !== undefined && (typeof value !== 'string' || value !== ''))
      .reduce((result, [key, value]) => result.set(key, String(value)), new HttpParams());
    return this.http.get<ApiResponse<PagedResult<ProductSummary>>>(this.api, { params });
  }
  getById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.api}/${id}`);
  }
  getReviews(id: number, page = 1): Observable<ApiResponse<PagedResult<Review>>> {
    return this.http.get<ApiResponse<PagedResult<Review>>>(`${this.api}/${id}/reviews?page=${page}&pageSize=10`);
  }
  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${this.api}/categories`);
  }
  addReview(id: number, data: object, headers: HttpHeaders): Observable<ApiResponse<Review>> {
    return this.http.post<ApiResponse<Review>>(`${this.api}/${id}/reviews`, data, { headers });
  }
  getWishlist(headers: HttpHeaders): Observable<ApiResponse<ProductSummary[]>> {
    return this.http.get<ApiResponse<ProductSummary[]>>(`${this.api}/wishlist`, { headers });
  }
  addToWishlist(productId: number, headers: HttpHeaders): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.api}/wishlist`, { productId }, { headers });
  }
  removeFromWishlist(productId: number, headers: HttpHeaders): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/wishlist/${productId}`, { headers });
  }
}
