using WebApiMezada.Configurations;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.User;
using WebApiMezada.Services.User.Validators;

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

builder.Services.Configure<FamilyGroupDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var userSettings = builder.Configuration.GetSection("Collections:FamilyGroups").Get<FamilyGroupDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.FamilyGroupCollectionName = userSettings.FamilyGroupCollectionName;
});

builder.Services.AddSingleton<UserRegisterValidator>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IFamilyGroupService, FamilyGroupService>();


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


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); 
}

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.Run();
