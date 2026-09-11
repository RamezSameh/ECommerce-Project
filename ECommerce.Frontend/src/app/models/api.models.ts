export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T | null;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface Category {
  id: number;
  name: string;
  slug?: string;
  parentId?: number;
}

export interface ProductSummary {
  id: number;
  name: string;
  price: number;
  brand?: string;
  averageRating: number;
  reviewCount: number;
  stock: number;
  imageUrl?: string;
}

export interface ProductVariant {
  id?: number;
  name: string;
  value: string;
  price?: number;
  stock: number;
}

export interface Product extends ProductSummary {
  description: string;
  lowStockAlert: boolean;
  category?: Category;
  tags: string[];
  images: { id: number; url: string; isMain: boolean }[];
  variants: ProductVariant[];
}

export interface Review {
  id: number;
  productId: number;
  userName: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface CartItem {
  productId: number;
  productName: string;
  variantId?: number;
  variantSummary?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Cart {
  id: number;
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
  couponCode?: string;
  items: CartItem[];
}

export enum PaymentMethod {
  CashOnDelivery = 0,
  Card = 1,
  Wallet = 2
}

export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4
}

export interface Order {
  id: number;
  orderNumber: string;
  status: OrderStatus;
  statusName: string;
  paymentMethod: PaymentMethod;
  subtotal: number;
  discount: number;
  tax: number;
  shipping: number;
  total: number;
  createdAt: string;
  items: { productId: number; productName: string; variantSummary?: string; quantity: number; unitPrice: number; lineTotal: number }[];
}

export interface AuthResponse {
  isSuccess: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  expiresAt?: string;
  email?: string;
  userId?: string;
  emailConfirmed: boolean;
  roles: string[];
}

export interface UserProfile {
  id?: string;
  userName?: string;
  email?: string;
  fullName?: string;
  address?: string;
  phone?: string;
  emailConfirmed: boolean;
}

export interface Coupon {
  id: number;
  code: string;
  discountAmount?: number;
  discountPercent?: number;
  isActive: boolean;
  expiresAt?: string;
  maxUsages?: number;
  usageCount: number;
}

export interface AdminUser {
  id: string;
  userName?: string;
  email?: string;
  fullName?: string;
  emailConfirmed: boolean;
  isBanned: boolean;
  createdAt: string;
  roles: string[];
}

export interface SalesReport {
  totalRevenue: number;
  totalOrders: number;
  totalProducts: number;
  totalUsers: number;
  revenueByDay: Record<string, number>;
  topProducts: { productId: number; productName: string; unitsSold: number; revenue: number }[];
}
