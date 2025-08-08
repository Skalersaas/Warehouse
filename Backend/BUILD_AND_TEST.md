# Warehouse Management System - Build and Test Guide

## Step 1: Remove Old Migration and Create New One

```bash
# Navigate to the Persistence project
cd Backend/Persistence

# Remove old migration
dotnet ef migrations remove --startup-project ../Api

# Create new migration with all our changes
dotnet ef migrations add CompleteWarehouseSystem --startup-project ../Api

# Update database
dotnet ef database update --startup-project ../Api
```

## Step 2: Build and Run

```bash
# Navigate to API project
cd Backend/Api

# Restore packages
dotnet restore

# Build project
dotnet build

# Run the application
dotnet run
```

## Step 3: Test the API

### Access Swagger UI
Open browser: `http://localhost:5000/swagger`

### Test Basic Flow

1. **Create a Unit:**
```json
POST /api/units
{
  "name": "kg"
}
```

2. **Create a Resource:**
```json
POST /api/resources
{
  "name": "Steel Bars"
}
```

3. **Create a Client:**
```json
POST /api/clients
{
  "name": "ABC Corporation",
  "address": "123 Main Street"
}
```

4. **Check Initial Balance (should be empty):**
```
GET /api/balance/warehouse
```

5. **Create Receipt Document (adds to balance):**
```json
POST /api/receiptdocuments
{
  "number": "RCP-001",
  "date": "2025-01-15T10:00:00Z",
  "items": [
    {
      "resourceId": 1,
      "unitId": 1,
      "quantity": 100.5
    }
  ]
}
```

6. **Check Balance After Receipt:**
```
GET /api/balance/warehouse
```

7. **Create Shipment Document (Draft status):**
```json
POST /api/shipmentdocuments
{
  "number": "SHP-001",
  "clientId": 1,
  "date": "2025-01-15T12:00:00Z",
  "items": [
    {
      "resourceId": 1,
      "unitId": 1,
      "quantity": 50.0
    }
  ]
}
```

8. **Sign the Shipment (reduces balance):**
```
PATCH /api/shipmentdocuments/1/sign
```

9. **Check Balance After Shipment:**
```
GET /api/balance/warehouse
```

10. **Test Archive Functionality:**
```
PATCH /api/resources/1/archive
```

## Expected Results

- All endpoints should respond with proper HTTP status codes
- Balance should be 100.5 after receipt
- Balance should be 50.5 after signing shipment
- Archive operations should work without errors
- Validation should prevent invalid operations

## Troubleshooting

If you encounter build errors:

1. **Check Package Restore:**
```bash
dotnet restore
```

2. **Clean and Rebuild:**
```bash
dotnet clean
dotnet build
```

3. **Check Connection String:**
Make sure your `.env` file has the correct PostgreSQL connection string.

4. **Database Issues:**
If database creation fails, check PostgreSQL is running and connection string is correct.
