# Serilog.Enrichers.Sensitive

This is a Serilog enricher that can mask sensitive data from a `LogEvent` message template and its properties. Currently this supports e-mail addresses and IBAN numbers but could easily be extended to other types of data.

With this enricher you can define an area where you are dealing with sensitive data. Optionally you can configure the enricher to mask all sensitive data passing through the logger.

## Usage

Configure your logger with the enricher:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.WithSensitiveDataMasking()
    .WriteTo.Console()
    .CreateLogger();
```

in your application you can then define a sensitive area:

```csharp
using(logger.EnterSensitiveArea())
{
    logger.Information("This is a sensitive {Email}", "joe.blogs@acme.org");
}
```

The effect is that the log message will be rendered as:

`This is a sensitive ***MASKED***`

If you wish to mask sensitive data everywhere configure the logger like so:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.WithSensitiveDataMasking(maskDataGlobally: true))
    .WriteTo.Console()
    .CreateLogger();
```

With this configuration you do not have to declare a sensitive area.

See the [Serilog.Enrichers.Sensitive.Demo](src/Serilog.Enrichers.Sensitive.Demo/Program.cs) app for a code example of the above.
