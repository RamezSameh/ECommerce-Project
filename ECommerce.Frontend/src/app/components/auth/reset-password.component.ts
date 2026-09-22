import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-reset-password', template: `
  <section class="auth-page"><div class="auth-card card"><div class="auth-brand">E Market</div>
  <h1>كلمة مرور جديدة</h1>
  <p>اختر كلمة مرور قوية لحسابك.</p>
  <form (ngSubmit)="submit()"><label>كلمة المرور الجديدة<input [(ngModel)]="password" name="password" type="password" required minlength="6" placeholder="٦ أحرف على الأقل" /></label><label>تأكيد كلمة المرور<input [(ngModel)]="confirm" name="confirm" type="password" required minlength="6" /></label><button class="primary full" [disabled]="busy">تغيير كلمة المرور</button></form>
  <div class="error" *ngIf="message">{{ message }}</div>
  </div></section>
`, standalone: false })
export class ResetPasswordComponent implements OnInit {
  email = ''; token = ''; password = ''; confirm = ''; busy = false; message = '';
  constructor(private route: ActivatedRoute, private auth: AuthService, private router: Router) {}
  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') || '';
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
    if (!this.email || !this.token) this.message = 'رابط الاستعادة غير مكتمل. اطلب رابطاً جديداً من صفحة نسيان كلمة المرور.';
  }
  submit(): void {
    this.message = '';
    if (this.password !== this.confirm) { this.message = 'كلمتا المرور غير متطابقتين'; return; }
    if (!this.email || !this.token) { this.message = 'رابط الاستعادة غير صالح'; return; }
    this.busy = true;
    this.auth.resetPassword({ email: this.email, token: this.token, newPassword: this.password }).subscribe({
      next: response => {
        this.busy = false;
        if (response.data?.token) this.auth.saveSession(response.data);
        this.router.navigate(['/login']);
      },
      error: e => { this.busy = false; this.message = e?.error?.message || 'تعذر تغيير كلمة المرور. ربما انتهت صلاحية الرابط.'; }
    });
  }
}
