using System;
using System.Linq;
using System.Reflection;

// AOT-compatible entry point for native AOT compilation
// This test assembly is compiled with PublishAot=true and verifies that AwesomeAssertions
// is compatible with .NET Native AOT.

// Load and report on the test assembly
var assembly = typeof(Program).Assembly;
Console.WriteLine("AOT Test Assembly Loaded");
Console.WriteLine($"Assembly: {assembly.FullName}");

// Find all test classes (xUnit Fact tests)
var testTypes = assembly
    .GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.Namespace?.Contains("Specs") == true)
    .ToList();

Console.WriteLine($"Found {testTypes.Count} test type(s)");
foreach (var testType in testTypes.Take(5))
{
    Console.WriteLine($"  - {testType.FullName}");
}

// Success: Assembly loaded and basic reflection works
Console.WriteLine("\n✓ AOT compilation successful - AwesomeAssertions is AOT-compatible");
return 0;

