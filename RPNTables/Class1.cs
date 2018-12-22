using System;
using System.Collections.Generic;
using System.Diagnostics;
using AbMath.Calculator;
using CLI;

namespace Solver
{
    class Program
    {
        private RPN RPN;
        private static bool DebugMode;
        private static bool MarkDownMode;
        private static double PrevAnswer;
        private static bool SupressOutput = false;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                SupressOutput = true;
            }

            if (!SupressOutput)
            {
                Console.Title = "Math Solver 1.0.4";
                Console.WindowWidth = Console.BufferWidth;
                Console.WriteLine("(C) 2018. Abhishek Sathiabalan");

                Console.WriteLine("Recent Changes:");
                Console.WriteLine("Unary negative is now implemented.");
                Console.WriteLine("Composite Function bug should now be fixed.");
                Console.WriteLine("Variadic Functions & Implicit Left bug fixed");

                Console.WriteLine("");
                Console.WriteLine("Known Bugs:");
                Console.WriteLine("Space between terms is necessary.");
                Console.WriteLine("Implicit multiplication.");
                Console.WriteLine();
            }

            while (true)
            {
                //try
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string Equation = string.Empty;
                    string Sstart = string.Empty;
                    string Send = string.Empty;
                    string Sfreq = string.Empty;

                    int start = 0;
                    int end = 0;
                    double freq = double.NaN;
                    
               

                    if (SupressOutput)
                    {
                        Equation = args[0];
                    }

                    while (string.IsNullOrWhiteSpace(Equation))
                    {
                        Console.Write("Equation>");
                        Equation = Console.ReadLine();

                        if (Equation.Length == 0) { Console.Clear(); }
                    }

                    Console.Write("Start Point>");
                    Sstart = Console.ReadLine();
                    start = (int)Calculate(Sstart, 0, 0, 0);

                    Console.Write("End Point>");
                    Send = Console.ReadLine();
                    end = (int)Calculate(Send, 0, 0, 0);

                    Console.Write("Freq>");
                    Sfreq = Console.ReadLine();
                    freq = Calculate(Sfreq, 0, 0, 0);


                    if (MarkDownMode)
                    {
                        Console.Clear();
                        Console.WriteLine($"Equation>``{Equation}``");
                    }

                    if (Equation.StartsWith("~"))
                    {
                        CLI(Equation);
                    }
                    else
                    {
                        double Answer = Calculate(Equation, start, end, freq, true);
                        Console.ForegroundColor = ConsoleColor.White;

                        PrevAnswer = Answer;
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    if (!SupressOutput)
                    {
                        Console.Write("Press any key to continue...");
                        Console.ReadKey(true);
                        Console.Clear();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        ///<summary>
        /// All the Command Line Interpreter Code Here
        /// </summary>
        static void CLI(string Equation)
        {

            StringComparison SC = StringComparison.InvariantCultureIgnoreCase;
            if (Equation.StartsWith("~functions", SC))
            {
                var RPN = new RPN("");
                foreach (KeyValuePair<string, RPN.Function> KV in RPN.Data.Functions)
                {
                    Console.WriteLine(KV.Key);
                }
            }
            else if (Equation.StartsWith("~operators", SC))
            {
                var RPN = new RPN("");
                foreach (KeyValuePair<string, RPN.Operator> KV in RPN.Data.Operators)
                {
                    Console.WriteLine(KV.Key);
                }
            }
            else if (Equation.StartsWith("~debug", SC))
            {
                DebugMode = !DebugMode;
                Console.WriteLine($"Debug Mode : {DebugMode}");
            }
            else if (Equation.StartsWith("~md"))
            {
                MarkDownMode = !MarkDownMode;
            }
        }

        ///<summary>
        /// All the RPN math interactions.
        /// </summary>
        static double Calculate(string Equation, double start, double end, double freq, bool write = false)
        {
            var RPN = new RPN(Equation);
            RPN.Data.MarkdownTables = MarkDownMode;

            if (DebugMode)
            {
                RPN.Logger += Write;
            }
            RPN.Compute();

            PostFix postFix = new PostFix(RPN);
            if (DebugMode)
            {
                postFix.Logger += Write;
            }

                double Rsum = 0;
                double Lsum = 0;
                int count = 0;
                double DeltaX = end - start;
                double n =  DeltaX / freq;
                int max = (int)Math.Ceiling(n);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                Config config = new CLI.Config {Title = "Table"};
                Tables<string> tables = new Tables<string>(config);
                tables.Add(new Schema {Column = "x", Width = 10});
                tables.Add(new Schema {Column = "f(x)", Width = 10});

                if (write)
                {
                    Console.WriteLine(tables.GenerateHeaders());
                }


                for (int x = 0; x <= max; x++)
                {
                    double RealX = start + count * DeltaX / n;
                    for (int i = 0; i < RPN.Data.Variables.Count; i++)
                    {
                        
                        if (RPN.Data.Variables[i] == "ans")
                        {
                            postFix.SetVariable(RPN.Data.Variables[i], PrevAnswer.ToString());
                        }

                        if (RPN.Data.Variables[i] == "x")
                        {
                            postFix.SetVariable(RPN.Data.Variables[i], (RealX).ToString());
                        }
                    }
                    double answer = postFix.Compute();
                    if (x < max )
                    {
                        Rsum += answer;
                    }

                    if (count > 0)
                    {
                        Lsum += answer;
                    }

                    PrevAnswer = answer;
                    tables.Add(new string[] { (RealX).ToString(), answer.ToString(), });
                    if (write)
                    {
                        Console.Write(tables.GenerateNextRow());
                        Console.WriteLine();
                    }

                    postFix.Reset();
                    count++;
                }

                if (write)
                {
                    Console.WriteLine(tables.GenerateFooter());
                }

                if (tables.SuggestedRedraw && write)
                {
                    Console.WriteLine(tables.Redraw());
                }

            if (write)
            {
                Console.WriteLine($"Elapsed Time: {sw.ElapsedMilliseconds} (ms)");
                Console.WriteLine($"Iterations: {count} ");
                Console.WriteLine();

                Console.WriteLine($"Left Sum : {Rsum}");
                Console.WriteLine($"Right Sum: {Lsum}");
                Console.WriteLine($"Left Integral ? : {Rsum * DeltaX / n}");
                Console.WriteLine($"Right Integral ? : {Lsum * DeltaX / n}");
            }

            if (!RPN.ContainsVariables)
            {
                return postFix.Compute();
            }

            return 0;

        }

        static void Write(object sender, string Event)
        {
            Console.WriteLine(Event);
        }
    }
}
