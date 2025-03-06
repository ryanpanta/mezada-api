using WebApiMezada.Configurations;
using WebApiMezada.Services.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.Configure<UserDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var userSettings = builder.Configuration.GetSection("Collections:Users").Get<UserDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.UserCollectionName = userSettings.UserCollectionName;
});

builder.Services.AddSingleton<UserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); // Só usa HTTPS fora de desenvolvimento
}

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.Run();
