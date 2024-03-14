using System;

namespace CandymanNumbers
{
    public class ZukermanNumbersFinder
    {
        public delegate void ZukermanNumberHandler(int number);

        public event ZukermanNumberHandler OnZukermanNumber;
        public event EventHandler OnFullScreen;

        public void FindZukermanNumbers(int start, int end)
        {
            int count = 0;

            for (int i = start; i <= end; i++)
            {
                if (IsZukermanNumber(i))
                {
                    OnZukermanNumber?.Invoke(i);
                    count++;

                    if (count == 10)
                    {
                        OnFullScreen?.Invoke(this, EventArgs.Empty);
                        count = 0;
                    }
                }
            }
        }

        private int getProduct(int number)
        {
            int product = 1;
            while (number != 0)
            {
                product = product * (number % 10);
                number = number / 10;
            }
            return product;
        }

        private bool IsZukermanNumber(int number)
        {
            return number % getProduct(number) == 0;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            ZukermanNumbersFinder finder = new ZukermanNumbersFinder();
            finder.OnZukermanNumber += OnZukermanNumberHandler;
            finder.OnFullScreen += OnFullScreenHandler;
            finder.FindZukermanNumbers(0, 2000000000);
        }

        static void OnZukermanNumberHandler(int number)
        {
            Console.WriteLine($"Found a zukerman number: {number}");
        }

        static void OnFullScreenHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Full screen activated. Press any key to continue or 'q' to quit...");

            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.KeyChar == 'q')
            {
                Environment.Exit(0);
            }
        }
    }
}
