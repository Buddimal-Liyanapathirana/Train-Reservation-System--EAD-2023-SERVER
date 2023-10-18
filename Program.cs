using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDotnetDemo.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings:SecretKey").Value!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Define policies for roles
    options.AddPolicy("BACK_OFFICER", policy => policy.RequireRole("BACK_OFFICER"));
    options.AddPolicy("TRAVEL_AGENT", policy => policy.RequireRole("TRAVEL_AGENT"));
    options.AddPolicy("TRAVELER", policy => policy.RequireRole("TRAVELER"));
});


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

//resolving the RoutesService
builder.Services.AddTransient<IRouteService, RouteService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(options => options
    //.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5000", "https://travelermate.netlify.app") // Add the origins you need
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .AllowAnyOrigin()
);


app.MapControllers();

app.Run();
