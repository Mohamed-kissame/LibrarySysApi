# Library Borrowing RESTful API

A real-world **Library Borrowing RESTful API** built with **ASP.NET Core Web API**, **C#**, **SQL Server**, **ADO.NET**, **Stored Procedures**, **BCrypt password hashing**, and a clean **3-Tier Architecture**.

This project manages books, members, borrowing operations, return operations, borrowing history, dashboard statistics, soft delete rules, database-level business transactions, and the first authentication foundation before JWT authorization.

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
- Authentication foundation with register and login endpoints
- Secure password hashing using BCrypt
- Role-ready user accounts for Admin, Librarian, and Member users

---

## Tech Stack

- ASP.NET Core Web API
- C#
- SQL Server
- ADO.NET
- Stored Procedures
- SQL Transactions
- BCrypt.Net-Next
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
- Mapping internal models to response DTOs
- Returning safe responses without exposing sensitive fields

### BLL Layer

Responsible for:

- Business rules
- Input validation
- ResultCode mapping
- Exception-based business flow
- Password hashing and password verification
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

These endpoints are currently available and will be protected later with JWT roles and policies.

### Authentication Foundation

The project now includes the first authentication layer before JWT implementation.

Current authentication features:

- Separate `Users` table for login accounts
- Separation between `Users` and `Members`
- Register endpoint
- Login endpoint
- BCrypt password hashing
- BCrypt password verification during login
- Role-ready user system
- Passwords are never stored as plain text
- Password hashes are never returned in API responses

Supported roles:

- `Admin`
- `Librarian`
- `Member`

At this stage, JWT token generation is not implemented yet. The current authentication foundation prepares the project for JWT, role-based authorization, and ownership policies.

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

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Create a new system user |
| POST | `/api/auth/login` | Verify email/password and log in the user |

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
If valid, return safe user information
```

The system does not return different messages for invalid email and invalid password. This avoids exposing whether a specific email exists in the system.

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
| 200 OK | Successful GET, update, return, or login operation |
| 201 Created | Resource created successfully |
| 204 No Content | Resource deleted successfully |
| 400 Bad Request | Invalid request data or invalid ID |
| 401 Unauthorized | Invalid login credentials or missing/invalid authentication later |
| 404 Not Found | Resource does not exist |
| 409 Conflict | Business rule conflict |
| 500 Internal Server Error | Unexpected server error |

---

## SQL Transactions

The most important operations in this project are protected using SQL transactions:

```text
Borrow Book
Return Book
```

This prevents inconsistent data.

Example:

```text
Borrowing a book must insert a borrowing record and decrease available copies.

If one step fails, the whole operation is rolled back.
```

---

## Stored Procedures

The project uses stored procedures for all database operations.

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
│   ├── Program.cs
│   └── appsettings.json
│
├── BLL
│   ├── AuthService.cs
│   ├── BookService.cs
│   ├── BorrowingService.cs
│   └── MemberService.cs
│
├── DAL
│   ├── BookDAL.cs
│   ├── BorrowingDAL.cs
│   ├── MemberDAL.cs
│   └── UserDAL.cs
│
└── Models
    ├── Book.cs
    ├── Borrowing.cs
    ├── Member.cs
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

Use **User Secrets** for the real SQL Server connection string.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LibrarySysDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Do not commit real database passwords to GitHub.

### 4. Create the database

Run the SQL scripts for:

```text
Tables
Constraints
Stored procedures
Test data
```

### 5. Run the API

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
- Login verification without exposing password details
- Preparing an API for JWT authentication and role-based authorization

---

## Future Improvements

Possible future improvements:

```text
JWT Authentication
Role-based Authorization
Ownership Policies
Refresh Tokens
Pagination
Search and Filtering
Logging
Global Exception Middleware
Unit Testing
Late Return Fines
Email Notifications
Admin Dashboard
Frontend Client
```

---

## Project Status

Core backend logic completed.

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
Authentication foundation
BCrypt password hashing
Register and login endpoints
```

Next step:

```text
JWT token generation
JWT authentication middleware
Swagger JWT authorization testing
Role-based endpoint protection
Ownership policies for member-specific data
```

---

## Author

**Mohamed Kissame**

Backend Development learner focused on building real-world C# and .NET backend systems.
