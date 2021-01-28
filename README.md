# VehiclesTrackingAPI 

**REST API design with ASP.NET Core using .net 5**

## Run the project

1. Build the solution using Visual Studio, or on the [command line](https://www.microsoft.com/net/core) with `dotnet build`.
2. Run the project in Visual Studio or `dotnet run`. The API will start up on http://localhost:50647
3. Use an HTTP client like [Postman](https://www.getpostman.com/) or [Fiddler](https://www.telerik.com/download/fiddler) to `GET http://localhost:50647`.
4. Swagger support http://localhost:50647/swagger
5. HATEOAS

### Database
- Entity Framework Core in-memory (No need additional Set Up)
- Can change to Entity Framework Core SQLServer for production `Microsoft.EntityFrameworkCore.SqlServer`
- Can also change to Entity Framework Core Cosmos DB for production `Microsoft.EntityFrameworkCore.Cosmos`


### Dependencies
- **AutoMapper**: For auto map model and entity. Easy for scale the project in a fast pace.
	- `AutoMapper`
	- `AutoMapper.Extensions.Microsoft.DependencyInjection`
- **NSwag**: Auto generate API document and API definitions
	- `NSwag.AspNetCore`
