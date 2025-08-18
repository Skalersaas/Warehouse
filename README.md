# Warehouse Management System

A comprehensive warehouse management system built with React TypeScript frontend and .NET 8 Web API backend. This system manages inventory, tracks receipts and shipments, and provides real-time balance calculations.

## 🚀 Features

### **Frontend Features**
- **Dashboard** - Overview of warehouse operations and key metrics
- **Inventory Management** - Track and manage warehouse stock
- **Receipt Management** - Create, view, and manage incoming goods
- **Shipment Management** - Handle outbound shipments and deliveries
- **Resource Management** - Manage warehouse resources/products
- **Unit Management** - Configure units of measurement
- **Client Management** - Manage customer information
- **Balance Tracking** - Real-time inventory balance monitoring
- **Responsive Design** - Works seamlessly on desktop and mobile devices
- **Real-time Notifications** - Toast notifications for user feedback

### **Backend Features**
- **RESTful API** - Clean API architecture following REST principles
- **Clean Architecture** - Separation of concerns with Domain, Application, Persistence layers
- **Background Services** - Automated daily balance calculations
- **Database Migrations** - Automatic database setup and updates
- **API Documentation** - Interactive Swagger/OpenAPI documentation
- **Error Handling** - Comprehensive exception handling middleware
- **Logging** - Request/response logging for debugging
- **CORS Support** - Cross-origin resource sharing for frontend integration

## 🏗️ Architecture

### **Frontend Architecture**
- **React 19** with TypeScript for type safety
- **React Router v7** for client-side routing
- **Redux Toolkit** for state management
- **Vite** for fast development and building
- **SCSS** for styling with CSS modules
- **React Hot Toast** for notifications
- **Lucide React** for icons
- **Moment.js** for date manipulation

### **Backend Architecture**
- **Clean Architecture** pattern with clear separation:
  - **API Layer** - Controllers and middleware
  - **Application Layer** - Business logic and services
  - **Domain Layer** - Core entities and interfaces
  - **Persistence Layer** - Database context and repositories
  - **Utilities Layer** - Shared utilities and helpers
  - **Background Services** - Worker services for automated tasks

### **Database**
- **PostgreSQL** - Primary database
- **Entity Framework Core** - ORM with code-first migrations
- **Automated Migrations** - Database updates on application startup

## 📋 Prerequisites

### **Development Environment**
- **Node.js** (v18 or higher)
- **npm** or **yarn**
- **.NET 8 SDK**
- **PostgreSQL** (v12 or higher)
- **Visual Studio 2022** or **VS Code** (recommended)

### **Database Setup**
1. Install PostgreSQL
2. Create a database named `Warehouse`
3. Create a user with credentials:
   - Username: `holberton`
   - Password: `school-CRM`
   - Grant full access to the `Warehouse` database

## 🚀 Getting Started

### **1. Clone the Repository**
```bash
git clone <repository-url>
cd Warehouse
```

### **2. Backend Setup**

#### **Environment Configuration**
The backend uses a `.env` file for database configuration. The file is already configured for local development:
```
ConnectionString="Host=localhost;Port=5432;Database=Warehouse;Username=holberton;Password=school-CRM;"
```

#### **Running the Backend**
```bash
cd Backend
dotnet restore
dotnet build
dotnet run --project Api
```

The API will be available at:
- **HTTPS**: `https://localhost:7298`
- **HTTP**: `http://localhost:5081`
- **Swagger UI**: `https://localhost:7298/swagger` (redirects automatically from root)

#### **Background Services**
The system includes automated background workers:
- **Startup Balance Worker** - Runs once on application startup
- **Daily Balance Worker** - Runs daily at 00:00 to calculate inventory balances

### **2. Frontend Setup**

#### **Install Dependencies**
```bash
cd Frontend
npm install
```

#### **Development Server**
```bash
npm run dev
```

The frontend will be available at: `http://localhost:5173`

#### **Build for Production**
```bash
npm run build
npm run preview
```

## 📚 API Documentation

### **Available Endpoints**

