# SDET Technical Challenge — User Management API

Automated API testing framework developed using **C#, .NET 10, NUnit, and RestSharp**, with continuous integration through GitHub Actions.

The project validates the User Management API against its OpenAPI specification in two environments: **DEV** and **PROD**.

It includes positive and negative testing, boundary value analysis, authentication checks, data persistence verification, defect reporting, and parallel CI/CD execution.

## 1. Technology Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Test implementation and runtime |
| NUnit | Test framework and assertions |
| RestSharp | HTTP client for API requests |
| System.Text.Json | JSON deserialization |
| Docker | Running the application under test |
| GitHub Actions | Automated CI/CD execution |
| TRX | Test execution reports |

## 2. Project Structure

```text
SdetTechnicalChallenge/
├── .github/
│   └── workflows/
│       └── e2e-tests.yml
├── tests/
│   └── UserManagementApi.Tests/
│       ├── Clients/
│       │   └── UserApiClient.cs
│       ├── Helpers/
│       │   └── TestConfiguration.cs
│       ├── Models/
│       │   ├── User.cs
│       │   └── ErrorResponse.cs
│       ├── Tests/
│       │   ├── GetUsersTests.cs
│       │   ├── CreateUserTests.cs
│       │   ├── GetUserTests.cs
│       │   ├── UpdateUserTests.cs
│       │   └── DeleteUserTests.cs
│       └── UserManagementApi.Tests.csproj
├── .gitignore
├── BUGS.md
├── TEST_REPORT.md
├── README.md
└── SdetTechnicalChallenge.slnx
```

### Architecture

The framework separates responsibilities into four main components:

- **Clients:** Centralized HTTP request execution through `UserApiClient`.
- **Models:** C# classes representing API data structures.
- **Helpers:** Environment configuration and reusable settings.
- **Tests:** NUnit test classes organized by endpoint and operation.

This structure improves maintainability, readability, and code reuse.

## 3. Prerequisites

The following tools are required to execute the tests locally:

- .NET 10 SDK
- Docker Desktop or Docker Engine
- Git
- An available local port `3000`

Verify your .NET installation:

```bash
dotnet --version
```

Verify Docker:

```bash
docker --version
```

## 4. Getting Started

### Clone the repository

```bash
git clone https://github.com/dellmiguel/SdetTechnicalChallenge.git
cd SdetTechnicalChallenge
```

### Start the API

Pull the Docker image:

```bash
docker pull ghcr.io/danielsilva-loanpro/sdet-interview-challenge
```

Start the application:

```bash
docker run -d --name user-api -p 3000:3000 ghcr.io/danielsilva-loanpro/sdet-interview-challenge
```

The API will be accessible through:

| Environment | Base URL |
|---|---|
| DEV | `http://localhost:3000/dev/` |
| PROD | `http://localhost:3000/prod/` |

### Restore dependencies

```bash
dotnet restore
```

## 5. Running the Tests

### Run against DEV

**PowerShell:**

```powershell
$env:TEST_ENV="dev"
dotnet test
```

### Run against PROD

**PowerShell:**

```powershell
$env:TEST_ENV="prod"
dotnet test
```

### Run an individual test

```bash
dotnet test --filter "Name=DeleteUser_WithoutToken_ShouldReturn401"
```

### Environment Configuration

The framework supports the following environment variables:

| Variable | Description | Default |
|---|---|---|
| `BASE_URL` | API server address | `http://localhost:3000` |
| `TEST_ENV` | Target environment (`dev` or `prod`) | `dev` |
| `AUTH_TOKEN` | Authentication token used for DELETE requests | Challenge-provided token |

The API base URL is constructed automatically using `BASE_URL` and `TEST_ENV`.

For local execution, `AUTH_TOKEN` can be set through an environment variable. In GitHub Actions, the token is provided through the `API_AUTH_TOKEN` repository secret.

## 6. API Test Coverage

The automated test suite covers all five documented operations.

