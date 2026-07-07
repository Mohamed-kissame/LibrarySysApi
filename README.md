\# Library Borrowing RESTful API



A real-world Library Borrowing RESTful API built with \*\*ASP.NET Core Web API\*\*, \*\*C#\*\*, \*\*SQL Server\*\*, \*\*ADO.NET\*\*, \*\*Stored Procedures\*\*, and a clean \*\*3-Tier Architecture\*\*.



This project manages books, members, borrowing operations, return operations, borrowing history, soft delete rules, and database-level business transactions.



\---



\## Project Goal



The goal of this project is to practice building a backend API that is more than simple CRUD.



The system supports:



\- Managing books

\- Managing members

\- Borrowing books

\- Returning books

\- Tracking available copies

\- Preventing invalid borrowing operations

\- Preserving borrowing history

\- Applying business rules through the BLL and SQL Server stored procedures



\---



\## Tech Stack



\- ASP.NET Core Web API

\- C#

\- SQL Server

\- ADO.NET

\- Stored Procedures

\- Swagger / OpenAPI

\- 3-Tier Architecture

\- DTOs

\- Async / Await

\- SQL Transactions



\---



\## Architecture



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



\### API Layer



Responsible for:



\- HTTP endpoints

\- Request DTOs

\- Response DTOs

\- Status codes

\- Swagger testing

\- Mapping models to response DTOs



\### BLL Layer



Responsible for:



\- Business rules

\- Validation

\- ResultCode mapping

\- Exception handling logic

\- Preventing invalid operations



\### DAL Layer



Responsible for:



\- SQL Server connection

\- Calling stored procedures

\- Reading output parameters

\- Mapping `SqlDataReader` data to models



\### SQL Server



Responsible for:



\- Tables

\- Constraints

\- Relationships

\- Stored procedures

\- Transactions

\- Data integrity



\---



\## Main Entities



\### Books



A book represents an item that can be borrowed.



Main fields:



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



Important rules:



\- ISBN must be unique.

\- TotalCopies must be greater than 0.

\- AvailableCopies cannot be negative.

\- AvailableCopies cannot be greater than TotalCopies.

\- Inactive books cannot be borrowed.

\- Books are soft deleted using `IsActive = 0`.

\- A book cannot be deleted if it has active borrowings.



\---



\### Members



A member represents a library user who can borrow books.



Main fields:



```text

MemberID

FullName

Email

Phone

IsActive

CreatedAt

UpdatedAt

```



Important rules:



\- Email must be unique.

\- Members are soft deleted using `IsActive = 0`.

\- Inactive members cannot borrow books.

\- A member cannot be deleted if they have active borrowings.



\---



\### Borrowings



A borrowing represents a book borrowed by a member.



Main fields:



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



Supported statuses:



```text

Borrowed

Returned

```



Important rules:



\- Borrowing creates a new record in `Borrowings`.

\- Borrowing decreases `Books.AvailableCopies` by 1.

\- Returning updates the borrowing status to `Returned`.

\- Returning increases `Books.AvailableCopies` by 1.

\- Borrow and return operations use SQL transactions.

\- A member cannot borrow more than 3 active books.

\- A member cannot borrow the same book twice before returning it.

\- A book cannot be borrowed if no copies are available.



\---



\## API Endpoints



\### Books



| Method | Endpoint | Description |

|---|---|---|

| GET | `/api/books` | Get all active books |

| GET | `/api/books/{bookId}` | Get book by ID |

| POST | `/api/books` | Add new book |

| PUT | `/api/books/{bookId}` | Update book |

| DELETE | `/api/books/{bookId}` | Soft delete book |



\---



\### Members



| Method | Endpoint | Description |

|---|---|---|

| GET | `/api/members` | Get all active members |

| GET | `/api/members/{memberId}` | Get member by ID |

| POST | `/api/members` | Add new member |

| PUT | `/api/members/{memberId}` | Update member |

| DELETE | `/api/members/{memberId}` | Soft delete member |



\---



\### Borrowings



| Method | Endpoint | Description |

|---|---|---|

| GET | `/api/borrowings` | Get all borrowings |

| GET | `/api/borrowings/{borrowingId}` | Get borrowing by ID |

| GET | `/api/borrowings/book/{bookId}` | Get borrowing history by book ID |

| GET | `/api/borrowings/member/{memberId}` | Get borrowing history by member ID |

| POST | `/api/borrowings` | Borrow a book |

| PUT | `/api/borrowings/{borrowingId}/return` | Return a borrowed book |



\---



\## Request Examples



\### Add Book



```json

{

&#x20; "title": "Clean Code",

&#x20; "author": "Robert C. Martin",

&#x20; "isbn": "9780132350884",

&#x20; "totalCopies": 5

}

```



\---



\### Add Member



```json

{

&#x20; "fullName": "Mohamed Amrani",

&#x20; "email": "mohamed.amrani@example.com",

&#x20; "phone": "+212600111222"

}

```



\---



\### Borrow Book



```json

{

&#x20; "bookID": 1,

&#x20; "memberID": 1

}

```



\---



\### Return Book



No request body is required.



