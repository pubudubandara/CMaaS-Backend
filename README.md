# SchemaFlow - Content Management as a Service API

A powerful, multi-tenant Content Management as a Service (CMS) API built with ASP.NET Core 8 and Entity Framework Core, supporting both JWT and API Key authentication.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Database Models](#database-models)
- [Project Structure](#project-structure)

## 🎯 Overview

SchemaFlow is a flexible, multi-tenant content management system that allows organizations to:
- Create custom content types (schemas)
- Manage content entries dynamically
- Control content visibility for public delivery
- Manage API keys for programmatic access
- View dashboard statistics

## ✨ Features

- **Multi-Tenant Architecture**: Complete data isolation per tenant
- **Dual Authentication**: JWT tokens and API Keys
- **Dynamic Content Types**: Define flexible schemas for content
- **Content Visibility Control**: Toggle content visibility from admin panel
- **API Key Management**: Create, manage, and revoke API keys
- **Dashboard Statistics**: Real-time analytics on content and API usage
- **Pagination & Filtering**: Built-in support for large datasets
- **Request Logging**: Comprehensive logging for debugging

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer + Custom API Key Middleware
- **Validation**: FluentValidation
- **Hashing**: BCrypt.Net

## 📦 Prerequisites

- .NET 8 SDK or higher
- PostgreSQL 12+
- Visual Studio 2022 or VS Code
- Git

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CMaaS.Backend
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Create a PostgreSQL database and update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=schemaflow_db;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-characters-long"
  }
}
```

### 4. Apply Migrations

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at `http://localhost:5000` or `https://localhost:5001`

## ⚙️ Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long"
  }
}
```

### Database Connection

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=schemaflow_db;Username=postgres;Password=password"
  }
}
```

## 📚 API Endpoints

### Authentication

#### Register Company
```
POST /api/Auth/register-company
```

**Request Body:**
```json
{
  "organizationName": "Acme Corporation",
  "adminName": "John Doe",
  "email": "admin@acme.com",
  "password": "SecurePassword123!"
}
```

**Response (201 Created):**
```json
{
  "message": "Company registered successfully! Use the Admin Dashboard to create API keys.",
  "tenantId": 1,
  "apiKey": ""
}
```

#### Login
```
POST /api/Auth/login
```

