## Introduction

### Getting started

These instructions are aimed at helping you set up the project with xAuth. Sample.md will
provide you with installation steps and instruction on how to activate the package.

## Prerequisites

The following tools are required for the API to function please make sure that necessary tools
are installed and if not, install them utilizing the provider's main pages.

- [.Net Core](https://dotnet.microsoft.com/download/dotnet-core/3.0) - dotnet core 3 and greater

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

And now add the following code to startup.

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

### Install the database

For xAuth to function properly you will need to add or make sure that the database contains
necessary functions,procedures and tables with correct collumns. All this information
can be found in github, please follow the the link bellow.

Link: https://github.com/Xeroxcore/xAuth/blob/master/script/auth.sql
**Important**
The SQL script is made for postgres and will require some alterations to function on
on alternative SQL Databases.

### Tokenkey Authentication

### User Authentication

### Using RefreshToken

### Adding and removing roles
