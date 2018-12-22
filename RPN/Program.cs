using System;
using System.Collections.Generic;
using AbMath.Calculator;

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

                    if (MarkDownMode)
                    {
                        Console.Clear();
                        Console.WriteLine($"Equation>``{Equation}``");
                    }

                    if (Equation.StartsWith('~'))
                    {
                         CLI(Equation);
                    }
                    else
                    {
                        double Answer = Calculate(Equation);
                        Console.ForegroundColor = ConsoleColor.White;
                        if (MarkDownMode)
                        {
                            Console.WriteLine($"Answer: ``{Answer}``");
                        }
                        else if (!SupressOutput)
                        {
                            Console.Write("Answer:");
                            Console.WriteLine(Answer);
                        }
                        else
                        {
                            Console.WriteLine(Answer);
                        }

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
        static double Calculate(string Equation)
        {
            var RPN = new RPN(Equation);
            RPN.Data.MarkdownTables = MarkDownMode;

            if (DebugMode)
            {
                RPN.Logger += Write;
            }
            RPN.Compute();

            if (SupressOutput == false && DebugMode == false)
            {
                Console.WriteLine("Reverse Polish Notation:");
                Console.WriteLine(RPN.Polish.Print());
            }

            PostFix postFix = new PostFix(RPN);
            if (DebugMode)
            {
                postFix.Logger += Write;
            }


            if (RPN.ContainsVariables)
            {
                Console.WriteLine("Set the variables");
                for (int i = 0; i < RPN.Data.Variables.Count; i++)
                {
                    if (RPN.Data.Variables[i] == "ans")
                    {
                        postFix.SetVariable(RPN.Data.Variables[i], PrevAnswer.ToString());
                        Console.WriteLine($"{RPN.Data.Variables[i]}={PrevAnswer}");
                    }
                    else
                    {
                        Console.Write(RPN.Data.Variables[i] + "=");
                        var VariableExpression = Console.ReadLine();
                        postFix.SetVariable(RPN.Data.Variables[i], Calculate(VariableExpression).ToString());
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

        static void Write(object sender, string Event)
        {
            Console.WriteLine(Event);
        }
    }
}
