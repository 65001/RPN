﻿using System;
using System.Collections.Generic;
using AbMath.Utilities;

namespace Solver
{
    class Program
    {
        private RPN RPN;

        static void Main(string[] args)
        {
            Console.Title = "Math Solver 1.0.1";
            Console.WindowWidth = Console.BufferWidth;
            Console.WriteLine("(C) 2018. Abhishek Sathiabalan");

            Console.WriteLine("Recent Changes:");
            Console.WriteLine("Uniary negative is now implemented.");
            Console.WriteLine("Composite Function bug should now be fixed.");

            Console.WriteLine("");
            Console.WriteLine("Known Bugs:");
            Console.WriteLine("Space between terms is necessary.");
            Console.WriteLine("Implict multiplication.");
            Console.WriteLine();

            while (1 == 1)
            {
                try
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
                        Calculate(Equation);
                    }


                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An Error happened!");
                    Console.WriteLine(ex);
                }
            }

            void CLI(string Equation)
            {
                StringComparison SC = StringComparison.InvariantCultureIgnoreCase;
                if (Equation.StartsWith("~functions", SC))
                {
                    var RPN = new RPN("");
                    foreach (KeyValuePair<string,RPN.Functions> KV in RPN.ReadOnlyFunctions)
                    {
                        Console.WriteLine(KV.Key);
                    }
                }
                else if (Equation.StartsWith("~operators", SC))
                {
                    var RPN = new RPN("");
                    foreach (KeyValuePair<string, RPN.Operators> KV in RPN.ReadOnlyOperators)
                    {
                        Console.WriteLine(KV.Key);
                    }
                }


            }

            void Calculate(string Equation)
            {
                var RPN = new RPN(Equation);
                RPN.Logger += Write;
                RPN.Compute();

                Console.WriteLine("Reverse Polish Notation:");
                Console.WriteLine(RPN.Polish.Print());
                PostFix postFix = new PostFix(RPN);

                if (RPN.ContainsVariables)
                {
                    Console.WriteLine("Set the variables");
                    for (int i = 0; i < RPN.Variables.Count; i++)
                    {
                        Console.Write(RPN.Variables[i] + "=");
                        postFix.SetVariable(RPN.Variables[i], Console.ReadLine());
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Answer:");
                Console.WriteLine(postFix.Compute());
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            void Write(object sender, string Event)
            {
                Console.WriteLine(Event);
            }
        }
    }
}