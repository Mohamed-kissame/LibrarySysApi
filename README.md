# Library Borrowing RESTful API

A real-world **Library Borrowing RESTful API** built with **ASP.NET Core Web API**, **C#**, **SQL Server**, **ADO.NET**, **Stored Procedures**, **SQL Transactions**, **JWT Authentication**, **Role-Based Authorization**, **Policy-Based Ownership Authorization**, **Refresh Token Rotation**, **Rate Limiting**, **BCrypt password hashing**, **Security Logging**, **Business Auditing**, and a clean **3-Tier Architecture**.

This project manages books, members, borrowing operations, return operations, borrowing history, dashboard statistics, soft delete rules, database-level business transactions, secure authentication, authorization, refresh-token sessions, brute-force protection, and audit logs for important security and business actions.

---

## Project Overview

This project was built to practice backend development beyond simple CRUD operations.

The API supports:

- Managing books
- Managing members
- Borrowing books
- Returning borrowed books
- Tracking available copies
- Preserving borrowing history
- Preventing invalid borrowing operations
- Applying business rules through the BLL and SQL Server stored procedures
- Dashboard-ready statistics endpoints for total books, members, and borrowings
- Register and login endpoints
- Secure password hashing using BCrypt
- JWT access token generation and validation
- Refresh token rotation and logout revocation
- Role-based authorization for Admin, Librarian, and Member users
- Policy-based ownership authorization for member-specific data
- Rate limiting for authentication endpoints
- Security logging for authentication events
- Business auditing for important library actions

---

## Tech Stack

- ASP.NET Core Web API
- C#
- SQL Server
- ADO.NET
- Stored Procedures
- SQL Transactions
- JWT Bearer Authentication
- BCrypt.Net-Next
- ASP.NET Core Rate Limiting
- Swagger / OpenAPI
- DTOs
- Async / Await
- 3-Tier Architecture

---

## Architecture

The project follows a clean layered architecture:

```text
API Layer
↓
BLL Layer
↓
DAL Layer
↓
SQL Server
```

### API Layer

Responsible for:

- HTTP endpoints
- Request DTOs
- Response DTOs
- HTTP status codes
- Swagger documentation/testing
- JWT authentication setup
- Role and policy protection attributes
- Rate limiting policies on authentication endpoints
- Mapping internal models to response DTOs
- Returning safe responses without exposing sensitive fields

### BLL Layer

Responsible for:

- Business rules
- Input validation
- ResultCode mapping
- Exception-based business flow
- Password hashing and password verification
- Refresh token generation, hashing, verification, rotation, and revocation
- Audit log validation before sending records to the DAL
- Preventing invalid operations before reaching the database

### DAL Layer

Responsible for:

- SQL Server connection
- Calling stored procedures
- Reading output parameters
- Mapping SQL results to C# models

### SQL Server

Responsible for:

- Tables
- Constraints
- Relationships
- Stored procedures
- Transactions
- Data integrity
- Refresh token persistence
- Audit log persistence

---

## Main Features

### Books Management

The API allows managing books with full CRUD operations.

Book rules:

- ISBN must be unique.
- Total copies must be greater than zero.
- Available copies cannot be negative.
- Available copies cannot be greater than total copies.
- Books are soft deleted using `IsActive = 0`.
- A book cannot be deleted if it has active borrowings.

### Members Management

The API allows managing library members with full CRUD operations.

Member rules:

- Email must be unique.
- Members are soft deleted using `IsActive = 0`.
- Inactive members cannot borrow books.
- A member cannot be deleted if they have active borrowings.

### Borrowing System

The borrowing system is the core business part of this project.

Borrowing rules:

- A book must exist.
- A book must be active.
- A member must exist.
- A member must be active.
- The book must have available copies.
- A member cannot borrow more than 3 active books.
- A member cannot borrow the same book twice before returning it.
- Borrowing decreases `AvailableCopies` by 1.
- Returning increases `AvailableCopies` by 1.
- Borrowing and returning are protected using SQL transactions.

### Dashboard Statistics

The API includes dashboard-ready endpoints for simple statistics:

- Total books
- Total members
- Total borrowings

These endpoints are protected for Admin and Librarian users.

---

## Security Features

### Authentication

The project includes a secure authentication system.

Authentication features:

- Separate `Users` table for login accounts
- Separation between `Users` and `Members`
- Register endpoint
- Login endpoint
- BCrypt password hashing
- BCrypt password verification during login
- JWT access token generation
- Refresh token generation
- Passwords are never stored as plain text
- Password hashes are never returned in API responses

