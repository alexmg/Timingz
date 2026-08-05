

# Timingz

![Timingz Logo](https://raw.githubusercontent.com/alexmg/Timingz/develop/icon.png)

[![Continuous Integration](https://github.com/alexmg/Timingz/actions/workflows/continuous-integration.yml/badge.svg?branch=develop)](https://github.com/alexmg/Timingz/actions/workflows/continuous-integration.yml)
[![Coverage Status](https://img.shields.io/coveralls/github/alexmg/Timingz/develop)](https://coveralls.io/github/alexmg/Timingz?branch=develop)
[![CodeQL](https://github.com/alexmg/Timingz/actions/workflows/codeql-analysis.yml/badge.svg?branch=develop)](https://github.com/alexmg/Timingz/actions/workflows/codeql-analysis.yml)
[![Nuget](https://img.shields.io/nuget/v/Timingz?label=NuGet)](https://www.nuget.org/packages/Timingz)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/alexmg/Timingz/blob/master/LICENSE)
[![Open in VS Code](https://img.shields.io/badge/Open%20in%20VS%20Code-blue?logo=visualstudiocode)](https://open.vscode.dev/alexmg/Timingz)

Timingz es una implementación de middleware para ASP.NET Core diseñada para grabar y comunicar métricas del servidor backend, tal como se describe en la especificación [Server Timing](https://www.w3.org/TR/server-timing/). Estas métricas pueden consultarse mediante las herramientas de desarrollo del navegador y accederse utilizando la interfaz [PerformanceServerTiming](https://developer.mozilla.org/en-US/docs/Web/API/PerformanceServerTiming).

![alt Timing](https://raw.githubusercontent.com/alexmg/Timingz/master/assets/Timing.png)

## Características

- La configuración predeterminada prioriza la seguridad y la funcionalidad debe habilitarse explícitamente para evitar exponer información potencialmente sensible
- Las funciones pueden activarse o desactivarse a nivel de solicitud individual utilizando cualquier información disponible en el `HttpContext` de la solicitud:
  - Habilitar/deshabilitar la escritura del encabezado `Server-Timing`
  - Incluir descripciones de métricas en el encabezado o excluirlas para reducir su tamaño
  - Incluir solo la duración total de la solicitud o agregar métricas personalizadas para mayor detalle
- Existen diferentes tipos de métricas disponibles para cubrir una variedad de escenarios de uso:
  - Las métricas `Manual` pueden iniciarse y detenerse manualmente múltiples veces
  - Las métricas `Disposable` pueden envolverse en una instrucción `using`, capturando la duración al final del bloque
  - Las métricas `Marker` no contienen una duración, pero pueden indicar eventos significativos como un fallo de caché
  - Las métricas `Precalculated` pueden registrar valores capturados desde un mecanismo de medición existente
- Las métricas pueden validarse para resaltar errores lógicos que podrían generar registros de tiempo inválidos o engañosos
- Además de enviarse en el encabezado `Server-Timing`, las métricas capturadas durante la solicitud también pueden enviarse a otro servicio o paquete de métricas una vez completada la solicitud
- La integración con la [.NET Activity API](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Api/README.md#instrumenting-a-libraryapplication-with-net-activity-api) y [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet) permite incluir métricas existentes basadas en `Activity` en el encabezado `Server-Timing` y exportarlas a exportadores de OpenTelemetry
- El encabezado `Timing-Allow-Origin` puede incluirse con dominios configurables
- Los valores del encabezado se escriben utilizando la biblioteca [ZString (Zero Allocation StringBuilder)](https://github.com/Cysharp/ZString) para minimizar las asignaciones de memoria

## Primeros pasos

Instale el paquete `Timingz` desde NuGet en su aplicación web ASP.NET Core utilizando su mecanismo preferido.

Comando de PowerShell:

```powershell
Install-Package Timingz
```

CLI de .NET Core:

```powershell
dotnet add package Timingz
```

Actualice `ConfigureServices` en su clase `Startup` o en el constructor de host para incluir una llamada al método de extensión `AddServerTiming` en `IServiceCollection`. Esto agregará los servicios requeridos al contenedor de DI y hará disponible el servicio `IServerTiming` para su código. También puede registrar sus propias implementaciones de `IServerTimingCallback` aquí con la vida útil de servicio correspondiente.

```c#
// This call is mandatory.
services.AddServerTiming();

// This call is only required if you have callback services.
services.AddSingleton<IServerTimingCallback, SampleServerTimingCallback>();
```

Actualice `Configure` en su clase `Startup` o en el constructor de host para incluir una llamada al método de extensión `UseServerTiming` en `IApplicationBuilder`. Esto agregará el middleware a la canalización de ASP.NET Core y debe colocarse antes de `UseEndpoints` si este existe. El método `UseServerTiming` acepta una devolución de llamada para configurar las opciones. Algunas opciones son globales y otras son específicas por solicitud.

```c#
// Add the middleware before UseEndpoints.
app.UseServerTiming(options =>
{
    // Configure the per-request options.
    options.WithRequestTimingOptions((httpContext, requestOptions) =>
    {
        // The use of a query string parameter is only for demonstration.
        var query = httpContext.Request.Query;

        // Enable/disable the Server-Timing header.
        requestOptions.WriteHeader = query.ContainsKey("timing");

        // Enable/disable the inclusion of descriptions in the header.
        requestOptions.IncludeDescriptions = query.ContainsKey("desc");

        // Enable/disable the inclusion of custom metric values.
        requestOptions.IncludeCustomMetrics = query.ContainsKey("custom");
    });

    // Add any required domains for the Timing-Allow-Origin header.
    options.TimingAllowOrigins = new[] {"https://example.com"};

    // Enable/disable the invocation of callback services.
    options.InvokeCallbackServices = true;

    // Enable/disable the validation of metrics.
    options.ValidateMetrics = env.IsDevelopment();
    
    // Precision of duration values written to header (default 3). 
    options.DurationPrecision = 3;
});
```

## Uso del servicio `IServerTiming`

El servicio `IServerTiming` se registra con un `ServiceLifetime` de `Scoped`, lo que significa que se crea una nueva instancia para cada solicitud HTTP. Esto permite inyectar la misma instancia del servicio en diferentes componentes utilizados durante el procesamiento de la solicitud HTTP. El servicio `IServerTiming` no debe inyectarse en servicios con un `ServiceLifetime` de `Singleton` para evitar una [dependencia cautiva](https://blog.ploeh.dk/2014/06/02/captive-dependency/).

El servicio es seguro para su uso concurrente desde múltiples hilos durante una solicitud, pero una métrica con un nombre determinado solo puede agregarse una vez. Un intento de crear una métrica con un nombre duplicado lanzará una excepción.

### Marker

El método `Marker` se utiliza para agregar una métrica que tiene un nombre pero no incluye una duración. Estas métricas funcionan como etiquetas que indican algo relevante, como la ocurrencia de un fallo de caché.

```c#
serverTiming.Marker("miss", "Cache miss");
```

### Manual

El método `Manual` devuelve una métrica que puede iniciarse y detenerse manualmente múltiples veces. Esto le permite sumar la duración total de una actividad que se intercala entre otras llamadas.

```c#
var databaseMetric = serverTiming.Manual("db", "Database queries");

databaseMetric.Start();
// The first database query.
databaseMetric.Stop();

// Some other code.

databaseMetric.Start();
// The second database query.
databaseMetric.Stop();
```

### Disposable

El método `Disposable` devuelve una métrica que ya se encuentra en ejecución y registrará su duración al finalizar el bloque de una instrucción `using`. Esto resulta útil cuando el código que se mide se ejecuta dentro de una única sección.

```c#
using (serverTiming.Disposable("bus", "Send notification to bus"))
{
    // Send notification to bus.
}
```

### Precalculated

El método `Precalculated` se utiliza para registrar una métrica capturada mediante un mecanismo externo, como otro paquete de métricas. Esto le permite continuar utilizando un paquete de métricas existente e incluir sus valores en el encabezado `Server-Timing`.

```c#
var duration = GetTimingFromOtherPackage();
serverTiming.Precalculated("external", duration, "External metric timing");
```

### GetMetrics

Este método devuelve una lista de las métricas registradas y puede utilizarse con fines de depuración.

> Cabe señalar que, si se habilita la métrica de duración total de la solicitud, esta no estará presente hasta que haya comenzado la respuesta HTTP.

```c#
var metrics = serverTiming.GetMetrics();
foreach (var metric in metrics)
    await Console.Out.WriteLineAsync(
        $"- Name: {metric.Name}, Description: {metric.Description}, Duration: {metric.Duration}");
```

## Integración con la .NET Activity API

Cree un `ActivitySource` singleton que pueda reutilizar en toda su aplicación o biblioteca siguiendo estas [instrucciones](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Api/README.md#instrumenting-a-libraryapplication-with-net-activity-api).

```c#
internal static class Telemetry
{
    internal static readonly ActivitySource Source = new("MySource");
}
```

Agregue los nombres de `ActivitySource` que deben monitorearse a `ActivitySources` en `ServerTimingOptions`.

```c#
// Add the Activity Source that should be monitored for AddServerTiming calls. 
options.ActivitySources.Add(Telemetry.Source.Name);
```

Al utilizar un `Activity`, puede llamar al método de extensión `AddServerTiming` para incluir la duración en el encabezado `Server-Timing`.

```c#
using (Telemetry.Source.StartActivity("Database").AddServerTiming("Queries and caching"))
{
    // Perform database operations and cache results
}
```

El nombre de la métrica en el encabezado `Server-Timing` será el `Name` del `Activity`. Puede proporcionar una descripción para la métrica en el encabezado pasando un valor al parámetro opcional `description` del método de extensión `AddServerTiming`.

En el ejemplo anterior, la métrica resultante en el encabezado `Server-Timing` se llamará *Database* y tendrá un `desc` de *Queries and caching*.

```
Database;dur=94.4693;desc="Queries and caching"
```

Los servicios requeridos para el monitoreo de Activity se agregan al llamar a `AddServerTiming` en `IServiceCollection`. Esta llamada sigue siendo necesaria incluso cuando no se utiliza el servicio `IServerTiming` directamente.

```c#
services.AddServerTiming();
```

## Implementación de un servicio `IServerTimingCallback`

Un servicio de devolución de llamada debe implementar la interfaz `IServerTimingCallback`, la cual contiene un único método `OnServerTiming`. Este método recibe una instancia de `ServerTimingEvent` con propiedades para el `HttpContext` y una lista de instancias de `IMetric`. La lista contendrá todas las instancias de `IMetric` registradas con sus descripciones, incluidas las métricas personalizadas, independientemente de si estas se configuraron para incluirse en el encabezado `Server-Timing`. Esto le permite capturar estas métricas en otro servicio o paquete incluso si no desea enviarlas en el encabezado `Server-Timing`.

```c#
public class SampleServerTimingCallback : IServerTimingCallback, IAsyncDisposable
{
    public async Task OnServerTiming(ServerTimingEvent serverTimingEvent)
    {
        var displayUrl = serverTimingEvent.Context.DisplayUrl;
        var metrics = serverTimingEvent.Metrics;
        await Console.Out.WriteLineAsync($"Server-Timing for {displayUrl} has {metrics.Count} metrics");

        foreach (var metric in metrics)
            await Console.Out.WriteLineAsync(
                $"Name: {metric.Name}, Description: {metric.Description}, Duration: {metric.Duration}");
    }

    public async ValueTask DisposeAsync() => await Console.Out.WriteLineAsync("Disposing callback");
}
```

Ejemplo de salida del servicio de devolución de llamada anterior.

```
Server-Timing for http://localhost:5000/api/sample?timing&desc&custom has 6 metrics:
- Name: external, Description: External metric timing, Duration: 25
- Name: miss, Description: Cache miss, Duration:
- Name: cache, Description: Cache writes, Duration: 39.321600000000004
- Name: db, Description: Database queries, Duration: 61.916700000000006
- Name: total, Description: Total, Duration: 166.9794
- Name: bus, Description: Send notification to bus, Duration: 22.0557
Disposing callback
```

## Visualizador de Postman

Las herramientas de desarrollo del navegador incluyen una visualización nativa para las métricas en el encabezado `Server-Timing`, pero a menudo las solicitudes de API se prueban en herramientas como Postman. La carpeta de ejemplos contiene una colección de Postman con un visualizador que permite representar las métricas en un gráfico de barras horizontal, similar al de las herramientas de desarrollo del navegador.

Haga clic en el botón `Visualize` al visualizar el cuerpo de la respuesta en Postman para ver el gráfico. El visualizador se aplica a nivel de colección y funcionará con todas las solicitudes dentro de ella. Puede anular el visualizador a nivel de colección para una solicitud específica utilizando el método `pm.visualizer.set`.

El visualizador mostrará un mensaje cuando la respuesta no contenga un encabezado `Server-Timing` o no haya valores presentes en él. Las métricas que tienen un nombre pero no una duración se listarán en la parte superior de la visualización para proporcionar contexto a los valores del gráfico.

![alt Postman Visualizer](https://raw.githubusercontent.com/alexmg/Timingz/master/assets/Postman-visualizer.png)

## Créditos

Icono realizado por [Freepik](https://www.flaticon.com/authors/freepik) de [www.flaticon.com](https://www.flaticon.com/)
