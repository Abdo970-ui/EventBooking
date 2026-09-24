#!/usr/bin/env bash
# Run this in an EMPTY folder, then copy the Domain/Infrastructure files over the generated ones.
set -e
dotnet new sln -n EventBooking --format slnx   # needs .NET SDK 9.0.200+

dotnet new classlib -n EventBooking.Domain         -o EventBooking.Domain
dotnet new classlib -n EventBooking.Application    -o EventBooking.Application
dotnet new classlib -n EventBooking.Infrastructure -o EventBooking.Infrastructure
dotnet new webapi   -n EventBooking.API            -o EventBooking.API --use-controllers

dotnet sln add EventBooking.Domain EventBooking.Application EventBooking.Infrastructure EventBooking.API

# Dependency rules (Clean Architecture)
dotnet add EventBooking.Application    reference EventBooking.Domain
dotnet add EventBooking.Infrastructure reference EventBooking.Application EventBooking.Domain
dotnet add EventBooking.API            reference EventBooking.Application EventBooking.Infrastructure

# Packages
dotnet add EventBooking.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add EventBooking.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add EventBooking.API            package Microsoft.EntityFrameworkCore.Design
dotnet add EventBooking.API            package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add EventBooking.Application    package MediatR
dotnet add EventBooking.Application    package FluentValidation.DependencyInjectionExtensions
