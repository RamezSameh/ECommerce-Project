import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface AppNotification { id: number; message: string; type: 'success' | 'error' | 'info'; }

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 1;
  private state = new BehaviorSubject<AppNotification[]>([]);
  readonly notifications$ = this.state.asObservable();

  show(message: string, type: AppNotification['type'] = 'success'): void {
    const notification = { id: this.nextId++, message, type };
    this.state.next([...this.state.value, notification]);
    window.setTimeout(() => this.dismiss(notification.id), 3500);
  }
  dismiss(id: number): void { this.state.next(this.state.value.filter(item => item.id !== id)); }
}
