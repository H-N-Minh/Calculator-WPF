// ------------------------------------------------------------------------------
// © SDK GmbH 
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Official_Examples_Tests;
using Xunit;


internal class Program
{
    static void Main(string[] args)
    {
        //Debug.Assert(AiTestRunner.Start(args) == 0, "AiTests failed");

        OfficialTestRunner.Start(args);
    }
}
