using APIPrinter.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona a configuração de CORS
builder.Services.AddCors(options =>
{
    // Adiciona a política "AllowSpecificOrigin" que permite apenas a origem http://localhost:8080
    //options.AddPolicy("AllowSpecificOrigin",

    // Adiciona a política "AllowAll" que permite qualquer origem, método e cabeçalho
    options.AddPolicy("AllowAll",
        policy =>
        {
            // Adiciona a origem http://localhost:8080
            //policy.WithOrigins("http://localhost:8080")
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddTransient<Global>();

// Add services to the container.

builder.Services.AddControllers();
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

// Adiciona o middleware de CORS antes do middleware de autorização
app.UseCors("AllowAll");

//caso seje para permitir apenas um dominio especifico
//app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
