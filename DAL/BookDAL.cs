using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class BookDAL
    {


        private readonly string _connectionString;

        public BookDAL(string connectionString)
        {

            _connectionString = connectionString;

        }


        public async Task<List<Book>> GetAllBooksAsync()
        {

            List<Book> books = new List<Book>();



            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Books_GetAll", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {

                     books.Add(MapToBook(reader));

                }



            } catch (Exception ex)
            {
                
                throw new Exception("An error occurred while retrieving books.", ex);
            }

            return books;


        }


        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            Book? book = null;

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Books_GetById", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@BookID", bookId);

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    book = MapToBook(reader);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the book with ID {bookId}.", ex);
            }
            return book;
        }


        public async Task<Book?> AddNewBookAsync(string Title , string Author ,string ISBN , int TotalCopies)
        {


            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Books_Add", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar, 200).Value = Title;
                command.Parameters.Add("@Author", System.Data.SqlDbType.NVarChar, 100).Value = Author;
                command.Parameters.Add("@ISBN", System.Data.SqlDbType.NVarChar, 30).Value = ISBN;
                command.Parameters.Add("@TotalCopies", System.Data.SqlDbType.Int).Value = TotalCopies;

                SqlParameter NewBookID = new SqlParameter
                {
                    ParameterName = "@NewBookID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(NewBookID);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int newBookId = (int)NewBookID.Value;

                if (newBookId > 0)
                {
                    return await GetBookByIdAsync(newBookId);
                }
               
                return null;

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the book. Details: {ex.Message}", ex);
            }


        }

        public async Task<bool> ISBNExistAsync(string ISBN)
        {
            bool IsExist = false;

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("sp_Books_ISBNExists", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@ISBN", System.Data.SqlDbType.NVarChar, 30).Value = ISBN;

                SqlParameter IsbnIsExisting = new SqlParameter
                {
                    ParameterName = "@IsExisting",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(IsbnIsExisting);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                IsExist = (bool)IsbnIsExisting.Value;


            }
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while checking the ISBN existence. Details: {ex.Message}", ex);
            }



            return IsExist;

        }

        public async Task<Book?> UpdateBookAsync(int BookID, string Title, string Author, string ISBN)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("Sp_Update_Books", connection);


                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@BookID", System.Data.SqlDbType.Int).Value = BookID;
                command.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar, 200).Value = Title;
                command.Parameters.Add("@Author", System.Data.SqlDbType.NVarChar, 100).Value = Author;
                command.Parameters.Add("@ISBN", System.Data.SqlDbType.NVarChar, 30).Value = ISBN;

                SqlParameter RowAffected = new SqlParameter
                {
                    ParameterName = "@IsUpdated",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(RowAffected);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                bool isUpdated = (int)RowAffected.Value > 0;

                if (isUpdated)
                {
                    return await GetBookByIdAsync(BookID);

                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the book with ID {BookID}. Details: {ex.Message}", ex);
            }
        }


        public async Task<int> DeleteBookAsync(int BookID)
        {

            try
            {


                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("Sp_Books_Delete", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@BookID", System.Data.SqlDbType.Int).Value = BookID;

                SqlParameter resultcode = new SqlParameter
                {
                    ParameterName = "@ResultCode",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(resultcode);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int ResultCode = (int)resultcode.Value;


                return ResultCode;

            }
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while deleting the book with ID {BookID}. Details: {ex.Message}", ex);
            }

            

        }


        public async Task<bool> BookExistsByIdAsync(int bookId)
        {

            bool isExist = false;

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Books_ExistsById", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@BookID", System.Data.SqlDbType.Int).Value = bookId;

                SqlParameter BookExists = new SqlParameter
                {
                    ParameterName = "@Exists",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(BookExists);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                isExist = (bool)BookExists.Value;

            }
            catch (Exception ex)
            {

                throw new Exception($"An error occurred while checking the existence of the book with ID {bookId}. Details: {ex.Message}", ex);

            }


            return isExist;
        }

        private static Book MapToBook(SqlDataReader reader)
        {
            return new Book
            {
                BookID = reader.GetInt32(reader.GetOrdinal("BookID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Author = reader.GetString(reader.GetOrdinal("Author")),
                ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                TotalCopies = reader.GetInt32(reader.GetOrdinal("TotalCopies")),
                AvailableCopies = reader.GetInt32(reader.GetOrdinal("AvailableCopies")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };


        }
    }
}
