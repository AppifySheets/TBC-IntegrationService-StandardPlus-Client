# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8.0 C# class library that provides a type-safe, immutable wrapper around TBC Bank's Integration Service Standard+ SOAP API. The library aims to simplify working with the bank's complex SOAP interface by providing clean, functional abstractions.

## Build and Test Commands

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run the demo console application
dotnet run --project AppifySheets.TBC.IntegrationService.Client.DemoConsole
```

## Architecture and Code Structure

### Key Design Principles
- **Immutable Data Structures**: All data types are immutable records or value objects
- **Functional Programming**: Uses `CSharpFunctionalExtensions` for Result pattern and error handling
- **Type Safety**: Strong typing with specific interfaces for different transfer types (within bank, to other banks, treasury)
- **SOAP Abstraction**: Complex SOAP operations are wrapped in a clean, type-safe API

### Project Structure
- `AppifySheets.TBC.IntegrationService.Client` - Main client library with SOAP infrastructure
  - `SoapInfrastructure/` - SOAP request/response classes for each operation
  - `TBCSoapCaller.cs` - Main entry point for API calls
- `AppifySheets.Immutable.BankIntegrationTypes` - Shared immutable types and value objects
- `AppifySheets.TBC.IntegrationService.Tests` - xUnit tests with Shouldly assertions
- `AppifySheets.TBC.IntegrationService.Client.DemoConsole` - Example usage

### Key Components
1. **TBCSoapCaller**: Main service class that handles SOAP communication with certificate authentication
2. **Request/Response IO Classes**: Type-safe wrappers for each SOAP operation (e.g., `ImportSinglePaymentOrdersRequestIo`)
3. **Transfer Type Interfaces**: Different interfaces for various transfer types (`ITransferWithinBank`, `ITransferToOtherBankNationalCurrency`, etc.)
4. **Value Objects**: Strongly-typed wrappers like `BankAccountV`, `CurrencyV`, `BankAccountWithCurrencyV`

## Development Guidelines

### When Adding New SOAP Operations
1. Create request/response IO classes in `SoapInfrastructure/[OperationName]/`
2. Implement `ISoapRequestIo<TResponse>` interface
3. Use immutable records for data structures
4. Add XML serialization attributes matching TBC's SOAP schema
5. Create helper methods if the operation is complex (see `GetAccountMovementsHelper`)

### When Working with Transfer Types
- All transfer types implement specific interfaces that define required fields
- Use `TransferTypeRecordSpecific` for common transfer fields
- Each transfer type has its own validation rules and required fields

### Testing Approach
- Tests use xUnit framework with Shouldly for assertions
- Mock SOAP responses for unit tests
- Integration tests require valid TBC API credentials and certificates

## Important Implementation Details

### Certificate Authentication
The library uses X.509 certificates (.pfx files) for authentication. Credentials are managed through:
- `TBCApiCredentials` - Username and password
- `TBCApiCredentialsWithCertificate` - Adds certificate path and password

### Current Work in Progress
The codebase has ongoing work to add `PersonalNumber` field support to transfer operations. This field is being added to various transfer type interfaces and the SOAP request infrastructure.

### Error Handling
- Uses `Result<T>` pattern from CSharpFunctionalExtensions
- SOAP faults are converted to meaningful error messages
- Validation happens at multiple levels (value objects, request validation, SOAP response validation)