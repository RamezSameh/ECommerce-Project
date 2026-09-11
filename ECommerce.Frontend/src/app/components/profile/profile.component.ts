import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { UserProfile } from '../../models/api.models';
import { NotificationService } from '../../services/notification.service';

@Component({ selector: 'app-profile', template: `
  <section class="content-width page-section narrow"><div class="profile-hero card"><div class="avatar">{{ (profile?.fullName || profile?.userName || 'U').charAt(0).toUpperCase() }}</div><div><span class="eyebrow">حسابي</span><h1>{{ profile?.fullName || profile?.userName }}</h1><p>{{ profile?.email }}</p></div></div>
  <form class="card profile-form" (ngSubmit)="save()"><h2>بيانات الحساب</h2><label>الاسم الكامل<input [(ngModel)]="model.fullName" name="fullName" /></label><label>العنوان<textarea [(ngModel)]="model.address" name="address"></textarea></label><label>رقم الهاتف<input [(ngModel)]="model.phone" name="phone" /></label><div class="profile-actions"><button class="primary" [disabled]="saving">حفظ التغييرات</button><span class="verified" *ngIf="profile?.emailConfirmed">✓ البريد الإلكتروني مؤكد</span></div></form>
  </section>
`, standalone: false })
export class ProfileComponent implements OnInit {
  profile?: UserProfile; model = { fullName: '', address: '', phone: '' }; saving = false;
  constructor(private auth: AuthService, private notifications: NotificationService) {}
  ngOnInit(): void { this.auth.profile().subscribe(response => { this.profile = response.data || undefined; if (this.profile) this.model = { fullName: this.profile.fullName || '', address: this.profile.address || '', phone: this.profile.phone || '' }; }); }
  save(): void { this.saving = true; this.auth.updateProfile(this.model).subscribe({ next: response => { this.saving = false; this.profile = response.data || this.profile; this.notifications.show('تم حفظ بيانات الحساب بنجاح'); }, error: () => { this.saving = false; this.notifications.show('تعذر حفظ البيانات', 'error'); } }); }
}
