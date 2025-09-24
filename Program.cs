using LoginAPI.Context;
using LoginAPI.Model;
using LoginAPI.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.OpenApi;
using static LoginAPI.Model.Dto.AuthDto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDb>(o => 
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<JwtService>();

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = "app",
            ValidateAudience = true,
            ValidAudience = "app",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Login API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert only token"
    };

    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p =>
        p.WithOrigins("https://web-bhvd.onrender.com")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterReq req, AppDb db) =>
{
    if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username/Password required");

    if (req.Password != req.ConfirmPassword)
        return Results.BadRequest("Password and Confirm Password must match");

    var exists = await db.Users.AnyAsync(u => u.UserName == req.UserName);
    if (exists) return Results.Conflict("Username already exists");

    var hash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12);
    db.Users.Add(new User { UserName = req.UserName, Password = hash });
    await db.SaveChangesAsync();
    return Results.Ok("Resigter already");
});

app.MapPost("/api/auth/login", async (LoginReq req, AppDb db, JwtService jwt) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == req.UserName);
    if (user is null) return Results.Unauthorized();

    var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.Password);
    if (!ok) return Results.Unauthorized();

    var token = jwt.CreateToken(user.UserName);
    return Results.Ok(new AuthRes(token, user.UserName));
});

app.MapGet("/api/auth/me", async (AppDb db, HttpContext ctx) =>
{
    var username =
        ctx.User.FindFirstValue(ClaimTypes.Name) ??
        ctx.User.FindFirstValue("sub");

    if (string.IsNullOrWhiteSpace(username)) return Results.Unauthorized();

    var user = await db.Users.AsNoTracking()
        .FirstOrDefaultAsync(u => u.UserName == username);

    if (user is null) return Results.Unauthorized();

    return Results.Ok(new { user.UserName, user.CreateAt });
})
.RequireAuthorization()
.WithOpenApi(op =>
{
    op.Security = new List<OpenApiSecurityRequirement> {
        new() {
            [ new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            } ] = new List<string>()
        }
    };
    return op;
});

app.MapGet("/ping", () => "pong");

app.Run();
