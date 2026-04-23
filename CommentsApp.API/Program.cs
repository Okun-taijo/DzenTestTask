using CommentsApp.API.Hubs;
using CommentsApp.API.GraphQL;
using CommentsApp.Application.Interfaces;
using CommentsApp.Application.Services;
using CommentsApp.Infrastructure;
using CommentsApp.Infrastructure.Messaging;
using CommentsApp.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IRealtimeNotifier, RealTimeNotifier>();

builder.Services.AddSingleton<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ITextSanitizer, SanitizerService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var redisConn = cfg.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(redisConn);
});
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddSingleton<IMessageBus, RabbitMqBus>();
builder.Services.AddSingleton<IElasticService, ElasticService>();

builder.Services.AddHostedService<CommentWorker>();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<CommentsQuery>();


builder.Services.AddValidatorsFromAssemblyContaining<CreateCommentDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:5173"];

    options.AddPolicy("frontend",
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddProblemDetails();



var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
    await elastic.InitAsync();
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = error?.Error.Message,
            stack = error?.Error.StackTrace
        }));
    });
});
app.UseRouting();
app.UseCors("frontend");

var uploadRelativePath = builder.Configuration["FileStorage:UploadPath"];

var uploadPath = System.IO.Path.Combine(
    builder.Environment.ContentRootPath,
    uploadRelativePath!);

if (!Directory.Exists(uploadPath))
    Directory.CreateDirectory(uploadPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});
app.MapControllers();
app.MapHub<CommentHub>("/hubs/comments");
app.MapGraphQL("/graphql");

app.UseStatusCodePages();


app.Run();