using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalChatApplication.Data;
using MinimalChatApplication.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<RequestLoggingMiddleware>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



//configuring db path
builder.Services.AddDbContext<MinimalChatContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MinimalChatContext")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

ConfigurationManager Configuration = builder.Configuration;

// Add JWT authentication
// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens. TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = Configuration["JWT:ValidAudience"],
        ValidIssuer = Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<MinimalChatContext>();
    context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.UseMiddleware<RequestLoggingMiddleware>();



app.Run();
