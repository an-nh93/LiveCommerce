import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_URL || '';

export const api = axios.create({
  baseURL: `${API_BASE}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface CurrentUser {
  userId: number;
  shopId: number;
  shopCode: string;
  shopName: string;
  username: string;
  displayName?: string;
  roleCode: string;
  permissions: string[];
}

export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: CurrentUser;
}

export const authApi = {
  login: (shopCode: string, username: string, password: string) =>
    api.post<ApiResponse<LoginResult>>('/auth/login', { shopCode, username, password }),
  me: () => api.get<ApiResponse<CurrentUser>>('/auth/me'),
};

export interface LiveSessionDto {
  id: number;
  name: string;
  externalLiveId?: string;
  startedAtUtc: string;
  endedAtUtc?: string;
  isActive: boolean;
}

export interface CommentListDto {
  id: number;
  liveSessionId: number;
  liveSessionName: string;
  content: string;
  commentTimeUtc: string;
  senderName?: string;
  senderExternalId?: string;
  status: number;
  assignedUserId?: number;
  assignedUserName?: string;
  isSpam: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const liveSessionsApi = {
  list: () => api.get<ApiResponse<LiveSessionDto[]>>('/live-sessions'),
};

export const commentsApi = {
  list: (params: { liveSessionId?: number; status?: number; assignedUserId?: number; page?: number; pageSize?: number }) =>
    api.get<ApiResponse<PagedResult<CommentListDto>>>('/comments', { params }),
  getById: (id: number) => api.get<ApiResponse<CommentDetailDto>>(`/comments/${id}`),
  take: (id: number) => api.post<ApiResponse<CommentListDto>>(`/comments/${id}/take`),
  assign: (id: number, assignToUserId: number) => api.post<ApiResponse<CommentListDto>>(`/comments/${id}/assign`, { assignToUserId }),
  updateStatus: (id: number, status: number, note?: string) => api.post<ApiResponse<CommentListDto>>(`/comments/${id}/status`, { status, note }),
};

export interface CommentDetailDto extends CommentListDto {
  rawPayloadJson?: string;
  customerId?: number;
  customerName?: string;
  customerPhone?: string;
}

export interface ProductVariantDto {
  id: number;
  sku: string;
  color?: string;
  size?: string;
  price: number;
}
export interface ProductListDto {
  id: number;
  code: string;
  name: string;
  category?: string;
  basePrice: number;
  isActive: boolean;
  variants: ProductVariantDto[];
}
export interface OrderListDto {
  id: number;
  orderNo: string;
  liveSessionId?: number;
  receiverName?: string;
  receiverPhone?: string;
  totalAmount: number;
  status: number;
  assignedUserName?: string;
  createdAtUtc: string;
}
export interface OrderItemDto {
  productVariantId: number;
  productName?: string;
  sku?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}
export interface OrderDetailDto extends OrderListDto {
  receiverAddress?: string;
  note?: string;
  subTotal: number;
  shippingFee: number;
  discount: number;
  commentId?: number;
  customerId?: number;
  items: OrderItemDto[];
}

export const productsApi = {
  list: () => api.get<ApiResponse<ProductListDto[]>>('/products'),
};
export const ordersApi = {
  list: (params: { liveSessionId?: number; status?: number; search?: string; page?: number; pageSize?: number }) =>
    api.get<ApiResponse<PagedResult<OrderListDto>>>('/orders', { params }),
  getById: (id: number) => api.get<ApiResponse<OrderDetailDto>>(`/orders/${id}`),
  create: (body: CreateOrderRequest) => api.post<ApiResponse<OrderDetailDto>>('/orders', body),
  updateStatus: (id: number, status: number, note?: string) => api.post<ApiResponse<OrderDetailDto>>(`/orders/${id}/status`, { status, note }),
};
export interface CreateOrderRequest {
  liveSessionId?: number;
  commentId?: number;
  customerId?: number;
  receiverName?: string;
  receiverPhone?: string;
  receiverAddress?: string;
  note?: string;
  shippingFee?: number;
  discount?: number;
  items: { productVariantId: number; quantity: number; unitPrice: number }[];
}
