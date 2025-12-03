using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIP3_term3
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
        //csharp
        static int CorrectNumInput(int startNum, int endNum, string prompt = "")
        {
            int num;
            while (true)
            {
                if (!string.IsNullOrEmpty(prompt))
                    Console.Write(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out num) &&
                    num >= startNum &&
                    num <= endNum)
                {
                    break;
                }
                Console.Write($"Invalid input! Please enter a number between {startNum} and {endNum}: ");
            }
            return num;
        }
    }
}