Supported roles:

- `Admin`
- `Librarian`
- `Member`

### JWT Access Tokens

Access tokens are short-lived JWTs used to access protected endpoints.

JWT tokens include safe claims such as:

- UserID
- Email
- Role
- FullName
- MemberID when applicable

The API validates:

- Issuer
- Audience
- Expiration
- Signing key
- Token signature

Sensitive data is never stored inside the JWT payload.

### User Secrets

The JWT signing key is stored using **User Secrets** during development and is not committed to GitHub.

Production environments should use secure secret storage such as:

- Environment variables
- Azure Key Vault
- Hosting provider secret manager

### Role-Based Authorization

The API protects endpoints using role-based authorization.

Examples:

- Admin can delete books and members.
- Admin and Librarian can create/update books and members.
- Admin and Librarian can manage borrowings.
- Public users can read book listings and book details.

### Policy-Based Ownership Authorization

The API uses policy-based authorization to protect member-specific data.

Members can only access their own profile and borrowing history, while Admin and Librarian users can access all member-related data.

Protected ownership endpoints include:

- `GET /api/members/{memberID}`
- `GET /api/borrowings/member/{memberID}`

### Refresh Token System

The API supports secure refresh token rotation.

- Access tokens are short-lived JWTs.
- Refresh tokens are long-lived random secrets.
- Refresh tokens are stored in the database as BCrypt hashes.
- Refresh tokens are rotated after every use.
- Old refresh tokens are revoked after refresh.
- Logout revokes the current refresh token.

Refresh tokens use the format:

```text
RefreshTokenID.Secret
```

Only the secret part is hashed and stored in SQL Server. The full refresh token is never stored in plain text.

### Rate Limiting

The API uses ASP.NET Core built-in rate limiting to protect public authentication endpoints from brute-force and abuse attempts.

Protected endpoints include:

- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

When too many requests are sent, the API returns:

```http
429 Too Many Requests
```

### Logging and Auditing

The API stores important security and business events in the `AuditLogs` table.

Security events include:

- LoginSuccess
- LoginFailed
- RefreshSuccess
- RefreshFailed
- LogoutSuccess
- LogoutFailed

Business audit events include:

- CreateBook
- UpdateBook
- DeleteBook
- CreateMember
- UpdateMember
- DeleteMember
- BorrowBook
- ReturnBook

Each audit record can store:

- UserID
- EventType
- Action
- EntityName
- EntityID
- Result
- Reason
- IP address
- User-Agent
- Request path
- HTTP method
- CreatedAt timestamp

Sensitive data is never logged, including:

- Passwords
- Password hashes
- Access tokens
- Full refresh tokens
- JWT secret keys
- Connection strings

---

## Database Entities

### Books

```text
BookID
Title
Author
ISBN
TotalCopies
AvailableCopies
IsActive
CreatedAt
UpdatedAt
```

### Members

```text
MemberID
FullName
Email
Phone
IsActive
CreatedAt
UpdatedAt
```

### Borrowings

```text
BorrowingID
BookID
MemberID
BorrowDate
DueDate
ReturnDate
Status
CreatedAt
UpdatedAt
```

Supported borrowing statuses:

```text
Borrowed
Returned
```

### Users

```text
UserID
FullName
Email
PasswordHash
Role
MemberID
IsActive
CreatedAt
UpdatedAt
```

User account rules:

- `Admin` and `Librarian` users must not have a `MemberID`.
- `Member` users must be linked to an existing active `MemberID`.
- Each member can have only one user account.
- User email must be unique.
- Passwords are stored only as BCrypt hashes.

### RefreshTokens

```text
RefreshTokenID
UserID
TokenHash
ExpiresAt
CreatedAt
RevokedAt
ReplacedByRefreshTokenID
ReasonRevoked
```

Refresh token rules:

- Refresh token secrets are stored as BCrypt hashes.
- A revoked refresh token cannot be reused.
- Expired refresh tokens cannot be used.
- Each refresh operation revokes the old token and creates a new one.

### AuditLogs

```text
AuditLogID
UserID
EventType
Action
EntityName
EntityID
Result
Reason
IpAddress
UserAgent
RequestPath
HttpMethod
CreatedAt
```

Audit log rules:

