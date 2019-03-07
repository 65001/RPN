using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                Console.Title = "Math Solver 1.0.5";
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
                        double Answer = Calculate(Equation, start, end, freq / 2, true);
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
                RPN.Data.DebugMode = true;
                Console.WriteLine($"Debug Mode : { RPN.Data.DebugMode}");
            }
            RPN.Compute();

            PostFix postFix = new PostFix(RPN);
            Console.ForegroundColor = ConsoleColor.White;
            if (DebugMode)
            {
                postFix.Logger += Write;
            }

                double Rsum = 0;
                double Lsum = 0;
                double MidSum = 0;

                double f_a = 0;

                int count = 0;
                double DeltaX = end - start;
                double n =  DeltaX / freq;
                int max = (int)Math.Ceiling(n);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                Config config = new CLI.Config {Title = "Table"};
                Tables<string> tables = new Tables<string>(config);
                tables.Add(new Schema {Column = "x", Width = 20 });
                tables.Add(new Schema {Column = "f(x)", Width = 25});

                if (write)
                {
                    Console.WriteLine(tables.GenerateHeaders());
                }

                for (int x = 0; x <= max; x++)
                {
                    double RealX = start + count * DeltaX / n;
                    postFix.SetVariable("ans", PrevAnswer);
                    postFix.SetVariable("x", RealX);
                    double answer = postFix.Compute();

                    if (x == 0)
                    {
                        f_a = answer;
                    }

                    if (x % 2 == 0)
                    {
                        if (x < max)
                        {
                            Rsum += answer;
                        }

                        if (count > 0)
                        {
                            Lsum += answer;
                        }
                    }
                    else
                    {
                        MidSum += answer;
                    }

                    PrevAnswer = answer;
                    tables.Add(new string[] { (RealX).ToString(), answer.ToString(), });
                    if (write)
                    {
                        if (RPN.Data.ContainsEquation && answer == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }

                        Console.Write(tables.GenerateNextRow());
                        Console.WriteLine();

                        if (RPN.Data.ContainsEquation && answer == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
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

                double LApprox = (2 * Rsum * DeltaX / n);
                double RApprox = (2 * Lsum * DeltaX / n);
                double MApprox = (2 * MidSum * DeltaX / n);
                double TApprox = (LApprox + RApprox) / 2;

                double Lerror = 2 * DeltaX/n * Math.Abs(PrevAnswer - f_a);

                Tables<string> Error = new Tables<string>(new Config()
                {
                    Format = Format.Default,
                    Title = "Intervals Needed"
                });

                double error_const = (DeltaX) * Math.Abs(PrevAnswer - f_a);
                Error.Add(new Schema {Column = "Decimal", Width = 22});
                Error.Add(new Schema {Column = "Intervals", Width = 22});

                Console.WriteLine(Error.GenerateHeaders());

                for (int i = -18; i < 18; i++)
                {
                    int intervals = (int)Math.Ceiling( error_const * (2 * Math.Pow(10,i)) );

                    if (intervals < 0 || intervals == 1)
                    {
                        continue;
                    }

                    Error.Add(new string[] {i.ToString(), intervals.ToString() });

                    if ((n / 2) >= intervals)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.WriteLine(Error.GenerateNextRow());
                }
                Console.Write(Error.GenerateFooter());

                Console.ForegroundColor = ConsoleColor.White;
                if (Error.SuggestedRedraw)
                {
                    Console.WriteLine(Error.Redraw());
                }

                Console.WriteLine();

                Tables<string> Approx = new Tables<string>(new Config()
                {
                    Format = Format.Default,
                    Title = "Approximations"
                });

                Approx.Add(new Schema {Column = "Type", Width = 12});
                Approx.Add(new Schema {Column = "Sum", Width = 18});
                Approx.Add(new Schema {Column = "Approximation", Width = 18});
                Approx.Add(new Schema {Column = "Error", Width = 18});

                Console.WriteLine(Approx.GenerateHeaders());

                List<double> approximations = new List<double>(5);
                approximations.Add(LApprox);
                approximations.Add(RApprox);
                approximations.Add(MApprox);
                approximations.Add(TApprox);

                Approx.Add(new string[] {"Left", Rsum.ToString(), LApprox.ToString() , Lerror.ToString()});
                Approx.Add(new string[] {"Right", Lsum.ToString(), RApprox.ToString(), "" });
                Approx.Add(new string[] {"Mid", MidSum.ToString(), MApprox.ToString(), "" });
                Approx.Add(new string[] { "Trapezoidal", "NA", TApprox.ToString(), "" });

                if ( (n / 2) % 2 == 0)
                {
                    approximations.Add((TApprox + 2 * MApprox) / 3);
                    Approx.Add(new string[] {"Simpson", "NA", ( (TApprox + 2 * MApprox) / 3 ).ToString(), ""});
                }
                else
                {
                    Approx.Add(new string[] { "Simpson", "NA", "NA", "" });
                }

                Console.Write(Approx.GenerateBody());
                Console.Write(Approx.GenerateFooter());

                if (Approx.SuggestedRedraw)
                {
                    Console.WriteLine(Approx.Redraw());
                }

                Console.WriteLine();

                approximations.Sort();
                for (int i = 0; i < 2; i++)
                {
                    double min = approximations.First();
                    var data2 = approximations.Last();

                    approximations.Remove(min);
                    approximations.Remove(data2);

                    Console.WriteLine($"{min} ≤ x ≤ {data2}");
                }

                if (approximations.Count > 0)
                {
                    Console.WriteLine($"x ≈ {approximations.First()}");
                }
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
