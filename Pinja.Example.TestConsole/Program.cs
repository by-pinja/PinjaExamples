using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pinja.Examples.ConventionBasedFactories;

namespace Pinja.Example.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var factory = new MessageHandlerFactory(loggerFactory, null);


        }
    }
}
