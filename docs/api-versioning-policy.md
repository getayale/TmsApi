# TMS API Versioning Policy

## Breaking Changes

A breaking change is any change that can cause an existing client to fail or behave differently.

Examples of breaking changes:

- Removing an existing response field.
- Renaming an existing response field.
- Changing an existing HTTP status code behavior.
- Tightening validation rules that reject previously accepted requests.
- Changing default sorting or filtering behavior.
- Changing the meaning of an existing field.

Breaking changes require a new API version.

---

## Additive Changes

An additive change does not break existing clients.

Examples:

- Adding a new optional response field.
- Adding a new endpoint.
- Adding a new optional query parameter.
- Adding optional functionality while keeping existing behavior unchanged.

Additive changes can be released without creating a new API version.

---

## Version Lifecycle and Sunset Policy

TMS API supports multiple versions simultaneously.

When a new version is released, the previous version remains available for a minimum of 6 months. This allows clients, including training centres with scheduled maintenance cycles, enough time to migrate.

Deprecated versions continue returning normal responses but include:

- `Deprecation: true`
- `Sunset` date
- `Link` header pointing to the successor version

Example:
