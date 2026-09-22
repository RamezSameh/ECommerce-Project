import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-forgot-password', template: `
  <section class="auth-page"><div class="auth-card card"><div class="auth-brand">E Market</div>
  <h1>نسيت كلمة المرور؟</h1>
  <p *ngIf="!sent">أدخل بريدك الإلكتروني وسنرسل لك رابط إعادة تعيين كلمة المرور.</p>
  <form (ngSubmit)="submit()" *ngIf="!sent"><label>البريد الإلكتروني<input [(ngModel)]="email" name="email" type="email" required placeholder="example@mail.com" /></label><button class="primary full" [disabled]="busy">إرسال رابط الاستعادة</button></form>
  <div class="error" *ngIf="message">{{ message }}</div>
  <div *ngIf="sent"><p>تم إرسال رابط الاستعادة إلى بريدك الإلكتروني (تحقق من مجلد الرسائل).</p><a routerLink="/login" class="primary full">العودة لتسجيل الدخول</a></div>
  <a routerLink="/login" class="text-button" *ngIf="!sent">تذكرت كلمة المرور؟ سجل الدخول</a>
  </div></section>
`, standalone: false })
export class ForgotPasswordComponent {
  email = ''; busy = false; sent = false; message = '';
  constructor(private auth: AuthService) {}
  submit(): void {
    this.busy = true; this.message = '';
    this.auth.forgotPassword({ email: this.email }).subscribe({
      next: () => { this.busy = false; this.sent = true; },
      error: e => { this.busy = false; this.message = e?.error?.message || 'تعذر إرسال رابط الاستعادة'; }
    });
  }
}
