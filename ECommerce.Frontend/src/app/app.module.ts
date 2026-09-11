import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { ProductListComponent } from './components/products/product-list.component';
import { LoginComponent } from './components/auth/login.component';
import { CartComponent } from './components/cart/cart.component';
import { CheckoutComponent } from './components/orders/checkout.component';
import { OrdersComponent } from './components/orders/orders.component';
import { AdminComponent } from './components/admin/admin.component';
import { AuthService } from './services/auth.service';
import { ProductService } from './services/product.service';
import { CartService } from './services/cart.service';
import { OrderService } from './services/order.service';

@NgModule({
  declarations: [AppComponent, ProductListComponent, LoginComponent, CartComponent, CheckoutComponent, OrdersComponent, AdminComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule, RouterModule.forRoot([
    { path: '', component: ProductListComponent },
    { path: 'login', component: LoginComponent },
    { path: 'cart', component: CartComponent },
    { path: 'checkout', component: CheckoutComponent },
    { path: 'orders', component: OrdersComponent },
    { path: 'admin', component: AdminComponent }
  ])],
  providers: [AuthService, ProductService, CartService, OrderService],
  bootstrap: [AppComponent]
})
export class AppModule {}