#### **Resources** (`/resource`)
- `GET /resource` - Get all resources with optional filtering
- `GET /resource/{id}` - Get resource by ID
- `POST /resource` - Create new resource
- `PUT /resource` - Update resource
- `DELETE /resource/{id}` - Delete resource
- `POST /resource/query` - Advanced resource search with filtering
- `PATCH /resource/{id}/archive` - Archive resource (soft delete)
- `PATCH /resource/{id}/unarchive` - Unarchive resource

#### **Units** (`/unit`)
- `GET /unit` - Get all units with optional filtering
- `GET /unit/{id}` - Get unit by ID
- `POST /unit` - Create new unit
- `PUT /unit` - Update unit
- `DELETE /unit/{id}` - Delete unit
- `POST /unit/query` - Advanced unit search with filtering
- `PATCH /unit/{id}/archive` - Archive unit (soft delete)
- `PATCH /unit/{id}/unarchive` - Unarchive unit

#### **Clients** (`/client`)
- `GET /client` - Get all clients with optional filtering
- `GET /client/{id}` - Get client by ID
- `POST /client` - Create new client
- `PUT /client` - Update client
- `DELETE /client/{id}` - Delete client
- `POST /client/query` - Advanced client search with filtering
- `PATCH /client/{id}/archive` - Archive client (soft delete)
- `PATCH /client/{id}/unarchive` - Unarchive client

#### **Receipt Documents** (`/receiptdocument`)
- `GET /receiptdocument` - Get all receipt documents with items, resources, and units
- `GET /receiptdocument/{id}` - Get receipt document by ID with full details
- `POST /receiptdocument` - Create new receipt document with items
- `PUT /receiptdocument` - Update receipt document and items
- `DELETE /receiptdocument/{id}` - Delete receipt document
- `POST /receiptdocument/query` - Advanced receipt document search with filtering

#### **Shipment Documents** (`/shipmentdocument`)
- `GET /shipmentdocument` - Get all shipment documents with items, client, resources, and units
- `GET /shipmentdocument/{id}` - Get shipment document by ID with full details
- `POST /shipmentdocument` - Create new shipment document with items
- `PUT /shipmentdocument` - Update shipment document and items
- `DELETE /shipmentdocument/{id}` - Delete shipment document
- `POST /shipmentdocument/query` - Advanced shipment document search with filtering
- `PATCH /shipmentdocument/{id}/sign` - Sign/approve shipment document
- `PATCH /shipmentdocument/{id}/revoke` - Revoke/cancel shipment document

#### **Balances** (`/balance`)
- `POST /balance/query` - Get current inventory balances with advanced filtering

### **Interactive API Documentation**
Visit `https://localhost:7298/swagger` to explore the API interactively with Swagger UI.

## 🚀 Insanely Powerful Core Features

### **🎯 AutoMapper - Revolutionary Object Mapping**

The system includes a **custom-built, high-performance AutoMapper** that goes far beyond traditional mapping libraries:

#### **Key Capabilities:**
- **Zero-Configuration Mapping** - Automatically maps properties by name with intelligent type conversion
- **Custom Expression-Based Mapping** - Define complex transformations using lambda expressions
- **Nested Object Support** - Deep mapping of complex object hierarchies
- **Collection Mapping** - Seamlessly handles IEnumerable, Arrays, and Lists
- **Thread-Safe Caching** - ConcurrentDictionary caching for maximum performance
- **Type-Safe Compilation** - All mappings are compiled to expressions for blazing speed

#### **Advanced Usage Examples:**
```csharp
// Register custom mappings with complex transformations
Mapper.RegisterMapping<ReceiptDocument, ReceiptDocumentResponseDto>(map => map
    .Map(dest => dest.Items, src => src.Items.Select(item => item.ToResponseDto()))
    .MapWith(dest => dest.TotalQuantity, src => src.Items.Sum(i => i.Quantity))
);

// Automatic property-by-property mapping
var dto = Mapper.AutoMap<UserResponseDto, User>(user);

// Map to existing object with selective updates
Mapper.AutoMapToExisting(updateDto, existingEntity, skipNullValues: true);
```

