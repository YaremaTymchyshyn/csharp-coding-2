using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace task1
{
    class Program
    {
        static void Task_1()
        {
            int[,] A = { 
                { 876, 423, 123, 74, 12 }, 
                { 87, 93, 520, 673, 874 }, 
                { 8432, 432, 106721, 943, 4653 }, 
                { 784932, 8423, 643, 82, 12},
                { 35, 44323, 652, 63373, 4325} };
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                bool increasing = true;
                bool decreasing = true;

                for (int j = 1; j < n; j++)
                {
                    if (A[i, j] > A[i, j - 1])
                    {
                        decreasing = false;
                    }
                    else if (A[i, j] < A[i, j - 1])
                    {
                        increasing = false;
                    }
                }

                if (increasing || decreasing)
                {
                    Console.WriteLine("Row " + (i + 1) + " is monotonic.");
                }
            }
        }
        
        //static int Main()
        //{
        //    Task_1();
        //    return 0;
        //}
    }
}

namespace task2
{
    public class Monitor : IComparable<Monitor>
    {
        public string Model { get; set; }
        public double Diagonal { get; set; }

        public Monitor(string model, double diagonal)
        {
            Model = model;
            Diagonal = diagonal;
        }

        public int CompareTo(Monitor other)
        {
            return Diagonal.CompareTo(other.Diagonal);
        }
    }

    public class PlasmaMonitor : Monitor
    {
        public string Resolution { get; set; }
        public string ViewingAngle { get; set; }

        public PlasmaMonitor(string model, double diagonal, string resolution, string viewingAngle)
            : base(model, diagonal)
        {
            Resolution = resolution;
            ViewingAngle = viewingAngle;
        }
    }

    public class BeamMonitor : Monitor
    {
        public double Frequency { get; set; }
        public double RadiationAmount { get; set; }

        public BeamMonitor(string model, double diagonal, double frequency, double radiationAmount)
            : base(model, diagonal)
        {
            Frequency = frequency;
            RadiationAmount = radiationAmount;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Monitor[] monitors = LoadMonitorsFromFile("monitors.txt");

            Console.WriteLine("All monitors arranged by diagonals:");
            foreach (Monitor monitor in monitors.OrderBy(m => m.Diagonal))
            {
                Console.WriteLine($"{monitor.Model} {monitor.Diagonal}");
            }
        }

        static Monitor[] LoadMonitorsFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            Monitor[] monitors = new Monitor[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(',');
                string model = parts[0];
                double diagonal = double.Parse(parts[1]);
                string type = parts[2];

                if (type == "Plasma")
                {
                    string resolution = parts[3];
                    string viewingAngle = parts[4];
                    monitors[i] = new PlasmaMonitor(model, diagonal, resolution, viewingAngle);
                }
                else if (type == "Beam")
                {
                    int frequency = int.Parse(parts[3]);
                    double radiationAmount = double.Parse(parts[4]);
                    monitors[i] = new BeamMonitor(model, diagonal, frequency, radiationAmount);
                }
                else
                    throw new ArgumentException($"Invalid monitor type: {type}", nameof(filePath));
            }
            return monitors;
        }
    }
}