using MongoDotnetDemo.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<DatabaseSettings>(
     builder.Configuration.GetSection("MyDb")
    );

//resolving the UserService
builder.Services.AddTransient<IUserService,UserService>();

//resolving the ScheduleService
builder.Services.AddTransient<IScheduleService, ScheduleService>();

//resolving the TrainService
builder.Services.AddTransient<ITrainService, TrainService>();

//resolving the TrainService
builder.Services.AddTransient<IReservationService, ReservationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
