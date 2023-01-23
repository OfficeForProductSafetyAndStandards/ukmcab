# Code review check list

- [ ] Use nullable reference types
- [ ] Use `async` / `await`; avoid `Task.Result` or `Task.Wait()` 
- [ ] Guard conditions to be used a top of functions to validate parameters
    - [ ] Use `ArgumentNullException.ThrowIfNull(paramName)` 
- [ ] Unhandled exceptions to use the Guard class.  e.g., if a result always return true: `Guard.IsTrue(result, "The result should be true!")` (this will result in a `HTTP 500 error`; tech details will be logged for further investigation)
- [ ] Types of code to be located in the correct tier according to `Architecture.md`
- [ ] For domain exceptions, (i.e., purposeful exceptions as part of business logic) raise the `DomainException` error.  The message contained will be displayed to the user and the HTTP code will be `400 Bad Request` 
- [ ] There are no errors present in the project.
- [ ] Logging, where applicable is used to display contextual information during runtime.
- [ ] No Console.WriteLine or similar exists
- [ ] Most application-specific logic to be put into service classes inside the Core assembly
- [ ] The code is easy to understand.
- [ ] There is no usage of "magic numbers".
- [ ] `foreach` over `for` loops
- [ ] Constant variables have been used where applicable
- [ ] There are no empty blocks of code or unused variables
- [ ] Methods are named according to what they do and are kept small
- [ ] Async code all the way down
- [ ] Catch clauses are fine grained and catch specific exceptions
- [ ] Error messages should be informative
- [ ] Encryption should be performed by injecting an IDataProtector (for time-limited data use `ToTimeLimitedDataProtector`) 
    (https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/limited-lifetime-payloads?view=aspnetcore-6.0)
- [ ] Never catch a generic exception - allow it to propagate to the host handlers
- [ ] Use file-scoped namespaces
- [ ] Blob handling: Never assume a blob will exist; there is a small chance that Microsoft Defender for Cloud will have deleted a blob, if it's deemed to contain malware.





