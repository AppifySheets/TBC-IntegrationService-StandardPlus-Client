# TBC Bank IntegrationService Standard+ C#/net8 Client
## Why - official implementation is error-prone, easy-to-mess-up and requires a lot of manual fixing
### Current library attempts to wrap soap calls in immutable, type-safe way

#### You will require 4 things from the TBC Bank - 1) `.pfx` certificate, 2) `Username`, 3) `Password` and 4) `certificate_password`

Service Documentation by the TBC Bank is here - <a href="https://developers.tbcbank.ge/docs/dbi-overview" target="_blank">https://developers.tbcbank.ge/docs/dbi-overview</a>

## Installation

```bash
dotnet add package AppifySheets.TBC.IntegrationService.Client
```

## Following services are implemented:
* <a href="https://developers.tbcbank.ge/docs/import-single-payments" target="_blank">Import Single Payment Orders</a> - Execute various types of payment transfers
* <a href="https://developers.tbcbank.ge/docs/account-movement" target="_blank">Get Account Movements</a> - Retrieve account transaction history
* <a href="https://developers.tbcbank.ge/docs/payment-order-status" target="_blank">Get Payment Order Status</a> - Check status of submitted payment orders
* <a href="https://developers.tbcbank.ge/docs/change-password" target="_blank">Change Password</a> - Change API user password

## Usage Examples

### Setup
```csharp
// Create API credentials with validation
var credentialsResult = TBCApiCredentials.Create("Username", "Password");
if (credentialsResult.IsFailure)
    return credentialsResult.Error;

// Add certificate authentication
var credentialsWithCertResult = TBCApiCredentialsWithCertificate.Create(
    credentialsResult.Value, 
    "TBCIntegrationService.pfx", 
    "certificate_password");
    
// Create SOAP caller
var soapCallerResult = TBCSoapCaller.Create(credentialsWithCertResult.Value);
var tbcSoapCaller = soapCallerResult.Value;

// Create bank accounts
var ownAccountGEL = BankAccount.Create("GE00TB0000000000000001", "GEL").Value;
var ownAccountUSD = BankAccount.Create("GE00TB0000000000000002", "USD").Value;

// Or create components separately
var ibanResult = Iban.Create("GE00TB0000000000000001");
var accountResult = BankAccount.Create(ibanResult.Value, Currency.GEL);
if (accountResult.IsFailure)
    return accountResult.Error;
```

### Account Operations

<details>
<summary><b>Get Account Movements</b></summary>

```csharp
// Get account movements for a specific period
var accountMovements = await GetAccountMovementsHelper.GetAccountMovement(
    new Period(new DateTime(2023, 9, 1), new DateTime(2023, 9, 26)), 
    tbcSoapCaller);

// The helper method handles pagination automatically
// Returns all movements within the specified period
```
</details>

<details>
<summary><b>Get Payment Order Status</b></summary>

```csharp
// Check status of a specific payment order by its ID
var paymentOrderId = 1632027071; // The ID returned when creating a payment order
var checkStatus = await tbcSoapCaller.GetDeserialized(
    new GetPaymentOrderStatusRequestIo(paymentOrderId));

// Returns status information including:
// - Current status (pending, completed, rejected, etc.)
// - Processing details
// - Error messages if any
```
</details>

<details>
<summary><b>Change Password</b></summary>

```csharp
// Change API user password (requires digipass code)
var newPassword = "NewSecurePassword123!";
var digipassCode = "123456"; // One-time code from your digipass device

var passwordChangeResult = await tbcSoapCaller.GetDeserialized(
    new ChangePasswordRequestIo(newPassword, digipassCode));

// Note: After successful password change, update your credentials
```
</details>

### Payment Transfers

#### Common Transfer Parameters
```csharp
// Common parameters for all transfer types
var bankTransferCommonDetails = new BankTransferCommonDetails
{
    DocumentNumber = 123,
    Amount = 0.01m,
    BeneficiaryName = "TEST",
    SenderAccountWithCurrency = ownAccountGEL,
    Description = "TEST"
};
```

<details>
<summary><b>Internal Transfers (Within TBC Bank)</b></summary>

#### Transfer in GEL
```csharp
var withinBankGel = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        RecipientAccountWithCurrency = BankAccount.Create("GE00TB0000000000000003", "GEL").Value,
        BankTransferCommonDetails = bankTransferCommonDetails
    }));
```

#### Transfer in Foreign Currency (USD)
```csharp
var withinBankCurrency = await tbcSoapCaller.GetDeserialized(new ImportSinglePaymentOrdersRequestIo(
    new TransferWithinBankPaymentOrderIo
    {
        BankTransferCommonDetails = bankTransferCommonDetails with
        {
            SenderAccountWithCurrency = ownAccountUSD
        },
        RecipientAccountWithCurrency = BankAccount.Create("GE00TB0000000000000004", "USD").Value,
    }));
```
</details>

<details>
<summary><b>External Transfers (To Other Banks)</b></summary>

#### Domestic Transfer to Another Georgian Bank (GEL)
```csharp
var toAnotherBankGel = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankNationalCurrencyPaymentOrderIo(
            BankAccount.Create("GE00BG0000000000000001", "GEL").Value, 
            "123456789") // Beneficiary tax code
        {
            BankTransferCommonDetails = bankTransferCommonDetails
        }));
```

#### International Transfer (Foreign Currency)
```csharp
var toAnotherBankCurrency = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo(
            "Beneficiary Bank Name",
            "BANKSWIFT", // Bank SWIFT/BIC code
            "SHA", // Charge type: SHA (shared), OUR (sender pays), BEN (beneficiary pays)
            "Payment Reference",
            BankAccount.Create("GE00BG0000000000000002", "USD").Value)
        {
            BankTransferCommonDetails = bankTransferCommonDetails with 
            { 
                SenderAccountWithCurrency = ownAccountUSD 
            }
        }));
```

#### International Transfer Example (To China)
```csharp
var toChina = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TransferToOtherBankForeignCurrencyPaymentOrderIo(
            "China",
            "ICBKCNBJSZN", // Bank SWIFT code
            "INDUSTRIAL AND COMMERCIAL BANK OF CHINA SHENZHEN BRANCH", // Bank name
            "SHA", // Charge type
            BankAccount.Create("CN0000000000000000001", "USD").Value)
        {
            BankTransferCommonDetails = bankTransferCommonDetails with
            {
                SenderAccountWithCurrency = ownAccountUSD,
                BeneficiaryName = "Shenzhen Example Company Ltd"
            }
        }));
```
</details>

<details>
<summary><b>Treasury Transfers</b></summary>

```csharp
// Transfer to Georgian Treasury
var toTreasury = await tbcSoapCaller.GetDeserialized(
    new ImportSinglePaymentOrdersRequestIo(
        new TreasuryTransferPaymentOrderIo(101001000) // Treasury code
        { 
            BankTransferCommonDetails = bankTransferCommonDetails 
        }));
```
</details>

## Error Handling

All operations return a `Result<T>` type from CSharpFunctionalExtensions:

```csharp
var result = await tbcSoapCaller.GetDeserialized(request);

if (result.IsSuccess)
{
    var response = result.Value;
    // Process successful response
}
else
{
    var error = result.Error;
    // Handle error
    Console.WriteLine($"Operation failed: {error}");
}
```

## Important Notes

- All monetary amounts are in decimal format
- IBAN accounts must be valid Georgian IBANs (starting with GE)
- Document numbers must be unique per day
- Digipass codes are required for sensitive operations like password changes
- Certificate authentication is mandatory for all API calls