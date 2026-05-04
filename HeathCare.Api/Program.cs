using HeathCare.Api.Extensions;
using HeathCare.Api.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();

        });


    }

);



builder.Services.AddCore(builder.Configuration);

var app = builder.Build();




app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "HealthCare API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}


app.UseCors(("AllowFrontend"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();