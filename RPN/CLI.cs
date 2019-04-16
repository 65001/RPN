using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AbMath.Calculator;
using CLI;

namespace Solver
{
    class CLI
    {
        private static RPN RPN;
        private static bool DebugMode;
        private static bool MarkDownMode;
        private static bool SupressOutput = false;
        private static bool IntegrateMode = false;

        private static double PrevAnswer;
        

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                SupressOutput = true;
            }

            if (!SupressOutput)
            {
                Console.Title = "Math Solver 1.0.6";
                Console.WindowWidth = Console.BufferWidth;
                Console.WriteLine("(C) 2018. Abhishek Sathiabalan");
            }

            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string Equation = string.Empty;

                    if (SupressOutput)
                    {
                        Equation = args[0];
                    }

                    while (string.IsNullOrWhiteSpace(Equation))
                    {
                        Console.Write("Equation>");
                        Equation = Console.ReadLine();

                        if (Equation.Length == 0)
                        {
                            Console.Clear();
                        }
                    }

                    if (MarkDownMode)
                    {
                        Console.Clear();
                        Console.WriteLine($"Equation>``{Equation}``");
                    }

                    if (Equation.StartsWith('~'))
                    {
                        MetaCommands(Equation);
                    }
                    else if (IntegrateMode)
                    {
                        string SStart = string.Empty;
                        string SEnd = string.Empty;
                        string SFreq = string.Empty;

                        double start;
                        double end;
                        double freq;

                        //TODO: Validate User input

                        Console.Write("Start Point>");
                        SStart = Console.ReadLine();
                        start = Calculate(SStart);

                        //TODO: Validate User input
                        Console.Write("End Point>");
                        SEnd = Console.ReadLine();
                        end = Calculate(SEnd);

                        //TODO: Validate User input
                        Console.Write("Freq>");
                        SFreq = Console.ReadLine();
                        freq = Calculate(SFreq);

                        Stopwatch SW = new Stopwatch();
                        SW.Start();
                        double integral = Integral(Equation, start, end, freq / 2);
                        SW.Stop();

                        WriteAnswer(integral, SW);

                    }
                    else
                    {
                        Stopwatch SW = new Stopwatch();
                        SW.Start();
                        double Answer = Calculate(Equation);
                        SW.Stop();

                        if (SupressOutput == false && DebugMode == false)
                        {
                            Console.WriteLine("Reverse Polish Notation:");
                            Console.WriteLine(RPN.Polish.Print());
                        }

                        WriteAnswer(Answer, SW);
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        ///<summary>
        /// All the Command Line Interpreter Code Here
        /// </summary>
        static void MetaCommands(string Equation)
        {
            
            StringComparison SC = StringComparison.InvariantCultureIgnoreCase;
            if (Equation.StartsWith("~functions", SC))
            {
                Config config = new Config() { Format = Format.Default, Title = "Functions" };

                if (MarkDownMode)
                {
                    config.Format = Format.MarkDown;
                }

                Tables<string> functions = new Tables<string>(config);
                functions.Add(new Schema {Column = "Functions", Width = 10});
                functions.Add(new Schema {Column = "Min Arguments", Width = 14});
                functions.Add(new Schema {Column = "Max Arguments", Width = 14 });

                var RPN = new RPN("");
                foreach (KeyValuePair<string, RPN.Function> KV in RPN.Data.Functions)
                {
                    functions.Add(new string[] {KV.Key, KV.Value.MinArguments.ToString(), KV.Value.MaxArguments.ToString() });
                }

                Console.WriteLine(functions.ToString());
            }
            else if (Equation.StartsWith("~operators", SC))
            {
                Config config = new Config() {Format = Format.Default, Title = "Operators"};

                if (MarkDownMode)
                {
                    config.Format = Format.MarkDown;
                }

                Tables<string> operators = new Tables<string>(config);
                operators.Add(new Schema {Column = "Operator", Width = 9});
                operators.Add(new Schema {Column = "Assoc", Width = 5});
                operators.Add(new Schema {Column = "Arguments", Width = 10});
                operators.Add(new Schema {Column = "Weights", Width = 8 });

                var RPN = new RPN("");
                foreach (KeyValuePair<string, RPN.Operator> KV in RPN.Data.Operators)
                {
                    operators.Add(new string[] {KV.Key, KV.Value.Assoc.ToString(), KV.Value.Arguments.ToString(), KV.Value.Weight.ToString() });
                }

                Console.WriteLine(operators.ToString());
            }
            else if (Equation.StartsWith("~alias", SC))
            {

            }
            else if (Equation.StartsWith("~format", SC))
            {

            }
            else if (Equation.StartsWith("~version", SC))
            {
                Config config = new Config() { Format = Format.Default, Title = "Version Information" };

                if (MarkDownMode)
                {
                    config.Format = Format.MarkDown;
                }

                Tables<string> version = new Tables<string>(config);
                version.Add(new Schema { Column = "Program", Width = 30 });
                version.Add(new Schema { Column = "Version", Width = 10 });
                version.Add(new string[] { "AbMath", Assembly.GetAssembly(typeof(RPN)).GetName().Version.ToString() });
                version.Add(new string[] { "Math Solver", Assembly.GetExecutingAssembly().GetName().Version.ToString() });

                Console.WriteLine(version.ToString());
            }
            else if (Equation.StartsWith("~debug", SC))
            {
                DebugMode = !DebugMode;

                Console.WriteLine($"Debug Mode : {DebugMode}");
            }
            else if (Equation.StartsWith("~integrate", SC))
            {
                IntegrateMode = !IntegrateMode;

                Console.WriteLine($"Integration Mode : {IntegrateMode}");
            }
            else if (Equation.StartsWith("~md"))
            {
                MarkDownMode = !MarkDownMode;
            }
        }

        ///<summary>
        /// All the RPN math interactions.
        /// </summary>
        static double Calculate(string Equation)
        {
            RPN = GenerateRPN(Equation);

            PostFix postFix = GeneratePostFix(RPN);

            if (RPN.ContainsVariables)
            {
                Console.WriteLine("Set the variables");
                var variables = RPN.Data.Variables.Distinct().ToList();

                for (int i = 0; i < variables.Count; i++)
                {
                    if (variables[i] == "ans")
                    {
                        postFix.SetVariable("ans", PrevAnswer.ToString());
                        Console.WriteLine($"ans={PrevAnswer}");
                    }
                    else
                    {
                        string VariableExpression = string.Empty;

                        while (string.IsNullOrWhiteSpace(VariableExpression))
                        {
                            Console.Write(variables[i] + "=");
                            VariableExpression = Console.ReadLine();
                        }

                        postFix.SetVariable(variables[i], Calculate(VariableExpression));
                    }
                }
            }

            double answer = postFix.Compute();
            if (RPN.Data.Format.ContainsKey(answer))
            {
                Console.WriteLine($"This answer can also be formatted as {RPN.Data.Format[answer] }");
            }

            return answer;
        }

        /// <summary>
        /// Calculates the integral of a given Equation.
        /// If freq is an even number it returns the results of the Simpson Approximation
        /// otherwise it returns the MidPoint approximation. 
        /// </summary>
        /// <param name="Equation"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        static double Integral(string Equation, double start, double end, double freq)
        {
            RPN = GenerateRPN(Equation);

            double Rsum = 0;
            double Lsum = 0;
            double MidSum = 0;

            double f_a = 0;
            int count = 0;

            double DeltaX = end - start;
            double n = DeltaX / freq;
            int max = (int)Math.Ceiling(n);

            Console.ForegroundColor = ConsoleColor.White;
            PostFix postFix = GeneratePostFix(RPN);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Config config = new Config { Title = "Table" };
            Tables<string> tables = new Tables<string>(config);
            tables.Add(new Schema { Column = "x", Width = 20 });
            tables.Add(new Schema { Column = "f(x)", Width = 25 });

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

                if (RPN.Data.ContainsEquation && answer == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                if (RPN.Data.ContainsEquation && answer == 1)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                postFix.Reset();
                count++;
            }

            Console.WriteLine(tables.ToString());

            if (tables.SuggestedRedraw)
            {
                Console.WriteLine(tables.Redraw());
            }

            Console.WriteLine($"Elapsed Time: {sw.ElapsedMilliseconds} (ms)");
            Console.WriteLine($"Iterations: {count} ");
            Console.WriteLine();

            double LApprox = (2 * Rsum * DeltaX / n);
            double RApprox = (2 * Lsum * DeltaX / n);
            double MApprox = (2 * MidSum * DeltaX / n);
            double TApprox = (LApprox + RApprox) / 2;

            double Lerror = 2 * DeltaX / n * Math.Abs(PrevAnswer - f_a);

            Tables<string> Error = new Tables<string>(new Config()
            {
                Format = Format.Default,
                Title = "Intervals Needed"
            });

            double error_const = (DeltaX) * Math.Abs(PrevAnswer - f_a);
            Error.Add(new Schema { Column = "Decimal", Width = 22 });
            Error.Add(new Schema { Column = "Intervals", Width = 22 });

            Console.WriteLine(Error.GenerateHeaders());

            for (int i = -18; i < 18; i++)
            {
                int intervals = (int)Math.Ceiling(error_const * (2 * Math.Pow(10, i)));

                if (intervals < 0 || intervals == 1)
                {
                    continue;
                }

                //Make all intervals given usable by the simpson rule!
                if (intervals % 2 == 1)
                {
                    intervals++;
                }

                Error.Add(new string[] { i.ToString(), intervals.ToString() });

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

            Approx.Add(new Schema { Column = "Type", Width = 12 });
            Approx.Add(new Schema { Column = "Sum", Width = 18 });
            Approx.Add(new Schema { Column = "Approximation", Width = 18 });
            Approx.Add(new Schema { Column = "Error", Width = 18 });

            List<double> approximations = new List<double>(5);
            approximations.Add(LApprox);
            approximations.Add(RApprox);
            approximations.Add(MApprox);
            approximations.Add(TApprox);

            Approx.Add(new string[] { "Left", Rsum.ToString(), LApprox.ToString(), Lerror.ToString() });
            Approx.Add(new string[] { "Right", Lsum.ToString(), RApprox.ToString(), "" });
            Approx.Add(new string[] { "Mid", MidSum.ToString(), MApprox.ToString(), "" });
            Approx.Add(new string[] { "Trapezoidal", "NA", TApprox.ToString(), "" });

            //Recalculate the sub intervals based on the real frequency 
             freq = freq * 2;
             n = DeltaX / freq;
            double Simpson = double.NaN;

            //The sub-intervals must be even to use the Simpson's Rule.
            if (n % 2 == 0)
            {
                Simpson = (TApprox + 2 * MApprox) / 3;
                approximations.Add(Simpson);
                Approx.Add(new string[] { "Simpson", "NA", Simpson.ToString(), "" });

                Console.WriteLine("n is even");
            }
            else
            {
                Approx.Add(new string[] { "Simpson", "NA", "NA", "" });
            }

            Console.WriteLine(Approx.ToString());

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


            Console.WriteLine();

            Console.WriteLine($"Equation of {Equation} from {start} to {end} at {freq} with {n} sub intervals.");

            Console.WriteLine();

            //Simpsons Rule
            if (!double.IsNaN(Simpson))
            {
                return Simpson;
            }

            return MApprox;
        }

        private static RPN GenerateRPN(string Equation)
        {
            var RPN = new AbMath.Calculator.RPN(Equation);
            RPN.Data.MarkdownTables = MarkDownMode;

            if (DebugMode)
            {
                RPN.Data.DebugMode = DebugMode;
                RPN.Logger += Write;
            }
            RPN.Compute();

            return RPN;
        }

        private static PostFix GeneratePostFix(RPN rpn)
        {
            PostFix postFix = new PostFix(rpn);
            if (DebugMode)
            {
                postFix.Logger += Write;
            }

            return postFix;
        }

        private static void WriteAnswer(double Answer, Stopwatch SW)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (MarkDownMode)
            {
                Console.WriteLine($"Answer: ``{Answer}``");
            }
            else if (!SupressOutput)
            {
                Console.WriteLine($"Answer: {Answer}");
                Console.WriteLine($"{SW.ElapsedMilliseconds} (ms) {SW.ElapsedTicks} (Ticks)");
            }
            else
            {
                Console.WriteLine(Answer);
            }

            PrevAnswer = Answer;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void Write(object sender, string Event)
        {
            Console.WriteLine(Event);
        }
    }
}
