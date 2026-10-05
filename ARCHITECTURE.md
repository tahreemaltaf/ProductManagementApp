# Lab 03 — Architecture and SOLID

Request flow:

`USER → Products.razor → IProductService → ProductService → IProductRepository → ProductRepositoryJson → products.json`

```
Blazor UI (Products.razor)
        ↓
IProductService
        ↓
ProductService          ← business validation lives here
        ↓
IProductRepository
        ↓
ProductRepositoryJson   ← the only class that knows storage is JSON
        ↓
Data/products.json
```

## Why the Blazor component must not access products.json

`Products.razor` is presentation. File I/O, serialization, and storage details do not belong in UI. If the page opened the JSON file, every screen that needed products would duplicate that code, validation would mix with markup, and swapping JSON for a database would force UI rewrites.

Problems if file handling sits in `Products.razor`:

- The page becomes hard to test and hard to reuse.
- Business rules (name length, price > 0) mix with `File.ReadAllText` and `JsonSerializer`.
- A missing or corrupt file can crash the component with a raw exception.
- Changing storage (PostgreSQL, REST API, fake list) requires changing the Razor page.

Layer ownership:

| Concern | Layer | Class |
| --- | --- | --- |
| UI markup, forms, tables | Presentation | `Products.razor` |
| Validation and new IDs | Business | `ProductService` |
| JSON read/write | Data Access | `ProductRepositoryJson` |

## Dependency Injection

`Program.cs` registers:

```csharp
builder.Services.AddSingleton<IProductRepository, ProductRepositoryJson>();
builder.Services.AddScoped<IProductService, ProductService>();
```

When `ProductService` asks for `IProductRepository`, the container creates `ProductRepositoryJson` and injects it. When `Products.razor` asks for `IProductService`, the container creates `ProductService`. Neither UI nor service constructs the JSON repository with `new`.

`ProductRepositoryJson` is a singleton so one file path and one lock are shared. `ProductService` is scoped because each interactive circuit/request gets a short-lived coordinator that still talks to the same repository.

## Nullable properties on ProductModel

- `Name` and `Category` are required, so they are non-nullable `string` with empty defaults for form binding.
- `Description` is optional, so it is `string?`.
- `Id`, `Price`, and `IsActive` are value types and cannot be null.

## Architecture tests

4. `Products.razor` must not call `File.ReadAllText()`. That would skip the service and repository and break 3-tier separation.
5. `ProductService` must not call `File.WriteAllText()`. Persistence is the DAL's job. The service only validates and delegates.
6. Only `ProductRepositoryJson` should know that storage is JSON.
7. If JSON is replaced by PostgreSQL, primarily change the DAL: add a new `IProductRepository` implementation and switch the DI registration. UI and `ProductService` stay the same.

## SOLID (Task 23)

- **S**ingle Responsibility: UI renders, service validates, repository persists.
- **O**pen/Closed: swap `ProductRepositoryFake` in DI without modifying UI or `ProductService`.
- **L**iskov: fake and JSON repositories honor the same `IProductRepository` contract.
- **I**nterface Segregation: the product contracts stay focused on product operations.
- **D**ependency Inversion: `ProductService` depends on `IProductRepository`, not `ProductRepositoryJson`.

Replace JSON with the in-memory fake by changing only:

```csharp
builder.Services.AddSingleton<IProductRepository, ProductRepositoryFake>();
```

## Persistence experiment

JSON is a file on disk. After Create, `ProductRepositoryJson` serializes the list and writes `products.json`. Restarting the app reads that file, so the product is still there.

`ProductRepositoryFake` keeps a `List<ProductModel>` in process memory. Stopping the app unloads that memory, so fake data does not survive restart.

## Missing, empty, and invalid JSON

- Missing file: repository returns an empty list. The UI shows the empty state.
- Empty file or `[]`: same empty list, no crash.
- Invalid JSON: `JsonException` is wrapped as `ProductDataException`. The UI shows a controlled message, not a stack trace.

## Viva notes

8. Three-tier architecture splits presentation, business, and data access.
9. Interfaces let callers depend on a contract, not a concrete storage class.
10. The DAL loads and saves data. It does not own business policy.
11. UI should not access JSON because storage is not a presentation concern.
12. Dependency Injection is a container supplying constructor dependencies.
13. `ProductService` depends on `IProductRepository` so persistence can change without rewriting business rules.
14. Serialization turns objects into JSON text. Deserialization turns JSON text into objects.
15. If `products.json` is missing, handle it: empty list and/or create the file later on write.
16. If JSON is invalid, catch `JsonException` at the DAL boundary and show a controlled error.
17. JSON lives on disk; `List<T>` lives only in RAM.
18. Replace JSON with a database by writing another repository and changing DI.
19. SRP, OCP, and DIP are the principles this lab demonstrates most clearly.
20. A REST client would be another `IProductRepository` implementation. UI and service would not need to change.
