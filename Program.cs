using FluentValidation;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.Suggestion;
using WebApiMezada.DTOs.Task;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.FamilyGroup.Validators;
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

//setting up task assignment database settings
builder.Services.Configure<TaskAssignmentDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var taskAssignmentSettings = builder.Configuration.GetSection("Collections:TaskAssignments").Get<TaskAssignmentDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.TaskAssignmentCollectionName = taskAssignmentSettings.TaskAssignmentCollectionName;
});

//settings up suggestions database settings
builder.Services.Configure<SuggestionDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var suggestionSettings = builder.Configuration.GetSection("Collections:Suggestions").Get<SuggestionDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.SuggestionCollectionName = suggestionSettings.SuggestionCollectionName;
});

//setting up task history database settings
builder.Services.Configure<TaskHistoryDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var taskHistorySettings = builder.Configuration.GetSection("Collections:TaskHistories").Get<TaskHistoryDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.TaskHistoryCollectionName = taskHistorySettings.TaskHistoryCollectionName;
});

//setting up cycle database settings
builder.Services.Configure<CycleDatabaseSettings>(options =>
{
    var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    var cycleSettings = builder.Configuration.GetSection("Collections:Cycles").Get<CycleDatabaseSettings>();
    options.ConnectionString = dbSettings.ConnectionString;
    options.DatabaseName = dbSettings.DatabaseName;
    options.CycleCollectionName = cycleSettings.CycleCollectionName;
});


builder.Services.AddScoped<IValidator<TaskCreateDTO>, TaskCreateValidator>();
builder.Services.AddScoped<IValidator<SuggestionCreateDTO>, SuggestionCreateValidator>();
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
