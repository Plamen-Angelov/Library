using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;

using API;

using SendGrid.Extensions.DependencyInjection;
using log4net.Config;
using API.Validation;
using Common.Models.InputDTOs;
using Common.Models.JWT;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Seeding;
using Repositories;
using Repositories.Interfaces;
using Services.EmailSender;
using Services.Interfaces;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => 
    { 
        builder.WithOrigins(configuration["CorsOrigins"].Split(";")).AllowAnyMethod().AllowAnyHeader(); 
    });
});

builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<UserEntity, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+!#$%&'*/=?^`{|}~";
        options.Password.RequiredLength = 10;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LibraryDbContext>()
    .AddDefaultTokenProviders();

var jwtTokenConfig = configuration.GetSection("jwtTokenConfig").Get<JwtTokenConfig>();
builder.Services.AddSingleton(jwtTokenConfig);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtTokenConfig.Issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
        ValidAudience = jwtTokenConfig.Audience,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddSingleton(x => new BlobServiceClient(configuration.GetValue<string>("AzureBlobStorageConnectionString")));

builder.Services.Configure<EmailSenderOptions>(options =>
{
    options.ApiKey = builder.Configuration["SendGrid:ApiKey"];
    options.SenderEmail = builder.Configuration["SendGrid:SenderEmail"];
    options.SenderName = builder.Configuration["SendGrid:SenderName"];
});

builder.Services.AddSendGrid(options =>
{
    options.ApiKey = builder.Configuration["SendGrid:ApiKey"];
});

builder.Services.AddControllers().AddFluentValidation();

builder.Environment.ConfigureLog4Net(builder.Configuration);
builder.Services.AddLogging(loggingBuilder => loggingBuilder
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddLog4Net());

//XmlConfigurator.Configure(new FileInfo("log4net.config"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IJwtAuthService, JwtAuthService>();
builder.Services.AddSingleton<IBlobService, BlobService>();
builder.Services.AddTransient<LibraryDbContextSeeder>();
builder.Services.AddTransient<IMailSender, EmailSender>();

builder.Services.AddTransient<IBookService, BookService>();
builder.Services.AddTransient<IGenreService, GenreService>();
builder.Services.AddTransient<IAuthorService, AuthorService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IBookReservationService, BookReservationService>();

builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IGenreRepository, GenreRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IAuthorRepository, AuthorRepository>();
builder.Services.AddTransient<IAuthorsBooksRepository, AuthorsBooksRepository>();
builder.Services.AddTransient<IGenresBooksRepository, GenresBooksRepository>();
builder.Services.AddTransient<IBookReservationRepository, BookReservationRepository>();

builder.Services.AddTransient<IValidator<RegisterUserDto>, RegisterUserValidator>();
builder.Services.AddTransient<IValidator<LoginUserDto>, LoginUserValidator>();
builder.Services.AddTransient<IValidator<Genre>, GenreValidator>();
builder.Services.AddTransient<IValidator<AuthorDto>, AuthorValidator>();
builder.Services.AddTransient<IValidator<AddBookDto>, AddBookValidator>();
builder.Services.AddTransient<IValidator<ForgotPasswordDto>, ForgotPasswordValidator>();
builder.Services.AddTransient<IValidator<ResetPasswordDto>, ResetPasswordValidator>();
builder.Services.AddTransient<IValidator<PaginatorInputDto>, PaginatorValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Library project",
        Description = "RilaLibrary",

    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


// Seed data on application startup
SeedData(app);

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

void SeedData(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (var serviceScope = scopedFactory.CreateScope())
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        dbContext.Database.Migrate();

        new LibraryDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
    }
}