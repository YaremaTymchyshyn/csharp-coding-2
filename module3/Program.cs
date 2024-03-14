using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

public class Settlement
{
    public string Name { get; set; }
    public int Population { get; set; }
    public double Area { get; set; }

    public double PopulationDensity => Population / Area;
    public override string ToString()
    {
        return $"{Name} - Population: {Population}, Area: {Area}";
    }
}

public class City : Settlement
{
    public string Mayor { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()}, Mayor: {Mayor}";
    }
}

public class Village : Settlement
{
    public string Chairman { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()}, Chairman: {Chairman}";
    }
}

public enum PoliticalSystem
{
    Monarchy,
    Republic,
    Democracy,
    Dictatorship,
    Capitalism,
    Oligarchy
}

public class Country
{
    public string Name { get; set; }
    public PoliticalSystem System { get; set; }
    public List<Settlement> Settlements { get; set; }

    public Country()
    {
        Settlements = new List<Settlement>();
    }

    public Settlement this[int index]
    {
        get { return Settlements[index]; }
        set { Settlements[index] = value; }
    }

    public override string ToString()
    {
        string system = System.ToString();
        string settlements = string.Join("\n", Settlements);

        return $"Country: {Name}\nPolitical system: {system}\nSettlements:\n{settlements}";
    }

    public double CalculateHighestPopulationDensity()
    {
        double highestDensity = 0.0;

        foreach (Settlement settlement in Settlements)
        {
            double density = settlement.PopulationDensity;
            if (density > highestDensity)
                highestDensity = density;
        }

        return highestDensity;
    }

    public List<string> GetSettlementsWithHighestArea()
    {
        double highestArea = 0.0;
        List<string> settlementsWithHighestArea = new List<string>();

        foreach (Settlement settlement in Settlements)
        {
            if (settlement.Area > highestArea)
            {
                highestArea = settlement.Area;
                settlementsWithHighestArea.Clear();
                settlementsWithHighestArea.Add($"{settlement.Name} - Area: {settlement.Area}");
            }
            else if (settlement.Area == highestArea)
            {
                settlementsWithHighestArea.Add($"{settlement.Name} - Area: {settlement.Area}");
            }
        }
            
        return settlementsWithHighestArea;
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<Country> countries = ReadDataFromFile("countries.txt");
        SortSettlementsByName(countries);
        foreach (Country country in countries)
        {
            Console.WriteLine(country);
            Console.WriteLine();
        }

        Country countryWithHighestDensity = GetCountryWithHighestPopulationDensity(countries);
        Console.WriteLine($"Country with highest population density: {countryWithHighestDensity.Name}");
        Console.WriteLine($"Population density: {countryWithHighestDensity.CalculateHighestPopulationDensity()}");

        Console.WriteLine();

        foreach (PoliticalSystem system in Enum.GetValues(typeof(PoliticalSystem)))
        {
            List<Country> countriesWithSystem = GetCountriesWithSystem(countries, system);
            Console.WriteLine($"Countries with {system} government:");
            foreach (Country country in countriesWithSystem)
            {
                Console.WriteLine($"Country: {country.Name}, Total Area: {GetTotalArea(country)}");
            }
            Console.WriteLine();
        }
    }

    static List<Country> ReadDataFromFile(string filePath)
    {
        List<Country> countries = new List<Country>();

        string[] lines = File.ReadAllLines(filePath);

        Country currentCountry = null;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("Country:"))
            {
                currentCountry = new Country { Name = line.Substring(9) };
                countries.Add(currentCountry);
            }
            else if (line.StartsWith("System:"))
            {
                string system = line.Substring(8);
                if (Enum.TryParse(system, out PoliticalSystem politicalSystem))
                {
                    currentCountry.System = politicalSystem;
                }
                else
                {
                    Console.WriteLine($"Invalid political system: {system}");
                }
            }
            else
            {
                string[] settlementData = line.Split(',');
                if (settlementData.Length == 5)
                {
                    string type = settlementData[0].Trim();
                    string name = settlementData[1].Trim();
                    int population = int.Parse(settlementData[2].Trim());
                    double area = double.Parse(settlementData[3].Trim());

                    Settlement settlement;

                    if (type.Equals("City", StringComparison.OrdinalIgnoreCase))
                    {
                        string mayor = settlementData[4].Trim();
                        settlement = new City { Name = name, Population = population, Area = area, Mayor = mayor };
                    }
                    else if (type.Equals("Village", StringComparison.OrdinalIgnoreCase))
                    {
                        string chairman = settlementData[4].Trim();
                        settlement = new Village { Name = name, Population = population, Area = area, Chairman = chairman };
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid settlement type: {type}");
                    }

                    currentCountry.Settlements.Add(settlement);
                }
            }
        }

        return countries;
    }

    static void SortSettlementsByName(List<Country> countries)
    {
        foreach (Country country in countries)
        {
            country.Settlements = country.Settlements.OrderBy(s => s.Name).ToList();
        }
    }

    static Country GetCountryWithHighestPopulationDensity(List<Country> countries)
    {
        double highestDensity = 0.0;
        Country countryWithHighestDensity = null;

        foreach (Country country in countries)
        {
            double density = country.CalculateHighestPopulationDensity();
            if (density > highestDensity)
            {
                highestDensity = density;
                countryWithHighestDensity = country;
            }
        }

        return countryWithHighestDensity;
    }
    
    static List<Country> GetCountriesWithSystem(List<Country> countries, PoliticalSystem system)
    {
        return countries.Where(c => c.System == system).ToList();
    }

    static double GetTotalArea(Country country)
    {
        return country.Settlements.Sum(s => s.Area);
    }
}
