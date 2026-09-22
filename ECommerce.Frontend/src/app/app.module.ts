import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { ProductListComponent } from './components/products/product-list.component';
import { ProductDetailComponent } from './components/products/product-detail.component';
import { WishlistComponent } from './components/products/wishlist.component';
import { LoginComponent } from './components/auth/login.component';
import { CartComponent } from './components/cart/cart.component';
import { CheckoutComponent } from './components/orders/checkout.component';
import { OrdersComponent } from './components/orders/orders.component';
import { AdminComponent } from './components/admin/admin.component';
import { AuthGuard, AdminGuard } from './guards/auth.guard';
import { ProfileComponent } from './components/profile/profile.component';
import { VerifyEmailComponent } from './components/auth/verify-email.component';
import { ForgotPasswordComponent } from './components/auth/forgot-password.component';
import { ResetPasswordComponent } from './components/auth/reset-password.component';
import { OrderDetailComponent } from './components/orders/order-detail.component';

@NgModule({
  declarations: [AppComponent, ProductListComponent, ProductDetailComponent, WishlistComponent, ProfileComponent, LoginComponent, VerifyEmailComponent, ForgotPasswordComponent, ResetPasswordComponent, CartComponent, CheckoutComponent, OrdersComponent, OrderDetailComponent, AdminComponent],
  imports: [BrowserModule, FormsModule, RouterModule.forRoot([
    { path: '', component: ProductListComponent },
    { path: 'products/:id', component: ProductDetailComponent },
    { path: 'wishlist', component: WishlistComponent, canActivate: [AuthGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'verify-email', component: VerifyEmailComponent },
    { path: 'forgot-password', component: ForgotPasswordComponent },
    { path: 'reset-password', component: ResetPasswordComponent },
    { path: 'cart', component: CartComponent },
    { path: 'checkout', component: CheckoutComponent, canActivate: [AuthGuard] },
    { path: 'orders', component: OrdersComponent, canActivate: [AuthGuard] },
    { path: 'orders/:id', component: OrderDetailComponent, canActivate: [AuthGuard] },
    { path: 'admin', component: AdminComponent, canActivate: [AdminGuard] },
    { path: '**', redirectTo: '' }
  ])],
  providers: [provideHttpClient(withInterceptorsFromDi())],
  bootstrap: [AppComponent]
})
export class AppModule {}
