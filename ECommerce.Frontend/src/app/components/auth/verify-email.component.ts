import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-verify-email', template: `
  <section class="auth-page"><div class="auth-card card"><div class="auth-brand">E Market</div>
  <h1>تأكيد البريد الإلكتروني</h1>
  <p *ngIf="state === 'loading'">جاري تأكيد بريدك الإلكتروني...</p>
  <p *ngIf="state === 'success'">تم تأكيد بريدك الإلكتروني بنجاح. أهلاً بك في المتجر!</p>
  <p class="error" *ngIf="state === 'error'">{{ message }}</p>
  <a routerLink="/login" class="primary full" *ngIf="state === 'success'">تسجيل الدخول</a>
  <a routerLink="/" class="primary full" *ngIf="state === 'error'">العودة للمتجر</a>
  </div></section>
`, standalone: false })
export class VerifyEmailComponent implements OnInit {
  state: 'loading' | 'success' | 'error' = 'loading';
  message = '';
  constructor(private route: ActivatedRoute, private auth: AuthService, private router: Router) {}
  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email') || '';
    const token = this.route.snapshot.queryParamMap.get('token') || '';
    if (!email || !token) {
      this.state = 'error';
      this.message = 'رابط التأكيد غير مكتمل. تأكد من فتح الرابط المرسل إلى بريدك.';
      return;
    }
    this.auth.verifyEmail({ email, token }).subscribe({
      next: response => {
        if (response.data?.token) this.auth.saveSession(response.data);
        this.state = 'success';
        window.setTimeout(() => this.router.navigate(['/']), 2500);
      },
      error: e => {
        this.state = 'error';
        this.message = e?.error?.message || 'تعذر تأكيد البريد الإلكتروني. ربما انتهت صلاحية الرابط.';
      }
    });
  }
}
