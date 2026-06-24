using Microsoft.EntityFrameworkCore;          
using WorkTrackPro.API.Data;                 
using Microsoft.AspNetCore.Authentication.JwtBearer;   
using Microsoft.IdentityModel.Tokens;           
using System.Text;                              
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddDbContext<AppDbContext>(options =>    
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));    
// Add services to the container.

builder.Services.AddControllers();
Console.WriteLine("JWT KEY: " + builder.Configuration["Jwt:Key"]);
var keyString = builder.Configuration["Jwt:Key"];    

if (string.IsNullOrEmpty(keyString))             
{
    throw new Exception("JWT Key missing in appsettings.json");        
}

var key = Encoding.UTF8.GetBytes(keyString);             //--//

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)   //--//
    .AddJwtBearer(options =>                                                     //--//
    {
         options.RequireHttpsMetadata = false;                                //--//
        options.SaveToken = true;                                                //--//
        options.TokenValidationParameters = new TokenValidationParameters          //--//
        {
            ValidateIssuer = true,              //--//
            ValidateAudience = true,             //--//
            ValidateLifetime = true,               //--//
            ValidateIssuerSigningKey = true,         //--//
            ValidIssuer = builder.Configuration["Jwt:Issuer"],       //--//
            ValidAudience = builder.Configuration["Jwt:Audience"],         //--//
            IssuerSigningKey = new SymmetricSecurityKey(key)                //--//
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle   
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>     //--//
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token.\nExample: Bearer abc123"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseCors("AllowAngular");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();   //--//

app.UseAuthorization();

app.MapControllers();

app.Run();