**Request Body:**
```json
{
  "email": "admin@acme.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**JWT Token Contains:**
- `nameid`: User ID
- `email`: User email
- `FullName`: User's full name
- `TenantId`: Organization ID
- `TenantName`: Organization name
- `role`: User role (Admin, Editor, Viewer)

---

### Content Types

#### Create Content Type
```
POST /api/ContentTypes
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "name": "Blog Post",
  "schema": {
    "title": "string",
    "content": "string",
    "author": "string",
    "publishedDate": "date"
  }
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "name": "Blog Post",
  "schema": {...},
  "tenantId": 1,
  "createdAt": "2025-02-14T10:30:00Z"
}
```

#### Get All Content Types
```
GET /api/ContentTypes/{tenantId}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Blog Post",
    "schema": {...},
    "tenantId": 1
  }
]
```

#### Get Content Type by ID
```
GET /api/ContentTypes/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Blog Post",
  "schema": {...},
  "tenantId": 1
}
```

#### Update Content Type
```
PUT /api/ContentTypes/{id}
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "name": "Updated Blog Post",
  "schema": {...}
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Updated Blog Post",
  "schema": {...},
  "tenantId": 1
}
```

---

### Content Entries

#### Create Content Entry
```
POST /api/ContentEntries
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "contentTypeId": 1,
  "data": {
    "title": "My First Blog Post",
    "content": "This is the content...",
    "author": "John Doe"
  }
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "contentTypeId": 1,
  "data": {...},
  "tenantId": 1,
  "isVisible": true,
  "createdAt": "2025-02-14T10:30:00Z"
}
```

#### Get Entries by Content Type
```
GET /api/ContentEntries/{contentTypeId}?page=1&pageSize=10&searchTerm=optional
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "totalRecords": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "data": [...]
}
```

#### Get Entry by ID
```
GET /api/ContentEntries/entry/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "contentTypeId": 1,
  "data": {...},
  "tenantId": 1,
  "isVisible": true
}
```

#### Toggle Entry Visibility
```
PATCH /api/ContentEntries/{id}/toggle-visibility
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "message": "Visibility toggled successfully",
  "data": {
    "id": 1,
    "contentTypeId": 1,
    "data": {...},
    "isVisible": false
  }
}
```

---

### Content Delivery (Public API)

#### Get Public Entries (Only Visible)
```
GET /api/Delivery/{contentTypeName}?page=1&pageSize=10&searchTerm=optional
X-Api-Key: {api_key}
```

**Response (200 OK):**
```json
{
  "meta": {
    "page": 1,
    "pageSize": 10,
    "total": 5,
    "totalPages": 1
  },
  "data": [
    {
      "id": 1,
      "data": {...},
      "contentTypeId": 1,
      "tenantId": 1
    }
  ]
}
```

#### Get Single Public Entry
```
GET /api/Delivery/{contentTypeName}/{id}
X-Api-Key: {api_key}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "data": {...},
  "contentTypeId": 1,
  "contentTypeName": "Blog Post",
  "tenantId": 1
}
```

---

### Dashboard

#### Get Dashboard Statistics
```
GET /api/Dashboard/stats
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "totalContentTypes": 5,
  "totalEntries": 42,
  "totalApiKeys": 3,
  "recentEntries": [
    {
      "id": 42,
      "typeName": "Blog Post",
      "createdAt": "2025-02-14T10:30:00Z"
    }
  ]
}
```

---

### Tenants

#### Get All Tenants
```
GET /api/Tenants
Authorization: Bearer {token}
```

#### Create Tenant
```
POST /api/Tenants
Authorization: Bearer {token}
```

---

## 🔐 Authentication

### JWT Bearer Token

Include the token in the `Authorization` header:

```bash
curl -H "Authorization: Bearer {token}" https://api.example.com/api/ContentTypes
```

**Token Contains:**
- User ID
- Email
- Full Name
- Tenant ID
- Tenant Name (Organization)
- User Role

### API Key

Include the API key in the `X-Api-Key` header:

```bash
curl -H "X-Api-Key: {api_key}" https://api.example.com/api/Delivery/BlogPost
```

**API Key Claims:**
- Tenant ID
- API Key ID
- API Key Name

---

## 📊 Database Models

### User
```csharp
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int TenantId { get; set; }
    public UserRole Role { get; set; }
    public Tenant? Tenant { get; set; }
}
```

### Tenant
```csharp
public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SubscriptionPlan PlanType { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ApiKey> ApiKeys { get; set; }
}
```

### ContentType
```csharp
public class ContentType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public JsonDocument Schema { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Tenant? Tenant { get; set; }
}
```

### ContentEntry
```csharp
public class ContentEntry
{
    public int Id { get; set; }
    public JsonDocument Data { get; set; }
    public int ContentTypeId { get; set; }
    public int TenantId { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public ContentType? ContentType { get; set; }
    public Tenant? Tenant { get; set; }
}
```

### ApiKey
```csharp
public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Tenant? Tenant { get; set; }
}
```

---

## 📁 Project Structure

```
SchemaFlow.Backend/
├── Controllers/
│   ├── AuthController.cs
│   ├── ContentEntriesController.cs
│   ├── ContentTypesController.cs
│   ├── DeliveryController.cs
│   ├── DashboardController.cs
│   └── TenantsController.cs
├── Models/
│   ├── User.cs
│   ├── Tenant.cs
│   ├── ContentType.cs
│   ├── ContentEntry.cs
│   ├── ApiKey.cs
│   ├── UserRole.cs
│   └── SubscriptionPlan.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IContentEntryService.cs
│   │   ├── IContentTypeService.cs
│   │   ├── IJwtTokenService.cs
│   │   ├── IUserContextService.cs
│   │   ├── IApiKeyService.cs
│   │   ├── ITenantService.cs
│   │   └── IDashboardService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── ContentEntryService.cs
│       ├── ContentTypeService.cs
│       ├── JwtTokenService.cs
│       ├── UserContextService.cs
│       ├── ApiKeyService.cs
│       ├── TenantService.cs
│       └── DashboardService.cs
├── Middlewares/
│   ├── ApiKeyMiddleware.cs
│   └── ExceptionMiddleware.cs
├── Dtos/
│   ├── RegisterRequestDto.cs
│   ├── UserDto.cs
│   ├── FilterDto.cs
│   ├── DashboardStatsDto.cs
│   └── RecentEntryDto.cs
├── Data/
│   └── AppDbContext.cs
├── Migrations/
│   └── [EF Core migrations]
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## 💡 Examples

### Complete Workflow Example

#### 1. Register Company
```bash
curl -X POST https://localhost:5001/api/Auth/register-company \
  -H "Content-Type: application/json" \
  -d '{
    "organizationName": "Acme Corp",
    "adminName": "John Doe",
    "email": "john@acme.com",
    "password": "SecurePass123!"
  }'
```

#### 2. Login
```bash
curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@acme.com",
    "password": "SecurePass123!"
  }'
```

#### 3. Create Content Type
```bash
curl -X POST https://localhost:5001/api/ContentTypes \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Blog Post",
    "schema": {
      "title": "string",
      "content": "string",
      "author": "string"
    }
  }'
```

#### 4. Create Content Entry
```bash
curl -X POST https://localhost:5001/api/ContentEntries \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "contentTypeId": 1,
    "data": {
      "title": "My Blog",
      "content": "Hello World",
      "author": "John"
    }
  }'
```

#### 5. Get Public Content (via API Key)
```bash
curl -X GET https://localhost:5001/api/Delivery/BlogPost?page=1 \
  -H "X-Api-Key: {api_key}"
```

---

## 🐛 Error Handling

All error responses follow this format:

```json
{
  "message": "Error description"
}
```

**Common HTTP Status Codes:**
- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request parameters
- `401 Unauthorized` - Missing or invalid authentication
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## 🔄 Pagination

All list endpoints support pagination:

```
GET /api/endpoint?page=1&pageSize=10&searchTerm=optional
```

**Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Number of records per page (default: 10)
- `searchTerm`: Optional search filter

---

