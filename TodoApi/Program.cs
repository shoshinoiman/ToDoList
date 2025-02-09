using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>(options =>
options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
new MySqlServerVersion(new Version(8, 0, 0))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // מאפשר כל מקור
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// app.UseCors("AllowAnyOrigin");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapGet("/items", async (ToDoDbContext context) =>
{
    try
    {
        if (context == null) return Results.Problem("Database context is unavailable.");
        var items = await context.Items.AsNoTracking().ToListAsync();
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error fetching items: {ex.Message}");
    }
});


app.MapPost("/items", async ([FromBody] Item item, ToDoDbContext context) =>
{
    await context.Items.AddAsync(item);
    await context.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapPut("/items/{id}", async (int id, bool iscomplete, ToDoDbContext context) =>
{
    var tmp = await context.Items.FirstOrDefaultAsync(c => c.Id == id);
    if (tmp is null)
        return Results.NotFound($"Itemm with id :{id} not found");
    tmp.IsComplete = iscomplete;
    await context.SaveChangesAsync();
    return Results.Ok(tmp);
}
);

app.MapDelete("/items/{id}", async (int id, ToDoDbContext context) =>
{
    var item = await context.Items.FirstOrDefaultAsync(c => c.Id == id);
    if (item is null)
        return Results.NotFound($"Itemm with id :{id} not found");
    context.Remove(item);
    await context.SaveChangesAsync();
    return Results.Ok(item);
}
);
builder.WebHost.UseUrls("https://localhost:7103", "http://localhost:5126");
app.Run();