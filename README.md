
# VehiclesTrackingAPI 

**REST API design with ASP.NET Core using .net 5**

### Run the project

1. Build the solution using Visual Studio, or on the [command line](https://www.microsoft.com/net/core) with `dotnet build`.
2. Run the project in Visual Studio or `dotnet run`. The API will start up on `http://localhost:44345`
3. Use an HTTP client like [Postman](https://www.getpostman.com/) or [Fiddler](https://www.telerik.com/download/fiddler) to `GET http://localhost:44345`.
4. Swagger support http://localhost:44345/swagger
5. **HATEOAS!!!**

### Database
- Entity Framework Core in-memory (No need additional Set Up)
- Can change to Entity Framework Core SQLServer for production `Microsoft.EntityFrameworkCore.SqlServer`
	- Update connection string in `appsettings.json`
	- `add-migration InitDatabase` Create class for migration to database
	- `update-database`Apply change to database.
- Can also change to Entity Framework Core Cosmos DB for production `Microsoft.EntityFrameworkCore.Cosmos`


### Dependencies
- **AutoMapper**: For auto map model and entity. Easy for scale the project in a fast pace.
	- `AutoMapper`
	- `AutoMapper.Extensions.Microsoft.DependencyInjection`
- **EntityFrameworkCore**: For working with Database
	- `Microsoft.EntityFrameworkCore.InMemory`
	- `Microsoft.EntityFrameworkCore.SqlServer`
	- `Microsoft.EntityFrameworkCore.Tools`
- **OpenIddict**: For Authentication and Authorization
	- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
	- `OpenIddict.AspNetCore`
	- `OpenIddict.Server`
	- `OpenIddict.EntityFrameworkCore`
- **NetTopologySuite**: For working with geometry
	- `NetTopologySuite`
	- `Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite`
- **NSwag**: Auto generate API document and API definitions
	- `NSwag.AspNetCore`
- **JSON**: JSON Requirement
	- `Newtonsoft.Json`
	- `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
- **Versioning**: For API Versioning in Header
	- `Microsoft.AspNetCore.Mvc.Versioning`

### API Document

For API Document please see in **Swagger** (http://localhost:44345/swagger)
This Framework also have **HATEOAS!!!**. Journey start with http://localhost:44345


### Authorization 
- Resource Owner Password Credentials flow
- Token: **Bearer**



