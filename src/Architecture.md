
# Architecture

## Assemblies

- Common
- Core
- Data
- Infrastructure
- Web
- Web.UI
- Test


### Dependency diagram

graph TB;
    Data-->Common
    
    Core-->Common
    Core-->Infrastructure
    Core-->Data
    
    Infrastructure-->Common
    Infrastructure-->Data
    
    Web-->Infrastructure
    Web-->Common
    Web-->Core
    
    Web.UI-->Web
    Web.UI-->Core
    Web.UI-->Infrastructure
    Web.UI-->Common
    Web.UI-->Data
    

### Common
Sits at the bottom of the dependency chart.  Doesn't depend on anything else.  It primarily contains logic that is useful to all tiers of the application.  e.g., string extension methods, encryption helpers etc.

### Data
Contains all things data-related, e.g., EF Core models, repositories for Cosmos DB or Azure Table Storage

### Infrastructure
Infrastructural functionality (often useful to a multitude of applications); e.g., Caching, Event Bus, Logging, Telemetry, Blob storage, Queue messaging, Smtp

### Core
This is the application core; all application-specific functionality should be in here. There could be a facade class at the top that exposes all services of the Core.

### Web
Contains code related to a webhost consumer.  e.g., cookie helpers, middleware etc.

### Web.UI
Contains all the UI-specific code, e.g., MVC pattern bits.  

### Test
The test assembly contains unit tests that cover functionality of all the other assemblies.