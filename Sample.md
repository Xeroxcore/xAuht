## Introduction

### Getting started

These instructions are aimed at helping you set up the project with xAuth. Sample.md will
provide you with installation steps and instruction on how to activate the package.

## Prerequisites

The following tools are required for the API to function please make sure that necessary tools
are installed and if not, install them utilizing the provider's main pages.

- [.Net Core](https://dotnet.microsoft.com/download/dotnet-core/3.0) - dotnet core 3 and greater
- [Asp .Net Core JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/) - JWTBearer
- [xSql](https://www.nuget.org/packages/xSql/) - xSql

## Installation

Installing the package is very straigh forward. and you wont need to do more than to
type the following commands in your command terminal.

```
1. redirect to the folder where your csporj file is located.
2. run the following command <dotnet add package xAuth>.
```

### Registering the package in your application

1. Install Package

```
1. open the terminal in your project where the csproj file is located.
2. Run this command > dotnet add package xAuth
```

2. Register JWT to the project by adding the following code to your startup file

Prerequisites
if your havent arealy install JWTBerer in your project then please execute the
following command.

```
1. open the terminal in your project where the csproj file is located.
2. Run this command > dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

And now add the following code to startup. please take note that this section only
shows how to get the authentication working. Under no cercumstances should this
be seen as secure code.

```csharp

 services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                var secretKey = "Yoursupermegasecretkey"
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
```

Register token as scoped or singleton

```csharp
            Services.AddScoped<IJwtGenerator, JwtGenerator>(ServiceProvider =>
           { return new JwtGenerator("Yoursupermegasecretkey", "HS256"); }); // this secretkey must match your secretkey at addJwtBearer

            Services.AddScoped<ISqlHelper, NpgSql>(ServiceProvider =>
            {
                return new NpgSql("your database connectionstring");
            });

            Services.AddScoped<TokenAuth>();
            Services.AddScoped<UserAuth>();
```

### Install the database

For xAuth to function properly you will need to add or make sure that the database contains
necessary functions,procedures and tables with correct collumns. All this information
can be found in github, please follow the the link bellow.

Link: https://github.com/Xeroxcore/xAuth/blob/master/script/auth.sql
**Important**
The SQL script is made for postgres and will require some alterations to function on
on alternative SQL Databases.

### Tokenkey Authentication

Sample Controller

```csharp
   [ApiController]
    [Route("[controller]")]
    public class ApiAuthController : ControllerBase
    {
        private IAuth Auth { get; }
        public ApiAuthController(TokenAuth auth)
            => Auth = auth;

        [HttpPost]
        public ActionResult Post([FromBody] TokenKey accessKey)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    if (ModelState.IsValid)
                    {
                        var result = Auth.Authentiacte(accessKey, "token", "localhost", null);
                        return Ok(result);
                    }
                    return Unauthorized();
                }
                return Unauthorized();
            }
            catch
            {
                return Unauthorized();
            }
        }
    }
```

Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1ODU5OTMyMjQsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6InRva2VuIn0.T6wHg3tte-EgmfhWdeLLNaHspjleiL68uzkPgMOQLIg",
  "tokenType": "Bearer",
  "expiration": "UTC2020-04-04 9:40:24 AM",
  "refreshToken": "+OAzOTJvHIz4RM5fDsD6d/TfSsRUywOpKGzPJJLO5Mo="
}
```

### User Authentication

Sample controller

```csharp
  [ApiController]
    [Route("[controller]")]
    public class UserAuthController : ControllerBase
    {
        private IAuth Auth { get; }
        public UserAuthController(UserAuth auth)
            => Auth = auth;

        [HttpPost]
        public IActionResult Post([FromBody] UserAccount userAccount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = Auth.Authentiacte(userAccount, "user", "localhost", null);
                    return Ok(result);
                }
                return Unauthorized();
            }
            catch
            {
                return Unauthorized();
            }
        }
    }
```

Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1ODU5OTMyMjQsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6InRva2VuIn0.T6wHg3tte-EgmfhWdeLLNaHspjleiL68uzkPgMOQLIg",
  "tokenType": "Bearer",
  "expiration": "UTC2020-04-04 9:40:24 AM",
  "refreshToken": "+OAzOTJvHIz4RM5fDsD6d/TfSsRUywOpKGzPJJLO5Mo="
}
```

### Using RefreshToken

Sample Controller

```csharp
     [HttpPost]
        [Route("[controller]/RefreshToken")]
        public ActionResult RefreshToken([FromBody] RefreshToken token)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (ModelState.IsValid)
                    {
                        var result = Auth.RefreshToken(token.Token, "token", "localhost", null);
                        return Ok(result);
                    }
                    return Unauthorized();
                }
                return Unauthorized();
            }
            catch
            {
                return Unauthorized();
            }
        }
```

Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1ODU5OTMyMjQsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6InRva2VuIn0.T6wHg3tte-EgmfhWdeLLNaHspjleiL68uzkPgMOQLIg",
  "tokenType": "Bearer",
  "expiration": "UTC2020-04-04 9:40:24 AM",
  "refreshToken": "+OAzOTJvHIz4RM5fDsD6d/TfSsRUywOpKGzPJJLO5Mo="
}
```