- Security and business events are recorded.
- Sensitive data is never logged.
- Audit logging should not expose passwords, tokens, or secret keys.

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Create a new system user |
| POST | `/api/auth/login` | Verify email/password and return access/refresh tokens |
| POST | `/api/auth/refresh` | Rotate refresh token and return new tokens |
| POST | `/api/auth/logout` | Revoke refresh token |
| GET | `/api/auth/me` | Return current authenticated user claims |

### Books

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/books` | Get all active books |
| GET | `/api/books/{bookId}` | Get book by ID |
| POST | `/api/books` | Add a new book |
| PUT | `/api/books/{bookId}` | Update a book |
| DELETE | `/api/books/{bookId}` | Soft delete a book |
| GET | `/api/books/TotalBooks` | Get total books count |

### Members

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/members` | Get all active members |
| GET | `/api/members/{memberId}` | Get member by ID |
| POST | `/api/members` | Add a new member |
| PUT | `/api/members/{memberId}` | Update a member |
| DELETE | `/api/members/{memberId}` | Soft delete a member |
| GET | `/api/members/TotalMembers` | Get total members count |

### Borrowings

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/borrowings` | Get all borrowings |
| GET | `/api/borrowings/{borrowingId}` | Get borrowing by ID |
| GET | `/api/borrowings/book/{bookId}` | Get borrowing history by book ID |
| GET | `/api/borrowings/member/{memberId}` | Get borrowing history by member ID |
| POST | `/api/borrowings` | Borrow a book |
| PUT | `/api/borrowings/{borrowingId}/return` | Return a borrowed book |
| GET | `/api/borrowings/TotalBorrowing` | Get total borrowings count |

---

## Request Examples

### Register Admin User

```json
{
  "fullName": "Admin User",
  "email": "admin@test.com",
  "password": "Admin12345",
  "role": "Admin",
  "memberID": null
}
```

### Login User

```json
{
  "email": "admin@test.com",
  "password": "Admin12345"
}
```

Login returns safe user information with an access token and refresh token.

### Refresh Token

```json
{
  "refreshToken": "25.longRandomSecret"
}
```

### Logout

```json
{
  "refreshToken": "25.longRandomSecret"
}
```

### Add Book

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "totalCopies": 5
}
```

### Add Member

```json
{
  "fullName": "Mohamed Amrani",
  "email": "mohamed.amrani@example.com",
  "phone": "+212600111222"
}
```

### Borrow Book

```json
{
  "bookID": 1,
  "memberID": 1
}
```

### Return Book

No request body is required.

```http
PUT /api/borrowings/1/return
```

---

## Authentication Flow

### Register Flow

```text
Register request
↓
Validate full name, email, password, role, and optional MemberID
↓
Check if email already exists
↓
Hash password using BCrypt
↓
Store user with PasswordHash only
↓
Return safe user information
```

### Login Flow

```text
Login request
↓
Validate email and password
↓
Find user by email
↓
Verify entered password using BCrypt.Verify
↓
Generate JWT access token
↓
Generate refresh token secret
↓
Store BCrypt hash of refresh token secret
↓
Return access token + refresh token
```

The system does not return different messages for invalid email and invalid password. This avoids exposing whether a specific email exists in the system.

### Refresh Flow

```text
Refresh request
↓
Parse RefreshTokenID.Secret
↓
Find refresh token row by RefreshTokenID
↓
Verify secret using BCrypt.Verify
↓
Check token is not expired
↓
Check token is not revoked
↓
Revoke old refresh token
↓
Create new refresh token
↓
Generate new access token
↓
Return new access token + new refresh token
```

### Logout Flow

```text
Logout request
↓
Verify refresh token
↓
Revoke refresh token
↓
Client deletes local tokens
```

Logout revokes the refresh token. The access token remains stateless and naturally expires based on its lifetime.

### Password Security

During registration:

```text
Plain password
↓
BCrypt hash
↓
Stored as PasswordHash in SQL Server
```

During login:

```text
Entered password
↓
BCrypt.Verify(enteredPassword, storedPasswordHash)
↓
Valid or invalid login
```

The system does not store:

```text
Plain passwords
Separate password salt columns
Passwords in API responses
PasswordHash in API responses
```

BCrypt automatically handles the salt internally inside the generated hash.

---

## Business Rules

### Borrow Book Flow

When borrowing a book, the system checks:

```text
1. Book exists
2. Book is active
3. Member exists
4. Member is active
5. Book has available copies
6. Member has fewer than 3 active borrowings
7. Member has not already borrowed the same book
8. Insert borrowing record
9. Decrease AvailableCopies by 1
```

