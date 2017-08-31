using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac;
using Autofac.Integration.Mef;

namespace SimpleCalculator3
{
    public interface ICalculator
    {
        String Calculate(String input);
    }

    public interface IOperation
    {
        int Operate(int left, int right);
    }

    public interface IOperationData
    {
        Char Symbol { get; }
    }

    [Export(typeof(IOperation))]
    [ExportMetadata("Symbol", '+')]
    class Add : IOperation
    {
        public int Operate(int left, int right)
        {
            return left + right;
        }
    }

    [Export(typeof(IOperation))]
    [ExportMetadata("Symbol", '-')]
    class Subtract : IOperation
    {
        public int Operate(int left, int right)
        {
            return left - right;
        }
    }

    [Export(typeof(ICalculator))]
    class MySimpleCalculator : ICalculator
    {
        [ImportMany]
        IEnumerable<Lazy<IOperation, IOperationData>> operations;

        public String Calculate(String input)
        {
            int left;
            int right;
            Char operation;
            int fn = FindFirstNonDigit(input); //finds the operator
            if (fn < 0) return "Could not parse command.";

            try
            {
                //separate out the operands
                left = int.Parse(input.Substring(0, fn));
                right = int.Parse(input.Substring(fn + 1));
            }
            catch
            {
                return "Could not parse command.";
            }

            operation = input[fn];

            foreach (Lazy<IOperation, IOperationData> i in operations)
            {
                if (i.Metadata.Symbol.Equals(operation)) return i.Value.Operate(left, right).ToString();
            }
            return "Operation Not Found!";
        }

        private int FindFirstNonDigit(String s)
        {

            for (int i = 0; i < s.Length; i++)
            {
                if (!(Char.IsDigit(s[i]))) return i;
            }
            return -1;
        }
    }

    public interface IProgram
    {
        void Run();
    }

    public interface ILogger
    {
        void Log(string log);
    }

    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class Program : IProgram
    {
        private static CompositionContainer _container;
        private static IContainer _autofacContainer;

        private readonly ICalculator _calculator;
        private readonly ILogger _logger;

        public Program(ICalculator calculator,  ILogger logger)
        {
            _calculator = calculator;
            _logger = logger;
        }

        public void Run()
        {
            _logger.Log("Program.Run() - Entry.");    
            String s;
            Console.WriteLine("Enter Command:");
            while (true)
            {
                _logger.Log("Program.Run() - prompt");
                s = Console.ReadLine();
                Console.WriteLine(_calculator.Calculate(s));
            }
        }

        static void Main(string[] args)
        {
            // MEF
            var catalog = new AggregateCatalog();
            // current assembly
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            // plugins folder
            //catalog.Catalogs.Add(new DirectoryCatalog("Extensions"));
            _container = new CompositionContainer(catalog);

            // Autofac
            var builder = new ContainerBuilder();
            builder.RegisterComposablePartCatalog(catalog);
            builder.RegisterType<Logger>().As<ILogger>();
            builder.RegisterType<Program>().As<IProgram>();
            _autofacContainer = builder.Build();

            var l = _autofacContainer.ComponentRegistry.Registrations
                .SelectMany(x => x.Services).ToList();

            var p = _autofacContainer.Resolve<IProgram>();

            p.Run();
        }
    }
}