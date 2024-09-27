using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

class Program
{
    public class Completion
    {
        public string Name { get; set; }
        public string Timestamp { get; set; }
        public string Expires { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public List<Completion> Completions { get; set; }
    }

    static void Main(string[] args)
    {
        // Load the JSON file
         string jsonFilePath = "/Users/abi/Documents/Abinaya Project/Trainings/trainings (correct).txt"; // Path to the file
        var people = JsonConvert.DeserializeObject<List<Person>>(File.ReadAllText(jsonFilePath));

        // Output 1: List each completed training with a count of how many people have completed it
        var completionCounts = people.SelectMany(p => p.Completions)
                                     .GroupBy(c => c.Name)
                                     .Select(g => new { Training = g.Key, Count = g.Count() })
                                     .ToList();

        // Write Output 1 to JSON file
        File.WriteAllText("TrainingCompletionCounts.json", JsonConvert.SerializeObject(completionCounts, Formatting.Indented));

        // Output 2: List all people who completed specified trainings in the fiscal year 2024
        List<string> trainings = new List<string>
        {
            "Electrical Safety for Labs", "X-Ray Safety", "Laboratory Safety Training"
        };
        DateTime fiscalYearStart = new DateTime(2023, 7, 1);
        DateTime fiscalYearEnd = new DateTime(2024, 6, 30);

        var fiscalYearCompletions = new List<dynamic>();

        foreach (var person in people)
        {
            var completionsInFiscalYear = person.Completions
                .Where(c => trainings.Contains(c.Name) &&
                            DateTime.Parse(c.Timestamp) >= fiscalYearStart &&
                            DateTime.Parse(c.Timestamp) <= fiscalYearEnd)
                .ToList();

            if (completionsInFiscalYear.Any())
            {
                fiscalYearCompletions.Add(new
                {
                    PersonName = person.Name,
                    Completions = completionsInFiscalYear
                });
            }
        }

        // Write Output 2 to JSON file
        File.WriteAllText("FiscalYearCompletions.json", JsonConvert.SerializeObject(fiscalYearCompletions, Formatting.Indented));

        // Output 3: Find people with trainings expiring or expired by a specific date
        DateTime checkDate = new DateTime(2023, 10, 1);
        var expiringTrainings = new List<dynamic>();

        foreach (var person in people)
        {
            var personExpiringTrainings = person.Completions
                .Where(c => !string.IsNullOrEmpty(c.Expires) && DateTime.Parse(c.Expires) <= checkDate.AddMonths(1))
                .Select(c => new
                {
                    Training = c.Name,
                    ExpirationDate = DateTime.Parse(c.Expires),
                    Status = DateTime.Parse(c.Expires) < checkDate ? "Expired" : "Expires Soon"
                }).ToList();

            if (personExpiringTrainings.Any())
            {
                expiringTrainings.Add(new
                {
                    PersonName = person.Name,
                    Trainings = personExpiringTrainings
                });
            }
        }

        // Write Output 3 to JSON file
        File.WriteAllText("ExpiringTrainings.json", JsonConvert.SerializeObject(expiringTrainings, Formatting.Indented));

        Console.WriteLine("Outputs have been written to JSON files.");
    }
}