This operation is handled inside a SQL transaction.

### Return Book Flow

When returning a book, the system checks:

```text
1. Borrowing exists
2. Borrowing is still active
3. Update borrowing status to Returned
4. Set ReturnDate
5. Increase AvailableCopies by 1
```

This operation is handled inside a SQL transaction.

### Delete Book Rule

A book can be soft deleted only if it has no active borrowings.

```text
Active borrowing = Status = 'Borrowed' AND ReturnDate IS NULL
```

If active borrowings exist, the API returns:

```text
409 Conflict
```

### Delete Member Rule

A member can be soft deleted only if they have no active borrowings.

If active borrowings exist, the API returns:

```text
409 Conflict
```

### User Registration Rules

When registering a user, the system checks:

```text
1. Full name is valid
2. Email is valid and unique
3. Password is valid
4. Role is Admin, Librarian, or Member
5. Member role has a valid MemberID
6. Admin and Librarian roles do not have MemberID
7. Member exists and is active
8. Member does not already have a user account
```

---

## HTTP Status Codes

| Status Code | Meaning |
|---|---|
| 200 OK | Successful GET, refresh, return, or login operation |
| 201 Created | Resource created successfully |
| 204 No Content | Resource deleted, updated, or logout completed successfully |
| 400 Bad Request | Invalid request data or invalid ID |
| 401 Unauthorized | Missing, expired, or invalid authentication |
| 403 Forbidden | Authenticated user does not have permission |
| 404 Not Found | Resource does not exist |
| 409 Conflict | Business rule conflict |
| 429 Too Many Requests | Rate limit exceeded |
| 500 Internal Server Error | Unexpected server error |

---

## SQL Transactions

The most important operations in this project are protected using SQL transactions:

```text
Borrow Book
Return Book
Refresh Token Rotation
```

This prevents inconsistent data.

Example:

```text
Borrowing a book must insert a borrowing record and decrease available copies.

If one step fails, the whole operation is rolled back.
```

---

## Stored Procedures

The project uses stored procedures for database operations.

### Books Procedures

```text
sp_Books_GetAll
sp_Books_GetById
sp_Books_Add
sp_Books_Update
sp_Books_Delete
sp_Books_ISBNExists
sp_Books_ExistsById
sp_Books_TotalBooks
```

### Members Procedures

```text
sp_Members_GetAll
sp_Members_GetById
sp_Members_Add
sp_Members_Update
sp_Members_Delete
sp_Members_EmailExists
sp_Members_ExistsById
sp_Members_TotalMembers
```

### Borrowings Procedures

```text
sp_Borrowings_GetAll
sp_Borrowings_GetById
sp_Borrowings_GetByBookId
sp_Borrowings_GetByMemberId
sp_Borrowings_Add
sp_Borrowings_Return
sp_Borrowings_TotalBorrowing
```

### Users Procedures

```text
sp_Users_Add
sp_Users_GetById
sp_Users_GetByEmail
sp_Users_EmailExists
```

### Refresh Token Procedures

```text
sp_RefreshTokens_Add
sp_RefreshTokens_GetByID
sp_RefreshTokens_Revoke
sp_RefreshTokens_Rotate
```

### Audit Log Procedures

```text
sp_AuditLogs_Add
```

---

## ResultCode Handling

Some stored procedures return result codes to the C# application.

Example for borrowing:

```text
 1  = Success
-1  = Book not found
-2  = Book inactive
-3  = Member not found
-4  = Member inactive
-5  = No available copies
-6  = Borrowing limit reached
-7  = Same book already borrowed
```

Example for user registration:

```text
 1  = User created successfully
-1  = Email already exists
-2  = Invalid role
-3  = Member role requires MemberID
-4  = Admin/Librarian cannot have MemberID
-5  = Member not found or inactive
-6  = Member already has a user account
```

Example for refresh token rotation:

```text
 1  = Success
-1  = Old token not found
-2  = Old token already revoked
-3  = Old token expired
-4  = User not found or inactive
-5  = Invalid input
-99 = Unexpected SQL error
```

The BLL maps these result codes to exceptions, and the API controller maps those exceptions to proper HTTP responses.

---

## Swagger Testing

The API was manually tested using Swagger.

Tested scenarios include:

