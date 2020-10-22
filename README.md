# GBroker
gRPC based event broker for microservice architecture applications.


[TOC]


## Overview

### Event broker



### Infrastructure



### Consumption types




## Components

### EventBroker.Client

#### Non ASP.NET Core applications

#### ASP.NET Core applications

```csharp
services.AddEventsService();
services.AddEventsHandlers(Assembly fromAssembly);
services.AddEventHandler(Type handlerType);
services.AddEventHandler<THandler>();
```

### EventBroker.Grpc.Client



### EventBroker.Grpc.Server



### Sample.EventBroker.Api



