# Library Borrowing RESTful API

A real-world **Library Borrowing RESTful API** built with **ASP.NET Core Web API**, **C#**, **SQL Server**, **ADO.NET**, **Stored Procedures**, and a clean **3-Tier Architecture**.

This project manages books, members, borrowing operations, return operations, borrowing history, soft delete rules, and database-level business transactions.

---

## Project Overview

This project was built to practice backend development beyond simple CRUD operations.

The API supports:

* Managing books
* Managing members
* Borrowing books
* Returning borrowed books
* Tracking available copies
* Preserving borrowing history
* Preventing invalid borrowing operations
* Applying business rules through the BLL and SQL Server stored procedures
* Dashboard-ready statistics endpoints for total books, members, and borrowings

---

## Tech Stack

* ASP.NET Core Web API
* C#
* SQL Server
* ADO.NET
* Stored Procedures
* Swagger / OpenAPI
* DTOs
* Async / Await
* SQL Transactions
* 3-Tier Architecture

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

* HTTP endpoints
* Request DTOs
* Response DTOs
* HTTP status codes
* Swagger documentation/testing
* Mapping internal models to response DTOs

### BLL Layer

Responsible for:

* Business rules
* Input validation
* ResultCode mapping
* Exception-based business flow
* Preventing invalid operations

### DAL Layer

Responsible for:

* SQL Server connection
* Calling stored procedures
* Reading output parameters
* Mapping SQL results to C# models

### SQL Server

Responsible for:

* Tables
* Constraints
* Relationships
* Stored procedures
* Transactions
* Data integrity

---

## Main Features

### Books Management

The API allows managing books with full CRUD operations.

Book rules:

* ISBN must be unique.
* Total copies must be greater than zero.
* Available copies cannot be negative.
* Available copies cannot be greater than total copies.
* Books are soft deleted using `IsActive = 0`.
* A book cannot be deleted if it has active borrowings.

---

### Members Management

The API allows managing library members with full CRUD operations.

Member rules:

* Email must be unique.
* Members are soft deleted using `IsActive = 0`.
* Inactive members cannot borrow books.
* A member cannot be deleted if they have active borrowings.

---

### Borrowing System

The borrowing system is the core part of this project.

Borrowing rules:

* A book must exist.
* A book must be active.
* A member must exist.
* A member must be active.
* The book must have available copies.
* A member cannot borrow more than 3 active books.
* A member cannot borrow the same book twice before returning it.
* Borrowing decreases `AvailableCopies` by 1.
* Returning increases `AvailableCopies` by 1.
* Borrowing and returning are protected using SQL transactions.

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

---

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

---

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

---

## API Endpoints

### Books

| Method | Endpoint              | Description          |
| ------ | --------------------- | -------------------- |
| GET    | `/api/books`          | Get all active books |
| GET    | `/api/books/{bookId}` | Get book by ID       |
| POST   | `/api/books`          | Add a new book       |
| PUT    | `/api/books/{bookId}` | Update a book        |
| DELETE | `/api/books/{bookId}` | Soft delete a book   |

---

### Members

| Method | Endpoint                  | Description            |
| ------ | ------------------------- | ---------------------- |
| GET    | `/api/members`            | Get all active members |
| GET    | `/api/members/{memberId}` | Get member by ID       |
| POST   | `/api/members`            | Add a new member       |
| PUT    | `/api/members/{memberId}` | Update a member        |
| DELETE | `/api/members/{memberId}` | Soft delete a member   |

---

### Borrowings

| Method | Endpoint                               | Description                        |
| ------ | -------------------------------------- | ---------------------------------- |
| GET    | `/api/borrowings`                      | Get all borrowings                 |
| GET    | `/api/borrowings/{borrowingId}`        | Get borrowing by ID                |
| GET    | `/api/borrowings/book/{bookId}`        | Get borrowing history by book ID   |
| GET    | `/api/borrowings/member/{memberId}`    | Get borrowing history by member ID |
| POST   | `/api/borrowings`                      | Borrow a book                      |
| PUT    | `/api/borrowings/{borrowingId}/return` | Return a borrowed book             |

---

## Request Examples

### Add Book

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "totalCopies": 5
}
```

---

### Add Member

```json
{
  "fullName": "Mohamed Amrani",
  "email": "mohamed.amrani@example.com",
  "phone": "+212600111222"
}
```

---

### Borrow Book

```json
{
  "bookID": 1,
  "memberID": 1
}
```

---

### Return Book

No request body is required.

```http
PUT /api/borrowings/1/return
```

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

---

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

---

### Delete Book Rule

A book can be soft deleted only if it has no active borrowings.

```text
Active borrowing = Status = 'Borrowed' AND ReturnDate IS NULL
```

If active borrowings exist, the API returns:

```text
409 Conflict
```

---

### Delete Member Rule

A member can be soft deleted only if they have no active borrowings.

If active borrowings exist, the API returns:

```text
409 Conflict
```

---

## HTTP Status Codes

| Status Code               | Meaning                                     |
| ------------------------- | ------------------------------------------- |
| 200 OK                    | Successful GET, update, or return operation |
| 201 Created               | Resource created successfully               |
| 204 No Content            | Resource deleted successfully               |
| 400 Bad Request           | Invalid request data or invalid ID          |
| 404 Not Found             | Resource does not exist                     |
| 409 Conflict              | Business rule conflict                      |
| 500 Internal Server Error | Unexpected server error                     |

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
```

### Borrowings Procedures

```text
sp_Borrowings_GetAll
sp_Borrowings_GetById
sp_Borrowings_GetByBookId
sp_Borrowings_GetByMemberId
sp_Borrowings_Add
sp_Borrowings_Return
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
```

---

## Project Structure

```text
LibrarySysApi
│
├── LibrarySys
│   ├── Controllers
│   ├── DTOs
│   ├── Program.cs
│   └── appsettings.json
│
├── BLL
│   ├── BookService.cs
│   ├── MemberService.cs
│   └── BorrowingService.cs
│
├── DAL
│   ├── BookDAL.cs
│   ├── MemberDAL.cs
│   └── BorrowingDAL.cs
│
└── Models
    ├── Book.cs
    ├── Member.cs
    └── Borrowing.cs
```

---

## Setup Instructions

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY_NAME.git
```

---

### 2. Open the solution

Open the `.sln` file in Visual Studio.

---

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

---

### 4. Create the database

Run the SQL scripts for:

```text
Tables
Constraints
Stored procedures
Test data
```

---

### 5. Run the API

Start the Web API project and open Swagger.

---

## Learning Outcomes

This project helped me practice:

* RESTful API design
* Clean layered architecture
* DTO usage
* ADO.NET
* Stored procedures
* SQL transactions
* SQL constraints
* Business rule validation
* Soft delete
* ResultCode handling
* HTTP status codes
* Swagger testing
* Controller → BLL → DAL → SQL Server flow

---

## Future Improvements

Possible future improvements:

```text
JWT Authentication
Roles and Policies
Pagination
Logging
Unit Testing
Late return fines
Email notifications
Admin dashboard
```

---

## Project Status

Core backend logic completed.

The project includes:

```text
Books management
Members management
Borrowing system
Return system
Borrowing history
Soft delete protection
Transaction-based business operations
```

---

## Author

**Mohamed Kissame**

Backend Development learner focused on building real-world C# and .NET backend systems.