```text
Books CRUD
Members CRUD
Borrowing history
Borrow book success
Borrow invalid book/member
Borrow inactive book/member
Borrow with no available copies
Borrow same book twice
Borrowing limit reached
Return book success
Return already returned borrowing
Delete book with active borrowings
Delete member with active borrowings
Dashboard statistics endpoints
Register admin user
Register librarian user
Register member user
Register duplicate email
Register member without MemberID
Register admin/librarian with MemberID
Login with correct password
Login with wrong password
JWT authorization in Swagger
Role-based authorization
Policy-based ownership access
Refresh token rotation
Old refresh token reuse blocked
Logout refresh token revocation
Rate limiting returns 429 Too Many Requests
Login and business actions stored in AuditLogs
```

---

## Project Structure

```text
LibrarySysApi
│
├── LibrarySys
│   ├── Controllers
│   │   ├── AuthController.cs
│   │   ├── BooksController.cs
│   │   ├── BorrowingController.cs
│   │   └── MemberController.cs
│   │
│   ├── DTOs
│   │   ├── AuthDTOs
│   │   ├── BookDTOs
│   │   ├── BorrowingDTOs
│   │   └── MemberDTOs
│   │
│   ├── Authorization
│   ├── Services
│   ├── Program.cs
│   └── appsettings.json
│
├── BLL
│   ├── AuthService.cs
│   ├── AuditLogService.cs
│   ├── BookService.cs
│   ├── BorrowingService.cs
│   └── MemberService.cs
│
├── DAL
│   ├── AuditLogDAL.cs
│   ├── BookDAL.cs
│   ├── BorrowingDAL.cs
│   ├── MemberDAL.cs
│   ├── RefreshTokenDAL.cs
│   └── UserDAL.cs
│
└── Models
    ├── AuditLog.cs
    ├── Book.cs
    ├── Borrowing.cs
    ├── Member.cs
    ├── RefreshToken.cs
    └── User.cs
```

---

## Setup Instructions

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY_NAME.git
```

### 2. Open the solution

Open the `.sln` file in Visual Studio.

### 3. Configure the connection string

Use **User Secrets** or another secure secret storage mechanism for the real SQL Server connection string.

Example for development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LibrarySysDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Do not commit real database passwords or JWT keys to GitHub.

### 4. Configure JWT settings

Store the JWT key securely using User Secrets during development.

Example:

```bash
dotnet user-secrets set "JwtSettings:Key" "YOUR_LONG_SECURE_SECRET_KEY"
```

The non-secret JWT settings can stay in `appsettings.json`:

```json
{
  "JwtSettings": {
    "Issuer": "LibrarySysApi",
    "Audience": "LibrarySysClient",
    "ExpirationMinutes": 60
  }
}
```

### 5. Create the database

Run the SQL scripts for:

```text
Tables
Constraints
Stored procedures
Test data
```

### 6. Run the API

Start the Web API project and open Swagger.

---

## Learning Outcomes

This project helped me practice:

- RESTful API design
- Clean layered architecture
- DTO usage
- ADO.NET
- Stored procedures
- SQL transactions
- SQL constraints
- Business rule validation
- Soft delete
- ResultCode handling
- HTTP status codes
- Swagger testing
- Controller → BLL → DAL → SQL Server flow
- Authentication foundation design
- Secure password hashing with BCrypt
- JWT authentication and validation
- Swagger JWT authorization testing
- Role-based authorization
- Policy-based ownership authorization
- Refresh token rotation and logout revocation
- Rate limiting against brute-force abuse
- Security logging
- Business auditing
- Avoiding sensitive data leaks in logs

---

## Future Improvements

Possible future improvements:

```text
Pagination
Search and filtering
Global exception middleware
Unit testing
Integration testing
Serilog or structured logging provider
Audit log search/filter endpoints
Security alerts and monitoring
Late return fines
Email notifications
Admin dashboard
Frontend client
HttpOnly cookie-based refresh token storage
Production deployment hardening
```

---

## Project Status

Core backend logic and security foundation completed.

The project currently includes:

```text
Books management
Members management
Borrowing system
Return system
Borrowing history
Dashboard statistics
Soft delete protection
Transaction-based business operations
Authentication with BCrypt password hashing
JWT access tokens
Role-based authorization
Policy-based ownership authorization
Refresh token rotation
Logout revocation
Rate limiting for auth endpoints
Security logging
Business auditing
```

Next step:

```text
Monitoring and alerting
Global exception middleware
Pagination and search
Frontend integration
```

---

## Author

**Mohamed Kissame**

Backend Development learner focused on building real-world C# and .NET backend systems.
