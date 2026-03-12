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
