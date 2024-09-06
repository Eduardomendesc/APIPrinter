using APIPrinter.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona a configura��o de CORS
builder.Services.AddCors(options =>
{
    // Adiciona a pol�tica "AllowSpecificOrigin" que permite apenas a origem http://localhost:8080
    //options.AddPolicy("AllowSpecificOrigin",

    // Adiciona a pol�tica "AllowAll" que permite qualquer origem, m�todo e cabe�alho
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

// Adiciona o middleware de CORS antes do middleware de autoriza��o
app.UseCors("AllowAll");

//caso seje para permitir apenas um dominio especifico
//app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
