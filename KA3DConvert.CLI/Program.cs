using System.CommandLine;
using KA3DConvert;

namespace KA3DConvert.CLI
{

    internal static class Program
    {
        static int Main(string[] args)
        {

            var rootCommand = new RootCommand("KA3DConvert.CLI");

            var fileArgument = new Argument<string>("file", "The input file.");
            var outputOption = new Option<string?>(name: "--output", "The output file."); outputOption.AddAlias("-o");

            rootCommand.Add(fileArgument);
            rootCommand.Add(outputOption);

            rootCommand.SetHandler(Processor.Convert, fileArgument, outputOption);


            return rootCommand.Invoke(args);
        }


    }
}