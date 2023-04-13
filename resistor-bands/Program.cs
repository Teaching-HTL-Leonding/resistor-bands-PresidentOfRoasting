using System.Collections.Concurrent;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var colors = new ConcurrentDictionary<string, ColorDetail>()
{
    ["black"] = new(0, 1, 0),
    ["brown"] = new(1, 10, 1),
    ["red"] = new(2, 100, 2),
    ["orange"] = new(3, 1_000, 0),
    ["yellow"] = new(4, 10_000, 0),
    ["green"] = new(5, 100_000, 0.5),
    ["blue"] = new(6, 1_000_000, 0.25),
    ["violet"] = new(7, 10_000_000, 0.1),
    ["grey"] = new(8, 100_000_000, 0.05),
    ["white"] = new(9, 1_000_000_000, 0),
    ["gold"] = new(0, 0.1, 5),
    ["silver"] = new(0, 0.01, 10),
};

app.MapGet("/colors", () => colors.Keys).WithOpenApi(o =>
{
    o.Summary = "Return all colors for bands on resistors";
    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description = "A list of all colors";
    return o;
});

app.MapGet("/colors/{color}", (string color) => 
{
    if (!colors.ContainsKey(color)){
        return Results.NotFound();
    }
    ColorDetail result = colors[color];

    return Results.Ok(result);
})
.Produces<ColorDetail>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithOpenApi(o =>
{
    o.Summary = "Return details for a color band";
    o.Parameters[0].Description = "Color for which to get details";

    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description = "Details for a color band";
    o.Responses[((int)StatusCodes.Status404NotFound).ToString()].Description = "Unknown color";

    return o;
});

app.MapPost("/resistors/value-from-bands", (BandsDetail bands) => {
    if(!colors.ContainsKey(bands.FirstBand) || !colors.ContainsKey(bands.SecondBand) || colors.ContainsKey(bands.ThirdBand)){
        return Results.BadRequest();
    }
    string num = colors[bands.FirstBand].Value.ToString() + colors[bands.SecondBand].Value.ToString() +colors[bands.ThirdBand].Value.ToString();

    int multNum = Convert.ToInt32(num);

    double sum = multNum * colors[bands.Multiplier].Multiplier;

    return Results.Ok(new ReturnValue(sum, colors[bands.Tolerance].Tolerance));
    
})
.Produces<ReturnValue>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(o =>
{
    o.Summary = "Calculates the resistor value based on given color bands (using POST).";

    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description = "Resistor value could be decoded correctly";
    o.Responses[((int)StatusCodes.Status400BadRequest).ToString()].Description = "The request body contains invalid data";

    return o;
});

app.MapGet("/resistors/value-from-bands", (string firstBand, string secondBand, string thirdBand, string multiplier, string tolerance) => 
{
    if (!colors.ContainsKey(firstBand) || !colors.ContainsKey(secondBand) || !colors.ContainsKey(thirdBand)){
        return Results.BadRequest();
    }
    string num = colors[firstBand].Value.ToString() + colors[secondBand].Value.ToString() +colors[thirdBand].Value.ToString();

    int multNum = Convert.ToInt32(num);

    double sum = multNum * colors[multiplier].Multiplier;

    return Results.Ok(new ReturnValue(sum, colors[tolerance].Tolerance));
})
.Produces<ReturnValue>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(o =>
{
    o.Summary = "Calculates the resistor value based on given color bands (using GET).";

    o.Responses[((int)StatusCodes.Status200OK).ToString()].Description = "Resistor value could be decoded correctly";
    o.Responses[((int)StatusCodes.Status400BadRequest).ToString()].Description = "The request body contains invalid data";

    return o;
});

app.Run();

record ColorDetail (int Value, double Multiplier, double Tolerance);

record BandsDetail (string FirstBand, string SecondBand, string ThirdBand, string Multiplier, string Tolerance);
record ReturnValue (double resistorValue, double Tolerance);
record BandsFromValue(double ResistorValue, double Tolerance, double NumberOfBands);