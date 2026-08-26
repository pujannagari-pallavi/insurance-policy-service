# Policy Service Contract

## Ownership

Policy Service owns policy records, authorization rules, and its database. It validates customer identity through the Customer Service API and must not query the Customer Service database.

## Public API

- Local URL: `http://localhost:5182`
- Gateway prefix: `/policies`
- Health endpoint: `GET /health`
- Development Swagger UI: `/swagger`

Service routes use the `/api/{resource}` convention. Error responses use RFC 7807 `application/problem+json` with `status`, `title`, and `detail`.

## Authorization

Validate Identity Service JWTs locally with issuer `InsurancePlatform.Identity` and audience `InsurancePlatform.Clients`.

| Permission | Capability |
| --- | --- |
| `Policy.Read` | Read policy records |
| `Policy.Write` | Create or update the caller's policy records |
| `Policy.Write.Any` | Create or update policy records for any customer |

The service verifies that `CustomerId` exists and that the caller owns the customer identity, unless the caller has the `Admin` role or `Policy.Write.Any` permission.

## Events

Policy Service consumes customer lifecycle events and reserves `policy.created.v1` and `policy.updated.v1` for notification, billing, and reporting consumers.

## Change Rules

Review the OpenAPI diff with API consumers before changing public endpoints. Keep secrets, database connection strings, and deployed service URLs outside source control.