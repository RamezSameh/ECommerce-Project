import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="max-w-md mx-auto p-6 shadow rounded mt-10">
      <h2>Login</h2>
      <form (ngSubmit)="submit()" class="flex flex-col gap-2">
        <input [(ngModel)]="email" name="email" placeholder="Email" class="border p-2 rounded" />
        <input [(ngModel)]="password" name="password" type="password" placeholder="Password" class="border p-2 rounded" />
        <button class="bg-green-600 text-white p-2 rounded">Login</button>
      </form>
      <p *ngIf="msg" class="text-red-500">{{ msg }}</p>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  msg = '';
  constructor(private auth: AuthService) {}
  submit() {
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (r: any) => {
        this.auth.setToken(r.data?.token || r.token);
        this.msg = 'Logged in';
        window.location.href = '/';
      },
      error: () => this.msg = 'Login failed'
    });
  }
}
