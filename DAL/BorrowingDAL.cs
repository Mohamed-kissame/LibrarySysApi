using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class BorrowingDAL
    {
        private readonly string _connectionString;

        public BorrowingDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Borrowing>> GetAllBorrowingsAsync()
        {


            List<Borrowing> borrowings = new List<Borrowing>();

            try {


                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("sp_Borrowings_GetAll", connection);

                command.CommandType = CommandType.StoredProcedure;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving borrowings: " + ex.Message);

            }

            return borrowings;
        }

        public async Task<Borrowing?> GetBorrowingByIDAsync(int BorrowingID)
        {

            try
            {


                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Borrowings_GetById", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@BorrowinID", SqlDbType.Int).Value = BorrowingID;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if(await reader.ReadAsync())
                {
                    return MapReaderToBorrowing(reader);
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving borrowing by ID: " + ex.Message);
            }

            return null;
        }

        public async Task<List<Borrowing>> GetBorrowingsByMemberIDAsync(int MemberID)
        {
            List<Borrowing> borrowings = new List<Borrowing>();

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("sp_Borrowings_GetByMemberId", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@MemberID", SqlDbType.Int).Value = MemberID;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                     borrowings.Add(MapReaderToBorrowing(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving borrowings by member ID: " + ex.Message);
            }
            return borrowings;
        }

        public async Task<List<Borrowing>> GetBorrowingsByBookIDAsync(int BookID)
        {

            List<Borrowing> borrowings = new List<Borrowing>();


            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("sp_Borrowings_GetByBookId", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@BookID", SqlDbType.Int).Value = BookID;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                   borrowings.Add(MapReaderToBorrowing(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving borrowings by book ID: " + ex.Message);
            }
            return borrowings;
        }

        public async Task<(int ResultCode, Borrowing? Borrowing)> AddBorrowingAsync(int bookId, int memberId)
        {

            try
            {


                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Borrowings_Add", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@BookID", SqlDbType.Int).Value = bookId; 
                command.Parameters.Add("@MemberID", SqlDbType.Int).Value = memberId;

                SqlParameter NewBorrowingID = new SqlParameter("@NewBorrowingID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                SqlParameter ResultCode = new SqlParameter("@ResultCode", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(NewBorrowingID);
                command.Parameters.Add(ResultCode);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int resultCode = (int)ResultCode.Value;
                int newBorrowingID = (int)NewBorrowingID.Value;

                if(resultCode == 1 && newBorrowingID > 0)
                {
                    Borrowing? newBorrowing = await GetBorrowingByIDAsync(newBorrowingID);
                    return (resultCode, newBorrowing);
                }
                else
                {
                    return (resultCode, null);
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Error adding borrowing.", ex);
            }

        }


        public async Task<(int ResultCode, Borrowing? Borrowing)> ReturnBorrowingAsync(int borrowingId)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Borrowings_Return", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@BorrowingID", SqlDbType.Int).Value = borrowingId;

                SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(resultCodeParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int resultCode = (int)resultCodeParam.Value;

                if (resultCode == 1)
                {
                    Borrowing? updatedBorrowing = await GetBorrowingByIDAsync(borrowingId);
                    return (resultCode, updatedBorrowing);
                }

                return (resultCode, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error returning borrowing.", ex);
            }
        }

        private static Borrowing MapReaderToBorrowing(SqlDataReader reader)
        {
            return new Borrowing
            {
                BorrowingID = reader.GetInt32(reader.GetOrdinal("BorrowingID")),
                BookID = reader.GetInt32(reader.GetOrdinal("BookID")),
                BookTitle = reader.GetString(reader.GetOrdinal("BookTitle")),
                MemberID = reader.GetInt32(reader.GetOrdinal("MemberID")),
                MemberName = reader.GetString(reader.GetOrdinal("MemberName")),
                BorrowDate = reader.GetDateTime(reader.GetOrdinal("BorrowDate")),
                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}