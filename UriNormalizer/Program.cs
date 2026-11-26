// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Official_Examples_Tests;
using UriParserTest;
using Xunit;


internal class Program
{
    static void Main(string[] args)
    {
        int allPassed = 0;
        int allTotal = 0;

        OfficialTestRunner.Start(args, out int passed, out int total);
        allPassed += passed;
        allTotal += total;

        UriParserTestRunner.Start(args, out passed, out total);
        allPassed += passed;
        allTotal += total;


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"PROGRAM FINISHED WITH {allPassed}/{allTotal} TESTS PASSED");
        Console.ResetColor();
    }
}
