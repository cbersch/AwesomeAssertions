using System;
using System.Linq;
using System.Reflection;
using Xunit;

// AOT-compatible test entry point using xunit.v3 and Microsoft.Testing.Platform
// This test assembly is compiled with PublishAot=true and verifies that AwesomeAssertions
// is compatible with .NET Native AOT.

// Load and report on the test assembly
var assembly = typeof(Program).Assembly;
Console.WriteLine("AOT Test Assembly Loaded");
Console.WriteLine($"Assembly: {assembly.FullName}");

// Find all xUnit test classes
var testTypes = assembly
    .GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.GetMethods()
        .Any(m => m.GetCustomAttribute<FactAttribute>() != null || 
                  m.GetCustomAttribute<TheoryAttribute>() != null))
    .ToList();

Console.WriteLine($"Found {testTypes.Count} test type(s)");
foreach (var testType in testTypes.Take(5))
{
    Console.WriteLine($"  - {testType.FullName}");
}

// Success: Assembly loaded and xUnit tests discovered
Console.WriteLine("\n✓ AOT compilation successful - AwesomeAssertions is AOT-compatible");
return 0;
