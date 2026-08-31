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

## Rating And Underwriting

The Policy Service calculates the final premium on the server. Client-provided premium values are ignored. The calculation uses the selected product's base premium, total sum insured, deductible, and policy term. Product-specific reference coverage amounts are used for Health Standard, Auto Comprehensive, and Life Protect.

New policies start as `Draft`. Customers can edit only draft policies. An administrator or user with `Policy.Write.Any` can submit a controlled status transition with `PATCH /api/policies/{policyId}/status`:

- `Draft` to `PendingApproval` or `Cancelled`
- `PendingApproval` to `Active` or `Cancelled`
- `Active` to `Lapsed`, `Cancelled`, or `Expired`

The transition request requires a target `status` and non-empty underwriting `remarks`. Every transition is recorded in the policy history.

Administrators and users with `Policy.Write.Any` can retrieve the underwriting queue with `GET /api/policies`. Other users can retrieve only their own policies through `GET /api/policies/mine?customerId={customerId}`.

## Events

Policy Service consumes customer lifecycle events and reserves `policy.created.v1` and `policy.updated.v1` for notification, billing, and reporting consumers.

## Change Rules

Review the OpenAPI diff with API consumers before changing public endpoints. Keep secrets, database connection strings, and deployed service URLs outside source control.