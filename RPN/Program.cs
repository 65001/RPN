using System;
using System.Collections.Generic;
using AbMath.Utilities;

namespace Solver
{
    class Program
    {
        private RPN RPN;
        private static bool DebugMode;

        static void Main(string[] args)
        {
            Console.Title = "Math Solver 1.0.3";
            Console.WindowWidth = Console.BufferWidth;
            Console.WriteLine("(C) 2018. Abhishek Sathiabalan");

            Console.WriteLine("Recent Changes:");
            Console.WriteLine("Unary negative is now implemented.");
            Console.WriteLine("Composite Function bug should now be fixed.");

            Console.WriteLine("");
            Console.WriteLine("Known Bugs:");
            Console.WriteLine("Space between terms is necessary.");
            Console.WriteLine("Implicit multiplication.");
            Console.WriteLine();

            while (1 == 1)
            {
                //try
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    string Equation = string.Empty;
                    while (string.IsNullOrWhiteSpace(Equation))
                    {
                        Console.Write("Equation>");
                        Equation = Console.ReadLine();

                        if (Equation.Length == 0) { Console.Clear(); }
                    }

                    if (Equation.StartsWith('~'))
                    {
                         CLI(Equation);
                    }
                    else
                    {
                        double Answer = Calculate(Equation);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Answer:");
                        Console.WriteLine(Answer);

                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.Clear();
                }
                //catch (Exception ex)
                {
                   // Console.WriteLine("An Error happened!");
                   // Console.WriteLine(ex);
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
                foreach (KeyValuePair<string, RPN.Function> KV in RPN.data.Functions)
                {
                    Console.WriteLine(KV.Key);
                }
            }
            else if (Equation.StartsWith("~operators", SC))
            {
                var RPN = new RPN("");
                foreach (KeyValuePair<string, RPN.Operator> KV in RPN.data.Operators)
                {
                    Console.WriteLine(KV.Key);
                }
            }
            else if (Equation.StartsWith("~debug", SC))
            {
                DebugMode = !DebugMode;
                Console.WriteLine($"Debug Mode : {DebugMode}");
            }
        }

        ///<summary>
        /// All the RPN math interactions.
        /// </summary>
        static double Calculate(string Equation)
        {
            var RPN = new RPN(Equation);
            if (DebugMode)
            {
                RPN.Logger += Write;
            }
            RPN.Compute();
            if (DebugMode == false)
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
                for (int i = 0; i < RPN.data.Variables.Count; i++)
                {
                    Console.Write(RPN.data.Variables[i] + "=");
                    var VariableExpression = Console.ReadLine();
                    postFix.SetVariable(RPN.data.Variables[i], Calculate(VariableExpression).ToString());
                }
            }

            double answer = postFix.Compute();
            if (RPN.data.Format.ContainsKey(answer))
            {
                Console.WriteLine($"This answer can also be formatted as {RPN.data.Format[answer] }");
            }

            return answer;
        }

        static void Write(object sender, string Event)
        {
            Console.WriteLine(Event);
        }


    }
}