```http

PUT /api/borrowings/1/return

```



\---



\## Important Business Rules



\### Borrow Book



When borrowing a book, the API checks:



```text

1\. Book exists

2\. Book is active

3\. Member exists

4\. Member is active

5\. Book has available copies

6\. Member has fewer than 3 active borrowings

7\. Member has not already borrowed the same book

8\. Insert borrowing record

9\. Decrease AvailableCopies by 1

```



This operation is handled inside a SQL transaction.



\---



\### Return Book



When returning a book, the API checks:



```text

1\. Borrowing exists

2\. Borrowing is still active

3\. Update borrowing status to Returned

4\. Set ReturnDate

5\. Increase AvailableCopies by 1

```



This operation is handled inside a SQL transaction.



\---



\### Delete Book



A book can be soft deleted only if it has no active borrowings.



```text

Active borrowing = Status = 'Borrowed' AND ReturnDate IS NULL

```



If active borrowings exist, the API returns:



```text

409 Conflict

```



\---



\### Delete Member



A member can be soft deleted only if they have no active borrowings.



If active borrowings exist, the API returns:



```text

409 Conflict

```



\---



\## Status Codes



| Status Code | Meaning |

|---|---|

| 200 OK | Successful GET, update, return operation |

| 201 Created | Resource created successfully |

| 204 No Content | Resource deleted successfully |

| 400 Bad Request | Invalid request data or invalid ID |

| 404 Not Found | Resource does not exist |

| 409 Conflict | Business rule conflict |

| 500 Internal Server Error | Unexpected server error |



\---



\## SQL Transactions



The most important operations in this project are protected using SQL transactions:



```text

Borrow Book

Return Book

```



This prevents data inconsistency.



Example:



```text

Borrowing a book must insert a borrowing record and decrease available copies.

If one step fails, the whole operation is rolled back.

```



\---



\## Stored Procedures



The project uses stored procedures for all database operations.



Main stored procedure groups:



```text

Books:

\- sp\_Books\_GetAll

\- sp\_Books\_GetById

\- sp\_Books\_Add

\- sp\_Books\_Update

\- sp\_Books\_Delete

\- sp\_Books\_ISBNExists

\- sp\_Books\_ExistsById



Members:

\- sp\_Members\_GetAll

\- sp\_Members\_GetById

\- sp\_Members\_Add

\- sp\_Members\_Update

\- sp\_Members\_Delete

\- sp\_Members\_EmailExists

\- sp\_Members\_ExistsById



Borrowings:

\- sp\_Borrowings\_GetAll

\- sp\_Borrowings\_GetById

\- sp\_Borrowings\_GetByBookId

\- sp\_Borrowings\_GetByMemberId

\- sp\_Borrowings\_Add

\- sp\_Borrowings\_Return

```



\---



\## ResultCode Handling



Some stored procedures return result codes to the C# application.



Example for borrowing:



```text

&#x20;1  = success

\-1  = book not found

\-2  = book inactive

\-3  = member not found

\-4  = member inactive

\-5  = no available copies

\-6  = borrowing limit reached

\-7  = same book already borrowed

```



The BLL maps these result codes to exceptions, and the API controller maps those exceptions to proper HTTP responses.



\---



\## Setup Instructions



\### 1. Clone the repository



```bash

git clone https://github.com/YOUR\_USERNAME/YOUR\_REPOSITORY\_NAME.git

```



\### 2. Open the solution in Visual Studio



Open the `.sln` file.



\### 3. Configure the connection string



Use \*\*User Secrets\*\* for the real SQL Server connection string.



Example:



```json

{

&#x20; "ConnectionStrings": {

&#x20;   "DefaultConnection": "Server=.;Database=LibrarySysDB;Trusted\_Connection=True;TrustServerCertificate=True"

&#x20; }

}

```



Do not commit real database passwords to GitHub.



\### 4. Create the database



Run the SQL scripts for:



```text

Tables

Constraints

Stored procedures

Test data

```



\### 5. Run the API



Start the Web API project and open Swagger.



\---



\## Testing



The API was manually tested using Swagger.



Tested scenarios include:



```text

Books CRUD

Members CRUD

Borrowing history

Borrow book success

Borrow book invalid book/member

Borrow inactive book/member

Borrow with no available copies

Borrow same book twice

Borrowing limit reached

Return book success

Return already returned borrowing

Delete book with active borrowings

Delete member with active borrowings

```



\---



\## Learning Outcomes



This project helped practice:



\- RESTful API design

\- Layered architecture

\- DTO usage

\- ADO.NET

\- Stored procedures

\- SQL transactions

\- Business rule validation

\- Soft delete

\- HTTP status codes

\- Swagger testing

\- Clean backend flow from Controller to SQL Server



\---



\## Project Status



Core backend logic completed.



Ready for future improvements:



```text

JWT Authentication

Roles and Policies

Pagination

Logging

Unit testing

Fines for late returns

Email notifications

Admin dashboard

```



\---



\## Author



\*\*Mohamed Kissame\*\*



Software Engineering / Backend Development learner focused on building real-world C# and .NET backend systems.



