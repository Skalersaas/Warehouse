# Warehouse Backend Tests

This project contains comprehensive unit tests and integration tests for the Warehouse Backend application.

## Test Structure

```
Tests/
├── UnitTests/
│   ├── Domain/
│   │   ├── Entities/           # Entity model tests
│   │   └── Enums/              # Enum tests
│   ├── Application/            # Application service tests
│   ├── Api/                    # API controller tests
│   └── Persistence/            # Data access tests
├── IntegrationTests/           # Integration tests
├── Helpers/                    # Test utilities and helpers
├── GlobalUsings.cs             # Global using statements
└── appsettings.Test.json       # Test configuration
```

## Technologies Used

- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library for better readability
- **AutoFixture** - Auto-generation of test data
- **Moq** - Mocking framework
- **Coverlet** - Code coverage analysis

## Running Tests

### From Visual Studio
1. Open the `Backend.sln` solution
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test → Test Explorer)
4. Click "Run All Tests" or run specific test groups

### From Command Line
```bash
# Navigate to the Tests project directory
cd Backend/Tests

# Restore packages
dotnet restore

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ClientTests"

# Run tests by category/trait
dotnet test --filter "Category=Unit"
```

## Test Coverage

### Domain Entity Tests
- ✅ **Client** - Properties, interfaces, archiving
- ✅ **Resource** - Properties, navigation properties, relationships
- ✅ **Unit** - Properties, interfaces, archiving
- ✅ **ReceiptDocument** - Document management, items collection
- ✅ **ReceiptItem** - Foreign keys, quantities, relationships
- ✅ **ShipmentDocument** - Client relationships, status management
- ✅ **ShipmentItem** - Document relationships, quantities
- ✅ **Balance** - Stock representation, decimal precision
- ✅ **ShipmentStatus** - Enum values and conversions

### Integration Tests
- ✅ **Domain Model Integration** - Interface implementations, relationships
- ✅ **Entity Relationships** - Complex scenarios, data integrity

### Helper Classes
- ✅ **TestDataBuilder** - Factory methods for test data creation
- ✅ **TestAssertions** - Custom assertion extensions

## Test Data Generation

The `TestDataBuilder` class provides factory methods for creating valid test entities:

```csharp
// Create individual entities
var client = TestDataBuilder.CreateValidClient();
var resource = TestDataBuilder.CreateValidResource();

// Create entities with specific properties
var client = TestDataBuilder.CreateValidClient(name: "Test Client", isArchived: false);

// Create complex scenarios
var documentWithItems = TestDataBuilder.CreateReceiptDocumentWithItems(itemCount: 5);
var resourceWithRelations = TestDataBuilder.CreateResourceWithRelatedEntities();
```

## Custom Assertions

The `TestAssertions` class provides domain-specific assertions:

```csharp
// Verify interface implementation
typeof(Client).ShouldImplementInterface<IArchivable>();

// Verify property structure
typeof(Client).ShouldHaveProperty("Name", typeof(string));

// Verify audit trail patterns
typeof(ReceiptDocument).ShouldFollowAuditTrailPattern();

// Verify relationships
typeof(ReceiptItem).ShouldHaveForeignKeyFor("Document");
```

## Writing New Tests

### Unit Test Example
```csharp
[Fact]
public void MyEntity_ShouldDoSomething()
{
    // Arrange
    var entity = TestDataBuilder.CreateValidMyEntity();
    
    // Act
    var result = entity.DoSomething();
    
    // Assert
    result.Should().NotBeNull();
    result.SomeProperty.Should().Be(expectedValue);
}
```

### Theory Test Example
```csharp
[Theory]
[InlineData("value1", true)]
[InlineData("value2", false)]
public void MyEntity_ShouldHandleValues(string input, bool expected)
{
    // Arrange & Act
    var entity = new MyEntity { Property = input };
    
    // Assert
    entity.IsValid.Should().Be(expected);
}
```

### AutoData Test Example
```csharp
[Theory]
[AutoData]
public void MyEntity_ShouldSetProperties(string name, int value)
{
    // Arrange & Act
    var entity = new MyEntity { Name = name, Value = value };
    
    // Assert
    entity.Name.Should().Be(name);
    entity.Value.Should().Be(value);
}
```

## Test Categories

Tests can be categorized using the `[Trait]` attribute:

```csharp
[Fact]
[Trait("Category", "Unit")]
public void MyUnitTest() { /* ... */ }

[Fact]
[Trait("Category", "Integration")]
public void MyIntegrationTest() { /* ... */ }
```

Then run specific categories:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## Continuous Integration

The test project is configured to run in CI/CD pipelines:

- All tests must pass before deployment
- Code coverage reports are generated
- Test results are published to the build summary

## Best Practices

1. **AAA Pattern** - Arrange, Act, Assert
2. **One Assertion Per Test** - Keep tests focused
3. **Descriptive Names** - Test names should describe what is being tested
4. **Independent Tests** - Tests should not depend on each other
5. **Use TestDataBuilder** - For consistent test data creation
6. **FluentAssertions** - For readable assertions
7. **Mock External Dependencies** - Use Moq for isolation

## Troubleshooting

### Common Issues

1. **Tests not discovered** - Ensure test methods are public and marked with `[Fact]` or `[Theory]`
2. **Assembly not found** - Check project references in `Tests.csproj`
3. **Data generation issues** - Verify AutoFixture customizations

### Debug Tests
- Set breakpoints in test methods
- Use `dotnet test --logger "console;verbosity=diagnostic"` for detailed output
- Check test output window in Visual Studio

## Contributing

When adding new features:
1. Write tests first (TDD approach)
2. Ensure good test coverage for new code
3. Update this README if adding new test categories or patterns
4. Follow the existing test structure and naming conventions
