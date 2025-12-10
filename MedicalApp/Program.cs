using MedicalApp.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddSecurity(builder.Configuration)
    .AddApplication()
    .AddPresentation(); 


var app = builder.Build();

app.UseInfrastructure();
app.MapAllEndpoints(); 

app.Run();