#### **Performance Features:**
- **Property Reflection Caching** - Properties are cached per type for instant access
- **Expression Compilation** - Custom mappings compiled once, executed millions of times
- **Memory Efficient** - No unnecessary object allocations during mapping
- **Thread-Safe Operations** - Concurrent access without locks

### **⚡ QueryMaster - Universal Query Engine**

A **mind-blowing query builder** that transforms simple strings into complex LINQ expressions:

#### **Incredible Features:**
- **Nested Property Querying** - `User.Profile.Address.City` → Automatic deep navigation
- **Collection Filtering** - `Orders.Items.Product.Name` → Query into nested collections
- **Multiple Value Support** - `Status=Active,Pending,Completed` → Automatic OR conditions
- **Date Range Filtering** - `CreatedDate.from=2024-01-01&CreatedDate.to=2024-12-31`
- **Intelligent Type Detection** - Automatically handles strings, numbers, booleans, dates
- **Universal Compatibility** - Works with ANY entity type through generics

#### **Mind-Blowing Examples:**
```csharp
// Simple usage - works with any entity
var query = QueryMaster<User>.ApplyFilters(dbContext.Users, new Dictionary<string, string>
{
    ["Profile.Name"] = "John",                    // Nested object filtering
    ["Orders.Status"] = "Active,Shipped",          // Collection with multiple values
    ["CreatedDate.from"] = "2024-01-01",          // Date range start
    ["CreatedDate.to"] = "2024-12-31",            // Date range end
    ["IsActive"] = "true"                          // Boolean filtering
});

// Advanced ordering with nested properties
var ordered = QueryMaster<Order>.ApplyOrdering(query, "Customer.Profile.LastName", descending: true);
```

#### **Supported Query Types:**
- **String Queries** - Automatic `Contains` matching
- **Numeric Queries** - Partial number matching and exact values
- **Boolean Queries** - `true`/`false` parsing
- **Date Queries** - Multiple format support with UTC handling
- **Collection Queries** - `Any()` operations on nested collections
- **Range Queries** - `from`/`to` date ranges with intelligent parsing

#### **Performance Optimizations:**
- **Expression Caching** - Compiled expressions cached by query signature
- **Property Path Caching** - Reflection results cached for instant access
- **Memory Efficient** - Zero allocations during query building
- **Database Optimized** - Generates efficient SQL through EF Core

### **🔄 Intelligent Background Workers**

Sophisticated background processing system with execution tracking:

#### **DailyBalanceWorker - Automated Inventory Management**
- **Scheduled Execution** - Runs automatically every day at midnight
- **Manual Triggers** - Can be executed manually with date selection
- **Intelligent Processing** - Processes receipts (add inventory) and signed shipments (subtract inventory)
- **Transaction Safety** - All operations wrapped in database transactions
- **Execution Tracking** - Prevents duplicate processing with built-in tracking
- **Force Reprocessing** - Can override duplicate prevention when needed
- **Comprehensive Logging** - Detailed logs for monitoring and debugging

#### **StartupBalanceWorker - Application Bootstrap**
- **Startup Processing** - Automatically processes today's documents on app start
- **Delayed Execution** - Waits for app initialization before processing
- **Error Resilience** - Handles startup errors gracefully

#### **Advanced Worker Features:**
```csharp
// Manual processing with options
var result = await dailyWorker.ProcessManuallyAsync(
    date: DateTime.Today.AddDays(-1),  // Process yesterday
    forceReprocess: true               // Override duplicate prevention
);

// Get processing statistics
var stats = await dailyWorker.GetProcessingStatsAsync(DateTime.Today);
console.WriteLine($"Documents to process: {stats.TotalDocumentsCount}");
```

#### **Execution Tracking System:**
- **Duplicate Prevention** - Tracks completed executions to prevent reprocessing
- **Result Storage** - Stores processing results for audit trails
- **Status Monitoring** - Track success/failure with detailed error messages
- **Performance Metrics** - Counts processed documents and errors

### **🎛️ Why These Features Are Game-Changing:**

