import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { ApiResponse, AuthResponse, UserProfile } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = `${environment.apiUrl}/auth`;
  private tokenKey = 'jwt_token';
  private refreshKey = 'refresh_token';
  private rolesKey = 'user_roles';
  private guestKey = 'guest_id';

  constructor(private http: HttpClient, private router: Router) {}

  register(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/register`, data);
  }
  login(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/login`, data);
  }
  saveSession(response: AuthResponse): void {
    if (response.token) localStorage.setItem(this.tokenKey, response.token);
    if (response.refreshToken) localStorage.setItem(this.refreshKey, response.refreshToken);
    localStorage.setItem(this.rolesKey, JSON.stringify(response.roles || []));
  }
  getToken(): string | null { return localStorage.getItem(this.tokenKey); }
  getRefreshToken(): string | null { return localStorage.getItem(this.refreshKey); }
  isAuthenticated(): boolean { return !!this.getToken(); }
  hasRole(role: string): boolean { return this.getRoles().some(value => value.toLowerCase() === role.toLowerCase()); }
  getRoles(): string[] {
    try { return JSON.parse(localStorage.getItem(this.rolesKey) || '[]') as string[]; }
    catch { return []; }
  }
  getGuestId(): string {
    let guestId = localStorage.getItem(this.guestKey);
    if (!guestId) {
      guestId = crypto.randomUUID();
      localStorage.setItem(this.guestKey, guestId);
    }
    return guestId;
  }
  profile(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<UserProfile>>(`${this.api}/profile`, { headers: this.getHeaders() });
  }
  updateProfile(data: object): Observable<ApiResponse<UserProfile>> {
    return this.http.put<ApiResponse<UserProfile>>(`${this.api}/profile`, data, { headers: this.getHeaders() });
  }
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshKey);
    localStorage.removeItem(this.rolesKey);
    this.router.navigate(['/login']);
  }
  verifyEmail(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/verify-email`, data);
  }
  forgotPassword(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/forgot-password`, data);
  }
  resetPassword(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/reset-password`, data);
  }
  changePassword(data: object): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/change-password`, data, { headers: this.getHeaders() });
  }
  getHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      Authorization: token ? `Bearer ${token}` : '',
      'X-Guest-Id': this.getGuestId()
    });
  }
}
