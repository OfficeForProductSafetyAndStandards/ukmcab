# Exception handling

The error handling middleware is registered in the pipeline using `app.UseCustomHttpErrorHandling()` and should be registered _before_ `UseRouting`.

This method registers the following middleware:
- UnexpectedExceptionHandlerMiddleware
- DomainExceptionHandlerMiddleware
- PageNotFoundMiddleware

It also maps the following diagnostic URLs (if config item `EnableExceptionHandlingDiagnostics` is absent or `true`, set to `false` explicity to disable diagnostic urls)
- `/__diag/cmd/http-error-handling/unhandledex`
- `/__diag/cmd/http-error-handling/domainex`
- `/__diag/cmd/http-error-handling/permissiondenied`
- `/__diag/cmd/http-error-handling/flush-errors`
- `/__diag/cmd/http-error-handling/current-snapshot`

## UnexpectedExceptionHandlerMiddleware
This handles all unexpected/unhandled exceptions and internally rewrites to the path defined in `HttpErrorOptions.Error500Path` (default=`/500`).  
The HTTP Error 500 page displays a reference number of the logged error record, so the user can report the error, if they wish, and it can be investigated further.

## DomainExceptionHandlerMiddleware
This handles all "domain exceptions"; i.e., business rule related exceptions where an HTTP Error 400 `Bad Request` should be displayed. 
The error message associated with the exception is displayed to the user and no error is logged.  This is a fallback business error handler. 
Normally business rule errors should be handled via explicit UX design. Will rewrite to `HttpErrorOptions.Error400Path` (default=`/400`).
Any purposeful business rule errors should be of the type or derived from `DomainException`.

## Permission denied errors
When a user does not have permission to perform an operation; throw a `PermissionDeniedException` (derived from `DomainException`). 
This will display as `HTTP Error 403 Forbidden` and will rewrite to `HttpErrorOptions.Error403Path` (default=`/403`)

## PageNotFoundMiddleware
This handles `HTTP Error 404` (`Page not found`) errors and will rewrite to `HttpErrorOptions.Error404Path` (default=`/404`)