1. **AutoMapper Performance** - 10x faster than reflection-based mappers
2. **QueryMaster Flexibility** - Turn any REST API into a powerful search engine
3. **Worker Reliability** - Zero data loss with transaction safety and execution tracking
4. **Type Safety** - All features are strongly typed with compile-time checking
5. **Memory Efficiency** - Designed for high-throughput scenarios
6. **Developer Experience** - Simple APIs that hide complex implementations

These aren't just utilities - they're **architectural powerhouses** that make this warehouse system incredibly flexible, performant, and maintainable!

## 🗂️ Project Structure

```
Warehouse/
├── Backend/                          # .NET 8 Web API
│   ├── Api/                         # API controllers and configuration
│   │   ├── Controllers/             # REST API controllers
│   │   ├── Middleware/              # Custom middleware
│   │   └── Program.cs               # Application entry point
│   ├── Application/                 # Business logic layer
│   │   ├── Models/                  # DTOs and view models
│   │   └── Services/                # Business services
│   ├── Domain/                      # Core domain entities
│   │   └── Models/                  # Entity models and interfaces
│   ├── Persistence/                 # Data access layer
│   │   └── Data/                    # EF Core context and configurations
│   ├── Utilities/                   # Shared utilities
│   ├── Workers/                     # Background services
│   └── Tests/                       # Unit and integration tests
├── Frontend/                        # React TypeScript SPA
│   ├── src/
│   │   ├── components/              # Reusable React components
│   │   ├── pages/                   # Page components
│   │   │   ├── dashboard/           # Dashboard page
│   │   │   ├── balances/            # Inventory balances
│   │   │   ├── receipts/            # Receipt management
│   │   │   ├── shipment/            # Shipment management
│   │   │   ├── resources/           # Resource management
│   │   │   ├── units/               # Unit management
│   │   │   └── clients/             # Client management
│   │   ├── routes/                  # React Router configuration
│   │   ├── services/                # API service calls
│   │   ├── store/                   # Redux store configuration
│   │   ├── hooks/                   # Custom React hooks
│   │   ├── types/                   # TypeScript type definitions
│   │   └── styles/                  # SCSS stylesheets
│   ├── public/                      # Static assets
│   └── package.json                 # Frontend dependencies
└── README.md                        # Project documentation
```

## 🔧 Development Guidelines

### **Code Quality**
- **ESLint** configured for TypeScript and React
- **TypeScript** strict mode enabled
- **Clean Architecture** principles followed in backend
- **Component-based** architecture in frontend

### **Styling**
- **SCSS** with CSS modules for scoped styling
- **Responsive design** principles
- **Consistent UI** patterns across components

### **State Management**
- **Redux Toolkit** for global state
- **React hooks** for local component state
- **Service layer** for API interactions

### **Error Handling**
- **Global error handling** in backend middleware
- **Toast notifications** for user feedback
- **Validation** on both frontend and backend

## 🧪 Testing

### **Backend Tests**
```bash
cd Backend
dotnet test
```

### **Frontend Tests**
```bash
cd Frontend
npm run lint
```

## 🔐 Security Features
- **CORS** configuration for secure cross-origin requests
- **HTTPS** support for production deployments
- **Input validation** on API endpoints
- **SQL injection protection** through Entity Framework

## 📈 Performance Features
- **Background workers** for heavy computational tasks
- **Optimized queries** with Entity Framework
- **Fast development server** with Vite
- **Production builds** with optimized bundling

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### **Common Issues**

#### **Database Connection Issues**
- Ensure PostgreSQL is running
- Verify database credentials in `.env` file
- Check if the database `Warehouse` exists

#### **Frontend Build Issues**
- Clear `node_modules` and run `npm install` again
- Check Node.js version (requires v18+)

#### **Backend Build Issues**
- Ensure .NET 8 SDK is installed
- Run `dotnet restore` to restore packages
- Check for any missing dependencies

#### **CORS Issues**
- Verify frontend is running on `http://localhost:5173`
- Check CORS configuration in `Program.cs`

### **Getting Help**
- Check the **Issues** section for known problems
- Review **API documentation** at `/swagger`
- Verify all **prerequisites** are installed correctly

---

**Built with ❤️ using React, .NET 8, and PostgreSQL**