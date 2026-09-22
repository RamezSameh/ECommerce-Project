import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { UserProfile } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({ selector: 'app-profile', template: `
  <section class="content-width page-section narrow"><div class="profile-hero card"><div class="avatar">{{ (profile?.fullName || profile?.userName || 'U').charAt(0).toUpperCase() }}</div><div><span class="eyebrow">حسابي</span><h1>{{ profile?.fullName || profile?.userName }}</h1><p>{{ profile?.email }}</p></div></div>
  <form class="card profile-form" (ngSubmit)="save()"><h2>بيانات الحساب</h2><label>الاسم الكامل<input [(ngModel)]="model.fullName" name="fullName" /></label><label>العنوان<textarea [(ngModel)]="model.address" name="address"></textarea></label><label>رقم الهاتف<input [(ngModel)]="model.phone" name="phone" /></label><div class="profile-actions"><button class="primary" [disabled]="saving">حفظ التغييرات</button><span class="verified" *ngIf="profile?.emailConfirmed">✓ البريد الإلكتروني مؤكد</span></div></form>
  <form class="card profile-form" (ngSubmit)="changePassword()"><h2>تغيير كلمة المرور</h2><label>كلمة المرور الحالية<input [(ngModel)]="passwords.current" name="currentPassword" type="password" required /></label><label>كلمة المرور الجديدة<input [(ngModel)]="passwords.new" name="newPassword" type="password" required minlength="6" /></label><div class="profile-actions"><button class="primary" [disabled]="changing">تغيير كلمة المرور</button></div></form>
  </section>
`, standalone: false })
export class ProfileComponent implements OnInit {
  profile?: UserProfile; model = { fullName: '', address: '', phone: '' };
  passwords = { current: '', new: '' }; saving = false; changing = false;
  constructor(private auth: AuthService, private notifications: NotificationService) {}
  ngOnInit(): void { this.auth.profile().subscribe(response => { this.profile = response.data || undefined; if (this.profile) this.model = { fullName: this.profile.fullName || '', address: this.profile.address || '', phone: this.profile.phone || '' }; }); }
  save(): void { this.saving = true; this.auth.updateProfile(this.model).subscribe({ next: response => { this.saving = false; this.profile = response.data || this.profile; this.notifications.show('تم حفظ بيانات الحساب بنجاح'); }, error: () => { this.saving = false; this.notifications.show('تعذر حفظ البيانات', 'error'); } }); }
  changePassword(): void {
    this.changing = true;
    this.auth.changePassword({ currentPassword: this.passwords.current, newPassword: this.passwords.new }).subscribe({
      next: response => {
        this.changing = false;
        if (response.data?.token) this.auth.saveSession(response.data);
        this.passwords = { current: '', new: '' };
        this.notifications.show('تم تغيير كلمة المرور بنجاح');
      },
      error: e => { this.changing = false; this.notifications.show(e?.error?.message || 'تعذر تغيير كلمة المرور', 'error'); }
    });
  }
}
