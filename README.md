# LiveCommerce Platform

Nền tảng vận hành livestream & social-commerce theo tài liệu PRD (Product Requirements and Technical Spec). Stack: **ASP.NET Core 8**, **PostgreSQL**, **Redis**, **RabbitMQ**, **SignalR**, **React + Vite + TypeScript + Ant Design**.

## Cấu trúc solution

- **Backend (src/)**
  - `LiveCommerce.Api` – Web API, auth, Swagger
  - `LiveCommerce.Application` – use cases, DTOs, interfaces
  - `LiveCommerce.Domain` – entities, enums
  - `LiveCommerce.Infrastructure` – EF Core, PostgreSQL, Redis, RabbitMQ, Auth
  - `LiveCommerce.Worker` – background worker (queue consumer)
  - `LiveCommerce.Shared` – response envelopes, shared DTOs
- **Frontend (frontend/)** – React + Vite + TypeScript + Ant Design, Zustand, Axios

## Yêu cầu

- .NET 8 SDK  
- Node.js 20+  
- Docker & Docker Compose (để chạy PostgreSQL, Redis, RabbitMQ)  
- (Tùy chọn) Công cụ EF: `dotnet tool install --global dotnet-ef`

## Chạy nhanh

### 1. Khởi động PostgreSQL, Redis, RabbitMQ

```bash
docker-compose up -d
```

### 2. Tạo migration và cập nhật DB (lần đầu)

```bash
cd src
dotnet ef migrations add InitialCreate --project LiveCommerce.Infrastructure --startup-project LiveCommerce.Api --output-dir Persistence/Migrations
dotnet ef database update --project LiveCommerce.Infrastructure --startup-project LiveCommerce.Api
```

Nếu chưa cài `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
```

### 3. Chạy Backend API

```bash
cd src
dotnet run --project LiveCommerce.Api
```

API: http://localhost:5000 (hoặc port trong launchSettings). Swagger: http://localhost:5000/swagger

### 4. Chạy Frontend

```bash
cd frontend
npm install
npm run dev
```

Mở http://localhost:5173. Đăng nhập mặc định (sau khi seed): **Mã shop**: `SHOP01`, **User**: `admin`, **Mật khẩu**: `admin123`.

## Biến môi trường / Cấu hình

- **Backend** (`src/LiveCommerce.Api/appsettings.json`):  
  - `ConnectionStrings:DefaultConnection` – chuỗi kết nối PostgreSQL  
  - `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`  
  - `Cors:Origins` – origin frontend (ví dụ http://localhost:5173)
- **Frontend**: `VITE_API_URL` (để trỏ API; để trống thì dùng proxy Vite tới backend)

## Health checks

API có endpoint readiness cho load balancer / orchestrator:

- **GET** `/health/ready` – kiểm tra PostgreSQL và RabbitMQ. Trả về JSON: `status` (Healthy/Unhealthy/Degraded) và danh sách `checks` (postgres, rabbitmq).

Ví dụ:

```bash
curl http://localhost:5000/health/ready
```

## Triển khai (checklist)

- Cấu hình `ConnectionStrings:DefaultConnection`, `Jwt:*`, `RabbitMQ:*`, `Cors:Origins` (production).
- Chạy migrations: `dotnet ef database update --project LiveCommerce.Infrastructure --startup-project LiveCommerce.Api`.
- Chạy **LiveCommerce.Api** và **LiveCommerce.Worker** (cùng DB, cùng RabbitMQ).
- Build frontend: `cd frontend && npm ci && npm run build`; host thư mục `dist` qua CDN/web server.
- Cấu hình reverse proxy (nginx/IIS) cho API và `/hubs` (WebSocket) nếu cần.

## Các module theo PRD (đã có skeleton)

- Auth & RBAC (JWT, shop + user, permission)
- Live Session, Comment Center (realtime – SignalR sẽ gắn sau)
- Quick Order, Order Management
- Customer, Product, Blacklist, Follow-up
- Dashboard (placeholder)

Triển khai chi tiết từng sprint theo tài liệu **LiveCommerce_Product_Requirements_and_Technical_Spec_Final2.docx**.

## License

Internal / Confidential – development use only.
