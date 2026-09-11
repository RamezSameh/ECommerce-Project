import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', template: `
  <section class="auth-page"><div class="auth-card card"><div class="auth-brand">E Market</div><h1>{{ registerMode ? 'إنشاء حساب جديد' : 'مرحباً بعودتك' }}</h1><p>{{ registerMode ? 'ابدأ تجربة تسوق مختلفة' : 'سجل دخولك لإتمام طلباتك' }}</p>
  <form (ngSubmit)="submit()"><label *ngIf="registerMode">اسم المستخدم<input [(ngModel)]="userName" name="userName" required /></label><label>البريد الإلكتروني<input [(ngModel)]="email" name="email" type="email" required /></label><label>كلمة المرور<input [(ngModel)]="password" name="password" type="password" required minlength="6" /></label><label *ngIf="registerMode">الاسم الكامل<input [(ngModel)]="fullName" name="fullName" /></label><button class="primary full" [disabled]="busy">{{ registerMode ? 'إنشاء الحساب' : 'تسجيل الدخول' }}</button></form><div class="error" *ngIf="message">{{ message }}</div><button class="text-button" (click)="registerMode = !registerMode">{{ registerMode ? 'لديك حساب؟ سجل الدخول' : 'ليس لديك حساب؟ أنشئ حساباً' }}</button></div></section>
`, standalone: false })
export class LoginComponent {
  registerMode = false; userName = ''; email = ''; password = ''; fullName = ''; message = ''; busy = false;
  constructor(private auth: AuthService, private router: Router) {}
  submit(): void {
    this.busy = true; const request = this.registerMode ? this.auth.register({ userName: this.userName, email: this.email, password: this.password, fullName: this.fullName }) : this.auth.login({ email: this.email, password: this.password });
    request.subscribe({ next: response => { this.busy = false; if (response.data) { this.auth.saveSession(response.data); this.router.navigate(['/']); } else this.message = response.message; }, error: error => { this.busy = false; this.message = error?.error?.message || 'تعذر تنفيذ العملية'; } });
  }
}
