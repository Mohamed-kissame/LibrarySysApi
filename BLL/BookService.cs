using DAL;
using Models;

namespace BLL
{
    public class BookService
    {

        private readonly BookDAL _bookDAL;

        public BookService(BookDAL bookDAL)
        {
            _bookDAL = bookDAL;
        }

        private bool IsValidTitle(string title)
        {
            return !string.IsNullOrWhiteSpace(title) && title.Length <= 200;
        }

        private bool IsValidAuthor(string author)
        {
            return !string.IsNullOrWhiteSpace(author) && author.Length <= 100;
        }

        private bool IsValidISBN(string isbn)
        {
            return !string.IsNullOrWhiteSpace(isbn) && isbn.Length <= 30;
        }

        private bool IsValidTotalCopies(int totalCopies)
        {
            return totalCopies >= 1;
        }

        private async Task<bool> CheckISBNExistAsync(string ISBN)
        {
            return await _bookDAL.ISBNExistAsync(ISBN);
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _bookDAL.GetAllBooksAsync();
        }


        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await _bookDAL.GetBookByIdAsync(bookId);
        }


        public async Task<Book> AddNewBookAsync(string Title, string Author, string ISBN, int TotalCopies)
        {

            if (!IsValidTitle(Title))
            {
                throw new ArgumentException("Invalid title. Title must be non-empty and up to 200 characters.");
            }
            if (!IsValidAuthor(Author))
            {
                throw new ArgumentException("Invalid author. Author must be non-empty and up to 100 characters.");
            }

            if (!IsValidISBN(ISBN))
            {
                throw new ArgumentException("Invalid ISBN. ISBN must be non-empty and up to 30 characters");
            }
            if (!IsValidTotalCopies(TotalCopies))
            {
                throw new ArgumentException("Invalid total copies. Total copies must be at least 1.");
            }

            if (await CheckISBNExistAsync(ISBN))
            {
                throw new InvalidOperationException("A book with the same ISBN already exists.");
            }

            Book? book = await _bookDAL.AddNewBookAsync(Title, Author, ISBN, TotalCopies);

            if (book == null)
            {
                throw new Exception("Failed to add the new book.");

            }

            return book;

        }

        public async Task<Book> UpdateBookAsync(int BookID , string Title, string Author, string ISBN)
        {
            if (!IsValidTitle(Title))
            {
                throw new ArgumentException("Invalid title. Title must be non-empty and up to 200 characters.");
            }
            if (!IsValidAuthor(Author))
            {
                throw new ArgumentException("Invalid author. Author must be non-empty and up to 100 characters.");
            }
            if (!IsValidISBN(ISBN))
            {
                throw new ArgumentException("Invalid ISBN. ISBN must be non-empty and up to 30 characters");

            }

            Book? existingBook = await _bookDAL.GetBookByIdAsync(BookID);

            if(existingBook == null)
            {
                throw new KeyNotFoundException($"Book with ID {BookID} was not found.");
            }

            bool isISBNChanged = !string.Equals(existingBook.ISBN, ISBN, StringComparison.OrdinalIgnoreCase);



            if (isISBNChanged && await CheckISBNExistAsync(ISBN))
            {
                throw new InvalidOperationException("A book with the same ISBN already exists.");
            }

            Book? book = await _bookDAL.UpdateBookAsync(BookID, Title, Author, ISBN);

            if (book == null)
            {
                throw new Exception("Failed to update the book.");
            }
            return book;
        }

        public async Task<bool> DeleteBookAsync(int bookId)
        {

            if(bookId <= 0)
            {
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));
            }

            int result = await _bookDAL.DeleteBookAsync(bookId);


            return result switch
            {
                -1 => throw new KeyNotFoundException($"Book with ID {bookId} was not found."),
                1 => true,
                -2 => throw new InvalidOperationException("Cannot delete the book because it has active borrowings."),
                _ => throw new Exception("Failed to delete the book."),
            };


        }
    }
}