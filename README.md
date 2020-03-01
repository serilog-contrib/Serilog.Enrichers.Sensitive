# Serilog.Enrichers.Sensitive

This is a Serilog enricher that can mask sensitive data from a `LogEvent` message template and its properties. Currently this supports e-mail addresses and IBAN numbers but could easily be extended to other types of data.

There are two ways of using this enricher:

- Always mask sensitive data (default behaviour)
- Mask data in sensitive areas only

See [Usage](#usage) below on how to configure this.

## Possible use case

Let's say you have written a request/response logging middleware for ASP.Net Core that outputs:

`Request start {method} {url}`
`End {method} {status_code} {url} {duration}`

Here you have the potential that the `url` property contains sensitive data because someone might do `GET /api/users/?email=james.bond@universalexports.co.uk`.

Of course you can write your logging middleware to capture this and that may be the best place in this situation. However there might be cases where you don't know this is likely to happen and then you end up with the e-mail address in your logging platform.

When using this enricher what you will get is that the log message that used to be:

`Request start GET /api/users/?email=james.bond@universalexports.co.uk`

will be:

`Request start GET /api/users/?email=***MASKED***`

### It does not end here

Even though that you know the sensitive data will be masked, it is good practice to not log sensitive data *at all*.

The good thing is that with the masking applied you can add an alert to your logging platform that scans for `***MASKED***` and gives you feedback when sensitive data has been detected. That allows you to fix the problem where it originates (the logging middleware).

## Usage

### Always mask sensitive data

Configure your logger with the enricher:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.WithSensitiveDataMasking()
    .WriteTo.Console()
    .CreateLogger();
```

If you then have a log message that contains sensitive data:

```csharp
logger.Information("This is a sensitive {Email}", "james.bond@universalexports.co.uk");
```

the rendered message will be logged as:

`This is a sensitive ***MASKED***`

the structured log event will look like (abbreviated):

```json
{
    "RenderedMessage": "This is a sensitive ***MASKED***",
    "message": "This is a sensitive {Email}",
    "Properties.Email": "***MASKED***"
}
```

### Mask in sensitive areas only

Configure your logger with the enricher:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.WithSensitiveDataMaskingInArea()
    .WriteTo.Console()
    .CreateLogger();
```

in your application you can then define a sensitive area:

```csharp
using(logger.EnterSensitiveArea())
{
    logger.Information("This is a sensitive {Email}", "james.bond@universalexports.co.uk");
}
```

The effect is that the log message will be rendered as:

`This is a sensitive ***MASKED***`

See the [Serilog.Enrichers.Sensitive.Demo](src/Serilog.Enrichers.Sensitive.Demo/Program.cs) app for a code example of the above.
