var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add logging configuration (optional customization)
//builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug();   // Add debug logging
builder.Logging.AddEventSourceLogger(); // Add event source logging for Windows Event Log

var app = builder.Build();

// Serve static files (like images) from the wwwroot folder
app.UseStaticFiles();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
