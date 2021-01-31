
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

- For API Document please see in **Swagger** (http://localhost:44345/swagger)
- This Framework also have **HATEOAS!!!**. Journey start with http://localhost:44345
- Paging Syntax: `...?limit=25&offset=25`
	- Default paging is config in code
- Sort Syntax: `...?orderBy=email` or `orderBy=email desc`
	- Default sort is config in code
	- Sortable value please check in HATEOAS!!!
- Search Syntax: 
	- Single Search: `?search=email sw value,` 
	- Multiple or Search between : `?search=createdAt gt 2020-07-15&search=createdAt lt 2021-07-16`
	- Searchable value and pattern please check in HATEOAS!!!

### Authorization 
- Resource Owner Password Credentials flow
- Token: **Bearer**


### Bonus
Google Map API Get Place.
- Configurations: 
	- Update your google key in `appsettings.json` value `"GoogleAPIKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxx",`
	- Currently we get result type **locality** and **street_address** you can modify this type in `appsettings.json` value `"GoogleAPIResultType": "locality|street_address",` list each type separate with `|`
	- Currently I use server cache to reduce number of API call. `[ResponseCache(CacheProfileName = "Static")]`
	- TODO:             
			1. After call Google API, store result in Database.
			2. Before call Google API, check if data already have in Database, use data in Database instead.
			3. Create and a new method to refresh Place Data
                       (Remove Place in Database and call Google API for to generate new place)
- If the the API Key is correct the result of  `latitude: 13.721601174184647, longitude: 100.55825338870659` will be
```json
{
    "value": [
        {
            "createdAt": "2021-01-31T03:14:22.9415634+00:00",
            "place_id": "ChIJFx4urxCf4jARPJagAAXSVhM",
            "type": "street_address",
            "formatted_address": "492/34 Rama IV Rd, Khwaeng Khlong Toei, Khet Khlong Toei, Krung Thep Maha Nakhon 10110, Thailand"
        },
        {
            "createdAt": "2021-01-31T03:14:22.9437902+00:00",
            "place_id": "ChIJ82ENKDJgHTERIEjiXbIAAQE",
            "type": "locality",
            "formatted_address": "Bangkok, Thailand"
        }
    ]
}
```

