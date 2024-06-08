using System.Text.Json;
using VismaTask.Models;
namespace VismaTask.Services;

public class ShortageService
{
    private readonly string filePath = "shortages.json";
    private List<Shortage> shortages;
    public ShortageService() 
    {
        shortages = InitShortages();
    }
    private List<Shortage> InitShortages()
    {
        if (!File.Exists(filePath))
        {
            return new List<Shortage>();
        }
        var jsonData = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<List<Shortage>>(jsonData) ?? new List<Shortage>();
    }
    public void CreateShortage(Shortage shortage)
    {
        var existing = shortages.FirstOrDefault(s => s.Title == shortage.Title && s.Room == shortage.Room);
        if (existing != null)
        {
            if (shortage.Priority > existing.Priority)
            {
                shortages.Remove(existing);
                shortages.Add(shortage);
                SaveShortages();
                Console.Clear();
                Console.WriteLine("Shortage replaced\nPress any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: A shortage already exists, a new one has not been created.\nPress any key to continue...");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
            }
        }
        else
        {
            shortages.Add(shortage);
            SaveShortages();
            Console.WriteLine("Shortage created\nPress any key to continue...");
            Console.ReadKey();
        }
    }
    public List<Shortage> GetAllShortages(string userName,bool isAdmin)
    {
        if (isAdmin)
        {
            return shortages;
        }
        else
        {
           return shortages.Where(s => s.Name == userName).ToList();
        }
    }
    public void DeleteShortage(string title, string room, string userName, bool isAdmin)
    {
        var shortage = shortages.FirstOrDefault(s => s.Title == title && s.Room == room);
        if (shortage != null)
        {
            if (isAdmin || shortage.Name == userName)
            {
                shortages.Remove(shortage);
                SaveShortages();
                Console.Clear();
                Console.WriteLine("Shortage deleted.\nPress any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Not enough permissions to do this\nPress any key to continue...");
                Console.ReadKey();
            }
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Shortage not found\nPress any key to continue...");
            Console.ReadKey();
        }
    }
    public void SaveShortages()
    {
        var jsonData = JsonSerializer.Serialize(shortages, new JsonSerializerOptions {WriteIndented = true});
        File.WriteAllText(filePath, jsonData);
    }
}
