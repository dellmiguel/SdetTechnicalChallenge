# Bug Report — User Management API

---

## BUG-001: POST endpoint accepts invalid email formats

**Severity:** Medium  
**Environment:** DEV and PROD  
**Endpoint:** `POST /users`  
**Status:** Open

### Description

The API accepts invalid email addresses when creating users, despite the email format validation defined in the API specification.

### Steps to Reproduce

1. Send a POST request to `/users` with a valid name and age.
2. Set the email field to `invalid-email`.
3. Repeat the request using `missing-at-sign.com`.
4. Inspect the HTTP response.

### Expected Result

The API returns `400 Bad Request` and rejects the invalid email address.

### Actual Result

The API returns `201 Created` and accepts the invalid email address.

### Impact

Invalid user data can be stored, potentially affecting downstream functionality that relies on valid email addresses.

### Automated Test

`CreateUser_WithInvalidEmail_ShouldReturn400`

---

## BUG-002: POST endpoint returns 500 for duplicate email addresses

**Severity:** Medium  
**Environment:** DEV and PROD  
**Endpoint:** `POST /users`  
**Status:** Open

### Description

Creating a user with an email address that already exists causes the API to return an internal server error instead of a conflict response.

### Steps to Reproduce

1. Create a user using `POST /users` with a unique email address.
2. Send another POST request using the same email address.
3. Inspect the HTTP response.

### Expected Result

The API returns `409 Conflict`.

### Actual Result

The API returns `500 Internal Server Error`.

### Impact

The API does not handle duplicate user creation according to its contract, making error handling less predictable for clients.

### Automated Test

`CreateUser_WithDuplicateEmail_ShouldReturn409`

---

## BUG-003: GET endpoint returns 500 for nonexistent users

**Severity:** Medium  
**Environment:** DEV and PROD  
**Endpoint:** `GET /users/{email}`  
**Status:** Open

### Description

Requesting a user that does not exist causes the API to return an internal server error instead of a resource-not-found response.

### Steps to Reproduce

1. Generate an email address that does not exist in the database.
2. Send a GET request to `/users/{email}`.
3. Inspect the HTTP response.

### Expected Result

The API returns `404 Not Found` with an error response.

### Actual Result

The API returns `500 Internal Server Error`.

### Impact

API consumers cannot reliably distinguish a nonexistent resource from an internal server failure.

### Automated Test

`GetUser_WithNonExistingEmail_ShouldReturn404AndErrorResponse`

---

## BUG-004: PUT endpoint does not persist updated user information

**Severity:** High  
**Environment:** DEV and PROD  
**Endpoint:** `PUT /users/{email}`  
**Status:** Open

### Description

The API returns a successful response containing updated user information, but subsequent GET requests return the original values.

### Steps to Reproduce

1. Create a user with the following information:
   - Name: `Original User`
   - Age: `30`
2. Send a PUT request to update the user:
   - Name: `Updated User`
   - Age: `35`
3. Verify that PUT returns `200 OK`.
4. Retrieve the same user using `GET /users/{email}`.
5. Compare the persisted values with the requested updates.

### Expected Result

The API persists the updated information and subsequent GET requests return `Updated User` and age `35`.

### Actual Result

The PUT response indicates success, but GET returns `Original User` and age `30`.

### Impact

Clients may believe that updates have been saved when the stored information remains unchanged, potentially causing data inconsistencies and incorrect business decisions.

### Automated Test

`UpdateUser_WithValidData_ShouldReturn200AndUpdatedUser`


## BUG-005: DELETE endpoint allows unauthorized user deletion in DEV

**Severity:** Critical  
**Environment:** DEV  
**Endpoint:** `DELETE /dev/users/{email}`  
**Status:** Open

### Description

The DELETE endpoint allows a user to be deleted without providing the required `Authentication` header. The API returns HTTP 204 and permanently removes the user from the database.

This behavior violates the API authentication requirements.

### Steps to Reproduce

1. Create a new user using `POST /dev/users`.
2. Send a DELETE request to `/dev/users/{email}` without the `Authentication` header.
3. Verify the HTTP response.
4. Send `GET /dev/users` and check whether the deleted user is still present.

### Expected Result

- The DELETE request returns `401 Unauthorized`.
- The user remains in the database.

### Actual Result

- The DELETE request returns `204 No Content`.
- The user is removed from the database.

### Impact

An unauthenticated client can delete user records, causing unauthorized data loss.

### Environment Comparison

- **DEV:** Defect reproduced.
- **PROD:** Corresponding automated tests pass.

### Automated Test

`DeleteUser_WithoutToken_ShouldReturn401`

### Evidence

The defect was reproduced locally using NUnit and RestSharp. The DEV GitHub Actions job also reports failures in the authentication-related DELETE tests.
