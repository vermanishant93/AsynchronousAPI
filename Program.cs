using AsyncProductAPI.Data;
using AsyncProductAPI.Dtos;
using AsyncProductAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=RequestDB.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// Start Endpoint
app.MapPost("api/v1/products", async (AppDbContext context, ListingRequest listingRequest) =>{
    if(listingRequest == null)
        return Results.BadRequest();

    listingRequest.RequestStatus = "ACCEPT";
    listingRequest.EstimatedCompetionTime = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd:HH:mm:ss");

    await context.ListingRequests.AddAsync(listingRequest);
    await context.SaveChangesAsync();

    return Results.Accepted($"api/v1/productstatus/{listingRequest.RequestId}", listingRequest);

});

// Status endpoint

app.MapGet("api/v1/productstatus/{requestId}", (AppDbContext context, string requestId) => {
    var listingRequest = context.ListingRequests.FirstOrDefault(lr => lr.RequestId == requestId);

    if(listingRequest == null)
        return Results.NotFound();

    ListingStatus listingStatus = new ListingStatus{
        RequestStatus = listingRequest.RequestStatus,
        ResourceURL = String.Empty
    };

    if(listingRequest.RequestStatus!.ToUpper() == "COMPLETE")
    {
        listingStatus.ResourceURL = $"api/v1/products/{Guid.NewGuid().ToString()}";
        //return Results.Ok(listingStatus);

        return Results.Redirect("https://localhost:5001/" + listingStatus.ResourceURL);
    }

    listingStatus.EstimatedCompetionTime = DateTime.Now.AddMinutes(3).ToString("yyyy-MM-dd:HH:mm:ss");  
    return Results.Ok(listingStatus);

});

// Final Endpoint

app.MapGet("api/v1/products/{requestId}", (string requestId) =>
{
    return Results.Ok("This is where you would pass back the final result");
});

app.Run();

