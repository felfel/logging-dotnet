# logging-dotnet
Highly opinionated structured logging for .NET projects.

## Description

This is a simple logging façade built on top of Serilog (https://github.com/serilog/serilog) which serializes structured logging data and posts it to an HTTP endpoint of your choice.

While you probably don't want to use this out of the box, it may be a good starting point for your own logging. Also have a look at the ExceptionParser class, which has some handy utility methods to parse and unwrap exceptions.
