# IServiceModule - Modulare Service-Registrierung

Das **`IServiceModule`**-Interface ist das Herzstück von Common.Bootstrap. Es ermöglicht die Organisation von Dependency-Injection-Konfigurationen in unabhängige, wiederverwendbare Module.

## ?? Inhaltsverzeichnis

- [Konzept](#konzept)
- [Interface-Definition](#interface-definition)
- [Grundlegende Verwendung](#grundlegende-verwendung)
- [Praktische Beispiele](#praktische-beispiele)
- [Best Practices](#best-practices)
- [Häufige Muster](#häufige-muster)

## Konzept

### Warum Service-Module?

In größeren Anwendungen kann die DI-Konfiguration schnell unübersichtlich werden:

```csharp
// ? Problematisch: Alles in einer riesigen Methode
public void ConfigureServices(IServiceCollection services)
{
    // 50+ Zeilen Datenbank-Registrierungen
    services.AddDbContext<AppDbContext>();
    services.AddScoped<IUserRepository, UserRepository>();
    // ...
    
    // 30+ Zeilen Messaging-Registrierungen
    services.AddSingleton<IMessageBus, RabbitMQBus>();
    // ...
    
    // 40+ Zeilen Validation-Registrierungen
    services.AddScoped<IValidator<User>, UserValidator>();
    // ...
    
    // Unübersichtlich, schwer zu testen, schwer zu warten
}
```

**Service-Module** lösen dieses Problem durch Aufteilung nach Verantwortungsbereichen:

```csharp
// ? Besser: Organisiert in Module
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);

// Automatisch gefunden und registriert:
// - DatabaseModule
// - MessagingModule
// - ValidationModule
// - ...
```

## Interface-Definition

```csharp
namespace Common.Bootstrap;

/// <summary>
/// Schnittstelle für modulare DI-Registrierungen.
/// Jede Bibliothek implementiert ein Modul, um ihre eigenen Services anzumelden.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Führt alle DI-Registrierungen dieses Moduls aus.
    /// </summary>
    void Register(IServiceCollection services);
}
```

**Das Interface ist bewusst einfach gehalten:**
- Eine einzige Methode `Register`
- Keine Abhängigkeiten
- Keine Konfiguration erforderlich
- Vollständige Kontrolle über Registrierungen

## Grundlegende Verwendung

### 1. Modul implementieren

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

/// <summary>
/// Registriert alle Datenbank-bezogenen Services.
/// </summary>
public sealed class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Repository-Registrierungen
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer("ConnectionString"));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
```

### 2. Module automatisch registrieren

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Scannt die angegebene Assembly nach allen IServiceModule-Implementierungen
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### 3. Services nutzen

```csharp
// Services werden automatisch aufgelöst
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;  // Aus DatabaseModule
        _unitOfWork = unitOfWork;  // Aus DatabaseModule
    }

    public async Task CreateUserAsync(User user)
    {
        _repository.Add(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

## Praktische Beispiele

### Beispiel 1: Infrastruktur-Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

/// <summary>
/// Registriert externe Services und Infrastruktur-Komponenten.
/// </summary>
public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // HTTP-Clients
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        
        // Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();
        
        // Logging
        services.AddLogging();
        
        // File Storage
        services.AddSingleton<IFileStorage, LocalFileStorage>();
    }
}
```

### Beispiel 2: Domain-Services-Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Domain;

/// <summary>
/// Registriert alle Domain-Services und Business-Logik.
/// </summary>
public sealed class DomainServicesModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShippingService, ShippingService>();
        
        // Domain Event Handlers
        services.AddScoped<IEventHandler<OrderCreated>, OrderCreatedHandler>();
        services.AddScoped<IEventHandler<PaymentReceived>, PaymentReceivedHandler>();
        
        // Domain Validators
        services.AddScoped<IValidator<Order>, OrderValidator>();
        services.AddScoped<IValidator<Payment>, PaymentValidator>();
    }
}
```

### Beispiel 3: Bibliotheks-Modul für Wiederverwendung

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyCompany.SharedLibrary;

/// <summary>
/// Registriert alle Services der Shared Library.
/// Kann in mehreren Projekten wiederverwendet werden.
/// </summary>
public sealed class SharedLibraryModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Utilities
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IGuidProvider, GuidProvider>();
        
        // Common Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        
        // Converters
        services.AddSingleton<IJsonConverter, JsonConverter>();
        services.AddSingleton<IXmlConverter, XmlConverter>();
    }
}
```

### Beispiel 4: Konfigurationsbasiertes Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Configuration;

/// <summary>
/// Registriert Services basierend auf Konfiguration.
/// </summary>
public sealed class ConfigurableModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Service Provider temporär bauen, um auf Konfiguration zuzugreifen
        var sp = services.BuildServiceProvider();
        var configuration = sp.GetRequiredService<IConfiguration>();
        
        // Bedingte Registrierung basierend auf Konfiguration
        var useInMemoryDb = configuration.GetValue<bool>("UseInMemoryDatabase");
        
        if (useInMemoryDb)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
```

## Best Practices

### ? Do's

#### 1. Ein Modul pro Verantwortungsbereich

```csharp
// ? Gut: Fokussierte Module
public class DatabaseModule : IServiceModule { }
public class MessagingModule : IServiceModule { }
public class ValidationModule : IServiceModule { }
```

```csharp
// ? Schlecht: Alles in einem Modul
public class EverythingModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Datenbank
        services.AddDbContext<AppDbContext>();
        // Messaging
        services.AddSingleton<IMessageBus, RabbitMQBus>();
        // Validation
        services.AddScoped<IValidator<User>, UserValidator>();
        // ... 200 weitere Zeilen
    }
}
```

#### 2. Klare Namensgebung

```csharp
// ? Gut: Beschreibende Namen
public class DataToolKitServiceModule : IServiceModule { }
public class CommonBootstrapServiceModule : IServiceModule { }
public class UserManagementModule : IServiceModule { }
```

```csharp
// ? Schlecht: Unklare Namen
public class Module1 : IServiceModule { }
public class MyModule : IServiceModule { }
public class ServiceRegistrations : IServiceModule { }
```

#### 3. XML-Dokumentation

```csharp
/// <summary>
/// Registriert alle DataToolKit-Services:
/// - IStorageOptions für verschiedene Entitäten
/// - Repository-Implementierungen (JSON, LiteDB)
/// - IRepositoryFactory
/// </summary>
/// <remarks>
/// Abhängigkeiten:
/// - CommonBootstrapServiceModule muss registriert sein
/// - IEqualityComparer für verwendete Entitäten muss registriert sein
/// </remarks>
public sealed class DataToolKitServiceModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Implementation
    }
}
```

#### 4. Idempotente Registrierungen

```csharp
using Microsoft.Extensions.DependencyInjection.Extensions;

public class SafeModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // TryAdd* verhindert Doppel-Registrierungen
        services.TryAddSingleton<IMyService, MyService>();
        services.TryAddScoped<IMyRepository, MyRepository>();
    }
}
```

### ? Don'ts

#### 1. Keine Seiteneffekte

```csharp
// ? Schlecht: Seiteneffekte in Register()
public class BadModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // ? Dateisystem-Zugriffe
        File.WriteAllText("config.txt", "initialized");
        
        // ? Datenbank-Migrationen
        RunDatabaseMigrations();
        
        // ? Externe API-Aufrufe
        InitializeExternalService();
        
        // ? Nur Registrierungen!
        services.AddSingleton<IMyService, MyService>();
    }
}
```

#### 2. Keine komplexe Logik

```csharp
// ? Schlecht: Komplexe Logik
public class ComplexModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<IConfiguration>();
        
        // ? Zu komplex für ein Modul
        if (DateTime.Now.Hour < 12)
        {
            services.AddSingleton<IService, MorningService>();
        }
        else
        {
            services.AddSingleton<IService, AfternoonService>();
        }
    }
}
```

## Häufige Muster

### Pattern 1: Feature-Toggle-Modul

```csharp
public class FeatureModule : IServiceModule
{
    private readonly bool _enableNewFeature;

    public FeatureModule(bool enableNewFeature = false)
    {
        _enableNewFeature = enableNewFeature;
    }

    public void Register(IServiceCollection services)
    {
        if (_enableNewFeature)
        {
            services.AddScoped<IService, NewServiceImplementation>();
        }
        else
        {
            services.AddScoped<IService, LegacyServiceImplementation>();
        }
    }
}

// Verwendung
builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);
// Oder manuell mit Feature-Toggle
new FeatureModule(enableNewFeature: true).Register(builder.Services);
```

### Pattern 2: Umgebungs-spezifische Module

```csharp
public class DevelopmentModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEmailService, MockEmailService>();
        services.AddSingleton<IPaymentService, MockPaymentService>();
    }
}

public class ProductionModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddSingleton<IPaymentService, StripePaymentService>();
    }
}

// In Program.cs
if (builder.Environment.IsDevelopment())
{
    new DevelopmentModule().Register(builder.Services);
}
else
{
    new ProductionModule().Register(builder.Services);
}
```

### Pattern 3: Composite-Modul

```csharp
/// <summary>
/// Kombiniert mehrere verwandte Module.
/// </summary>
public class FullStackModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Registriert mehrere Sub-Module
        new DatabaseModule().Register(services);
        new MessagingModule().Register(services);
        new CachingModule().Register(services);
    }
}
```

## Weiterführende Themen

- [Modulare Registrierung ?](Modulare-Registrierung.md)
- [EqualityComparer-Management ?](EqualityComparer.md)
- [Zurück zur Übersicht ?](../README.md)
