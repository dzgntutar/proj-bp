
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using TT.Api.Data.Model;
using TT.Api.Mapping;
using TT.Api.Service;
using TT.Api.Validations;

//configuration by env 
string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var Configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped<IPostService, PostService>();

// add healthcheck service
builder.Services.AddHealthChecks();

//add configuration
builder.Configuration.AddConfiguration(Configuration);

//auto mapping
var mappingConfig = new MapperConfiguration(m => m.AddProfile(new AutoMappingProfile()));
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

//validation
builder.Services.AddTransient<IValidator<PostDto>, PostValidator>();

//logging
builder.Services.AddLogging(x =>
{
    x.ClearProviders(); 
    x.SetMinimumLevel(LogLevel.Debug);
    x.AddDebug();   
});

//builder.Host.ConfigureLogging(x => {
//    x.AddConsole();
//});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//healthCheck
app.UseHealthChecks("/api/healthcheck", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsync("Ok");
    }
});


app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