| Method | Endpoint | Coverage |
|---|---|---|
| GET | `/users` | Successful retrieval and JSON response structure |
| POST | `/users` | Creation, required fields, email validation, duplicate emails, age boundaries |
| GET | `/users/{email}` | Existing and nonexistent users |
| PUT | `/users/{email}` | Successful updates, persistence, validation, duplicate emails |
| DELETE | `/users/{email}` | Authorized deletion, missing and invalid authentication, nonexistent users |

### Testing Techniques

- Positive and negative testing
- Boundary value analysis
- Input validation
- HTTP status code verification
- Data persistence validation
- Authentication and authorization testing
- Parameterized testing with NUnit
- Environment comparison

Unique email addresses are generated using GUIDs to reduce collisions between test runs.

## 7. Test Execution Results

A total of **27 automated test cases** were executed against each environment.

| Metric | DEV | PROD |
|---|---:|---:|
| Total Tests | 27 | 27 |
| Passed | 20 | 22 |
| Failed | 7 | 5 |
| Skipped | 0 | 0 |
| Pass Rate | 74.1% | 81.5% |

**Total test executions:** 54  
**Passed executions:** 42  
**Failed executions:** 12  
**Distinct defects identified:** 5

These results were reproduced both locally and in GitHub Actions.

The failed tests represent known API defects. Test expectations have not been modified to conceal failures.

## 8. Identified Defects

| ID | Description | Severity | Affected Environments |
|---|---|---|---|
| BUG-001 | Invalid email formats accepted | Medium | DEV / PROD |
| BUG-002 | Duplicate emails return HTTP 500 | Medium | DEV / PROD |
| BUG-003 | Nonexistent users return HTTP 500 | Medium | DEV / PROD |
| BUG-004 | User updates are not persisted | High | DEV / PROD |
| BUG-005 | Unauthorized DELETE operation | Critical | DEV |

The most significant finding is BUG-005: the DEV environment permits deleting users without authentication.

For reproduction steps, expected and actual behavior, and impact assessments, see [BUGS.md](BUGS.md).

## 9. Continuous Integration

GitHub Actions automatically executes the API test suite when changes are pushed to the `main` branch or when a pull request targets `main`.

Manual execution is also supported through `workflow_dispatch`.

The pipeline contains two independent jobs:

- **E2E Tests - DEV**
- **E2E Tests - PROD**

Each job:

1. Checks out the repository.
2. Installs .NET 10.
3. Starts the application using Docker.
4. Waits for API readiness.
5. Runs the automated test suite.
6. Uploads test results as TRX artifacts.

Both jobs can execute in parallel.

### Workflow

[View GitHub Actions](https://github.com/dellmiguel/SdetTechnicalChallenge/actions)

### Test Artifacts

Each workflow run produces downloadable test result artifacts:

- `dev-test-results`
- `prod-test-results`

The artifacts contain `.trx` files that can be used for further analysis.

**Note:** GitHub Actions reports failing jobs when tests detect known defects. This is intentional and preserves accurate quality feedback.

## 10. Test Documentation

Additional documentation is available:

- [Bug Report](BUGS.md) — Detailed defect descriptions, reproduction steps, expected and actual results, severity, and impact.
- [Test Execution Report](TEST_REPORT.md) — Test strategy, scope, architecture, execution results, environment comparison, and conclusions.

## 11. Current Limitations

- The application under test is supplied as a Docker image.
- The framework does not modify the application's implementation.
- Performance, load, and penetration testing are outside the current scope.
- Additional automated response schema validations and edge cases could extend coverage.
- Known API defects remain visible as failed tests.

## 12. Conclusion

This project demonstrates an automated API testing solution using C#, NUnit, and RestSharp, integrated with GitHub Actions.

The framework validates CRUD operations, input constraints, authentication, and persistence behavior across two independent environments.

It successfully identifies functional defects and an environment-specific authorization vulnerability while maintaining reproducible test execution and traceable reporting.
