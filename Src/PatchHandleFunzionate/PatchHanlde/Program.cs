using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
    //config => { 
        
    //    //config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());   
    
    //}
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//builder.Services.ConfigureOptions<PatchHanlde.JsonPatch.PatchJsonInputFormatterConfiguration>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



//NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() => 
//    new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson().Services.BuildServiceProvider()
//    .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First();