# Mortgage Application API

An ASP.NET Core Web API for creating, retrieving and assessing mortgage applications.

The project uses .NET 10, FluentValidation, dependency injection, an in-memory repository, logging and API-key authentication.

## Features

- Create and retrieve mortgage applications.
- Validate requests using FluentValidation.
- Assess mortgage affordability based on income, expenses, deposit, property value and mortgage term.
- Secure endpoints using an `X-Api-Key` header.
- In-memory data storage and logging.
- Unit and integration testing using NUnit and Moq.
- Docker support.

## Prerequisites

- .NET 10 SDK
- Visual Studio or VS Code (optional)
- Docker Desktop (for running in Docker)

No database is required.

## Setup Instructions

1. Clone the repository.
2. Navigate to the solution directory.
3. Restore dependencies:

```bash
dotnet restore
```

4. Add an API key to `appsettings.Development.json`:

```json
{
  "ApiKey": "dev-key-123"
}
```

Use a development-only key and avoid committing sensitive information.

## Running the Application

From the API project directory, run:

```bash
dotnet run
```

The API is configured to run locally at:

`http://localhost:5002`

### API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/MortgageApplication` | Create a mortgage application |
| GET | `/MortgageApplication/{id}` | Retrieve an application |
| GET | `/MortgageApplication/Assess/{id}` | Assess mortgage affordability |

All requests require the following header:

```http
X-Api-Key: dev-key-123
```

### Example POST Request

```http
POST http://localhost:5002/MortgageApplication
Content-Type: application/json
X-Api-Key: dev-key-123
```

```json
{
  "annualIncome": 60000,
  "monthlyExpenses": 500,
  "depositAmount": 50000,
  "propertyValue": 300000,
  "termYears": 25
}
```

Successful requests return `200 OK`. Invalid requests return `400 Bad Request`, and missing or invalid API keys return `401 Unauthorized`.

## Running with Docker

The application uses a multi-stage Dockerfile with .NET 10 Linux images.

From the solution directory, build the Docker image:

```bash
docker build -t mortgage-api -f "Mortgage Application API/Dockerfile" .
```

Run the container:

```bash
docker run -d --name mortgage-api-container -p 5002:8080 -e "ApiKey=dev-key-123" mortgage-api
```

The API will be accessible at:

`http://localhost:5002`

The container listens on port `8080`, which is mapped to port `5002` on the host.

Useful commands:

```bash
# View logs
docker logs mortgage-api-container

# Stop container
docker stop mortgage-api-container

# Remove container
docker rm mortgage-api-container
```

The API key is passed through an environment variable rather than stored in the Docker image. For production, use secure secret management and HTTPS.

## Running Tests

From the solution directory:

```bash
dotnet test
```

The solution includes:

- **Unit tests:** NUnit and Moq tests covering validation, affordability calculations and controller behaviour.
- **Integration tests:** Tests using `WebApplicationFactory` to verify API endpoints, authentication, validation and application workflows.

## Design Decisions & Reflection

### 1. Key Design Decisions

**Repository Pattern**

I used an `IMortgageApplicationRepository` interface with an in-memory implementation. This separates data access from the controller and allows the datastore to be replaced without changing the API logic.

**Separation of Responsibilities**

FluentValidation handles request validation, while a dedicated `AffordabilityCalculator` handles mortgage assessments. Dependency injection makes these components independently testable.

**Extensible Logging**

I used ASP.NET Core's `ILogger<T>` abstraction with an in-memory logging provider. This allows other providers, such as Serilog or Application Insights, to be introduced without changing the application logic.

### 2. Scaling Consideration

The first change would be replacing the in-memory repository with a persistent database such as SQL Server.

Currently, data is lost when the application restarts and cannot be shared between multiple API instances. A database would provide persistent storage and support horizontal scaling.

### 3. Trade-offs

To keep the implementation simple and focused:

- Data and logs are stored in memory rather than persisted.
- API-key authentication is used instead of a more complex authentication system.
- Affordability assessments are processed synchronously.
- Advanced features such as rate limiting, monitoring and background processing have been left out.

These decisions reduce complexity while keeping the application maintainable and extensible.

### 4. Next Steps

Given more time, I would:

1. **Expand automated testing:** Add further unit tests for edge cases and integration tests covering complete workflows, failures and validation scenarios.
2. **Implement background processing:** Move affordability assessments into a background worker using a queue. The API could return `202 Accepted` with an assessment ID, allowing clients to retrieve the result later.
3. **Introduce persistent storage:** Replace the in-memory repository with a database.
4. **Improve security and monitoring:** Add stronger authentication, rate limiting, centralised logging and health checks.

## Notes

This API is a demonstration of software design and mortgage affordability calculations. It is not intended to be used as a production lending decision system.