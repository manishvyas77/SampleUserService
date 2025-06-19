# SampleUserService

## Project Structure
```
SampleUserService
	SampleUserService          # Class library with API client logic
	SampleUserService.ConsoleApp      # Console demo app
	SampleUserService.Test           # xUnit tests
	README.md
```

## Requirements Implemented

- API Client using HttpClientFactory
- Async/Await implementation
- DTOs and domain model mapping
- Service Layer with pagination support
- Unit Tests using xUnit and Moq

## How to Run

1. Clone the repo or extract this zip
2. Build the solution: `dotnet build`
3. Run the console app: `dotnet run --project SampleUserService.ConsoleApp`
4. Run tests: `dotnet test`

