using BLL;
using DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found.");

builder.Services.AddScoped<BookDAL>(_ => new BookDAL(connectionString));
builder.Services.AddScoped<BookService>();

builder.Services.AddScoped<MemberDAL>(_ => new MemberDAL(connectionString));
builder.Services.AddScoped<MemberService>();

builder.Services.AddScoped<BorrowingDAL>(_ => new BorrowingDAL(connectionString));
builder.Services.AddScoped<BorrowingService>();


builder.Services.AddScoped<UserDAL>(_ => new UserDAL(connectionString));
builder.Services.AddScoped<AuthService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("LibrarySysApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7154",
                "http://localhost:5141"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors("LibrarySysApiCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
