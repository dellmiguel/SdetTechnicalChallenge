# API Test Execution Report

**Project:** SDET Technical Challenge — User Management API  
**Testing Type:** Automated API Functional Testing  
**Framework:** C# / .NET 10 / NUnit / RestSharp  
**Execution Environments:** DEV and PROD  
**Execution Date:** October 8, 2026  
**Overall Status:** Completed with known defects

---

## 1. Executive Summary

Automated API testing was performed against the User Management API in two independent environments: DEV and PROD.

A total of 27 automated test cases were executed in each environment, covering user creation, retrieval, updating, deletion, input validation, error handling, and authentication.

The test suite was executed locally and through GitHub Actions using parallel jobs.

**Execution Results:**

| Metric | DEV | PROD |
|---|---:|---:|
| Total Tests | 27 | 27 |
| Passed | 20 | 22 |
| Failed | 7 | 5 |
| Skipped | 0 | 0 |
| Pass Rate | 74.1% | 81.5% |

The failed tests revealed five distinct defects across the two environments, including a critical authorization vulnerability affecting the DEV environment.

The API does not currently meet all expected functional and security requirements.

---

## 2. Testing Objectives

The main objectives were to:

- Validate API responses against the provided OpenAPI specification.
- Verify successful CRUD operations.
- Validate required fields and input constraints.
- Verify HTTP status codes for successful and unsuccessful requests.
- Confirm that created, updated, and deleted records are correctly persisted.
- Verify authentication requirements for DELETE operations.
- Compare API behavior between DEV and PROD.
- Automate test execution through CI/CD.

---

## 3. Test Scope

The following endpoints were included in the automated test suite:

| HTTP Method | Endpoint | Main Validation Areas |
|---|---|---|
| GET | `/users` | Successful retrieval, response structure |
| POST | `/users` | Creation, required fields, age boundaries, email validation, duplicate emails |
| GET | `/users/{email}` | Existing and nonexistent users |
| PUT | `/users/{email}` | Successful updates, persistence, validation, conflicts |
| DELETE | `/users/{email}` | Authorized deletion, missing or invalid authentication, nonexistent users |

Both environments were tested using the same automated test cases and expected results.

---

## 4. Test Automation Architecture

The automation framework was developed using:

| Technology | Purpose |
|---|---|
| C# | Test implementation |
| .NET 10 | Runtime and development platform |
| NUnit | Test framework and assertions |
| RestSharp | HTTP request execution |
| System.Text.Json | JSON response deserialization |
| Docker | Local API execution |
| GitHub Actions | Continuous integration |
| TRX | Test result reporting |

### Project Organization

```text
tests/UserManagementApi.Tests/
├── Clients/
│   └── UserApiClient.cs
├── Helpers/
│   └── TestConfiguration.cs
├── Models/
│   ├── User.cs
│   └── ErrorResponse.cs
└── Tests/
    ├── GetUsersTests.cs
    ├── CreateUserTests.cs
    ├── GetUserTests.cs
    ├── UpdateUserTests.cs
    └── DeleteUserTests.cs
```

### Design Decisions

**API Client Abstraction**

HTTP requests are centralized in `UserApiClient`, reducing duplicated request logic and separating API interactions from test assertions.

**Environment Configuration**

The framework supports environment selection through environment variables, allowing the same test suite to run against DEV and PROD.

**Independent Test Data**

Unique email addresses are generated using GUIDs to reduce conflicts between test executions.

**Response and Persistence Validation**

Tests validate HTTP responses and, where appropriate, perform additional GET requests to confirm whether changes were actually persisted.

**Parameterized Testing**

NUnit parameterized tests are used to validate multiple invalid inputs and boundary values without duplicating test methods.

---

## 5. Test Execution Strategy

### Local Execution

The API was started using Docker and exposed at:

`http://localhost:3000`

The two environments were accessed through:

- DEV: `http://localhost:3000/dev/`
- PROD: `http://localhost:3000/prod/`

Tests were executed using:

```powershell
$env:TEST_ENV="dev"
dotnet test
```

```powershell
$env:TEST_ENV="prod"
dotnet test
```

### CI/CD Execution

GitHub Actions was configured to execute the automated tests on pushes and pull requests targeting the main branch, with optional manual execution.

Two independent jobs were created:

- `test-dev`
- `test-prod`

Each job:

1. Checks out the repository.
2. Installs .NET 10.
3. Starts the API using Docker.
4. Waits for the API to become available.
5. Executes the automated NUnit test suite.
6. Uploads the TRX test results as workflow artifacts.

The two jobs can execute in parallel, with separate Docker containers and isolated execution environments.

---

## 6. CI/CD Execution Results

The GitHub Actions execution reproduced the same results observed locally.

### DEV Environment

**Total:** 27  
**Passed:** 20  
**Failed:** 7  
**Skipped:** 0  
**Execution Duration:** 215 ms

### PROD Environment

**Total:** 27  
**Passed:** 22  
**Failed:** 5  
**Skipped:** 0  
**Execution Duration:** 309 ms

Both jobs completed test execution and generated TRX result files.

The jobs were marked as failed because some assertions detected API defects. This is the expected CI/CD behavior: failing tests must remain visible rather than being suppressed.

---

## 7. Defect Summary

| Bug ID | Description | Severity | DEV | PROD |
|---|---|---|---|---|
| BUG-001 | Invalid email formats accepted during user creation | Medium | Failed | Failed |
| BUG-002 | Duplicate email returns HTTP 500 instead of 409 | Medium | Failed | Failed |
| BUG-003 | Nonexistent user returns HTTP 500 instead of 404 | Medium | Failed | Failed |
| BUG-004 | User updates are not persisted | High | Failed | Failed |
| BUG-005 | DELETE allows unauthorized user deletion | Critical | Failed | Passed |

### Environment Differences

The most significant difference was identified in DELETE authentication behavior.

In DEV, deleting a user without authentication returned HTTP 204 and removed the user from the database.

In PROD, the corresponding authentication tests passed.

This indicates an environment-specific security defect.

Detailed reproduction steps, expected results, actual results, and impact assessments are documented in `BUGS.md`.

---

## 8. Risks and Limitations

The following considerations apply to the current test implementation:

- Testing was performed against the supplied Docker-based API.
- Test coverage focuses on the documented functional behavior of the API.
- Performance, load, and penetration testing were outside the scope of this implementation.
- API defects remain unresolved because the application implementation is outside the test automation project.
- The automated suite intentionally fails when actual API behavior violates the specification.
- Additional negative scenarios and response schema validation could further improve coverage.

---

## 9. Conclusions

The automation framework successfully executed 27 test cases against both DEV and PROD environments.

The implementation demonstrates:

- Automated functional API testing using C#, NUnit, and RestSharp.
- Reusable API client and test configuration components.
- Positive, negative, boundary, and authentication test coverage.
- Data persistence verification.
- Parallel test execution through GitHub Actions.
- Automated TRX test result collection.
- Defect identification and environment comparison.

Five distinct defects were identified, including a critical authorization issue in DEV.

**Final Assessment:** The automation framework is operational and integrated into CI/CD. The API contains unresolved functional and security defects that should be addressed before considering the affected behavior production-ready.
