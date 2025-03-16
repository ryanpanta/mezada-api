using FluentValidation;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.Task;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.TaskGroup;
using WebApiMezada.Services.TaskGroup.Validators;
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

//setting up user database settings
builder.Services.Configure<UserDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var userSettings = builder.Configuration.GetSection("Collections:Users").Get<UserDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.UserCollectionName = userSettings.UserCollectionName;
});

//setting up family group database settings
builder.Services.Configure<FamilyGroupDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var familyGroupSettings = builder.Configuration.GetSection("Collections:FamilyGroups").Get<FamilyGroupDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.FamilyGroupCollectionName = familyGroupSettings.FamilyGroupCollectionName;
});

// setting up tasks database settings
builder.Services.Configure<TaskDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var taskSettings = builder.Configuration.GetSection("Collections:Tasks").Get<TaskDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.TaskCollectionName = taskSettings.TaskCollectionName;
});

builder.Services.AddScoped<IValidator<TaskCreateDTO>, TaskCreateValidator>();
builder.Services.AddSingleton<UserRegisterValidator>();



builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IFamilyGroupService, FamilyGroupService>();

builder.Services.AddScoped<ITaskService, TaskService>();



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
