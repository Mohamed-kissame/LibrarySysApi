using System.Net;
using DAL;
using Models;

namespace BLL
{
    public class BorrowingService
    {
        private readonly BorrowingDAL _borrowingDAL;
        private readonly BookDAL _bookDAL;
        private readonly MemberDAL _memberDAL;

        public BorrowingService(
            BorrowingDAL borrowingDAL,
            BookDAL bookDAL,
            MemberDAL memberDAL)
        {
            _borrowingDAL = borrowingDAL;
            _bookDAL = bookDAL;
            _memberDAL = memberDAL;
        }

        public async Task<List<Borrowing>> GetAllBorrowingsAsync()
        {
            return await _borrowingDAL.GetAllBorrowingsAsync();
        }

        public async Task<Borrowing?> GetBorrowingByIDAsync(int BorrowingID)
        {

            return await _borrowingDAL.GetBorrowingByIDAsync(BorrowingID);
        }

        public async Task<List<Borrowing>> GetBorrowingByBookIDAsync(int BookID)
        {

            bool bookExists = await _bookDAL.BookExistsByIdAsync(BookID);

            if (!bookExists)
            {
                throw new KeyNotFoundException($"Book with ID {BookID} was not found.");
            }

            return await _borrowingDAL.GetBorrowingsByBookIDAsync(BookID);
        }

        public async Task<List<Borrowing>> GetBorrowingByMemberIDAsync(int MemberID)
        {
            bool memberExists = await _memberDAL.MemberExistsAsync(MemberID);

            if (!memberExists)
            {
                throw new KeyNotFoundException($"Member with ID {MemberID} was not found.");
            }

            return await _borrowingDAL.GetBorrowingsByMemberIDAsync(MemberID);
        }

        public async Task<Borrowing> AddBorrowingAsync(int bookId, int memberId)
        {

            if (bookId <= 0)
            {
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));
            }

            if (memberId <= 0)
            {
                throw new ArgumentException("Member ID must be greater than zero.", nameof(memberId));
            }

            var results = await _borrowingDAL.AddBorrowingAsync(bookId, memberId);


            return results.ResultCode switch
            {
                1 when results.Borrowing is not null
                    => results.Borrowing,

                1
                    => throw new Exception("Borrowing was created but could not be retrieved."),

                -1
                    => throw new KeyNotFoundException($"Book with ID {bookId} was not found."),

                -2
                    => throw new InvalidOperationException($"Book with ID {bookId} is inactive and cannot be borrowed."),

                -3
                    => throw new KeyNotFoundException($"Member with ID {memberId} was not found."),

                -4
                    => throw new InvalidOperationException($"Member with ID {memberId} is inactive and cannot borrow books."),

                -5
                    => throw new InvalidOperationException("No available copies for this book."),

                -6
                    => throw new InvalidOperationException("This member has reached the borrowing limit."),

                -7
                    => throw new InvalidOperationException("This member has already borrowed this book and has not returned it yet."),

                _
                    => throw new Exception("An unexpected error occurred while adding the borrowing record.")
            };


        }

        public async Task<Borrowing> ReturnBorrowingAsync(int borrowingId)
        {
            if (borrowingId <= 0)
            {
                throw new ArgumentException("Borrowing ID must be greater than zero.", nameof(borrowingId));
            }

            var results = await _borrowingDAL.ReturnBorrowingAsync(borrowingId);

            return results.ResultCode switch
            {
                1 when results.Borrowing is not null
                    => results.Borrowing,

                1
                    => throw new Exception("Borrowing was returned but could not be retrieved."),

                -1
                    => throw new KeyNotFoundException($"Borrowing record with ID {borrowingId} was not found."),

                -2
                    => throw new InvalidOperationException($"Borrowing record with ID {borrowingId} has already been returned."),

                _
                    => throw new Exception("An unexpected error occurred while returning the borrowing record.")
            };
        }

        public async Task<int> GetTotalBorrowingsAsync()
        {

            int Results = await _borrowingDAL.GetTotalBorrowingsAsync();

            return Results;

        }
    }
}