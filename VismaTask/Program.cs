using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Schema;
using VismaTask.Models;
using VismaTask.Services;

namespace VismaTask;

internal class Program
{
    private static ShortageService _shortageService = new ShortageService();
    private static string username = string.Empty;
    private static bool isAdmin = false;
    private static string welcomeMessage = string.Empty;
    static void Main(string[] args)
    {
        LogIn();
        while (true)
        {
            Console.Clear();
            Console.Write(welcomeMessage);
            var command = Console.ReadLine().Trim().ToLower();
            switch (command)
            {
                case "register":
                    Register();
                    break;
                case "delete":
                    Delete();
                    break;
                case "list":
                    ListAll();
                    break;
                case "logout":
                    LogIn();
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
            }

        }
    }

    private static void LogIn()
    {
        Console.Clear();
        Console.WriteLine("\x1b[33mWarning: For Admin mode type admin\x1b[0m");
        Console.Write("Enter User Name: ");
        username = Console.ReadLine();
        isAdmin = (username == "admin");
        if (isAdmin)
        {
            welcomeMessage = "\x1b[31mAdmin mode\x1b[0m\nAvailable commands: | register | delete | list | logout | exit |\nEnter the command to proceed: ";
        }
        else
        {
            welcomeMessage = $"Logged in as \x1b[32m{username}\x1b[0m\nAvailable commands: | register | delete | list | exit |\nEnter the command to proceed: ";
        }

    }

    public static string SelectOption(string title, string[] options, bool listMode)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Select a {title}:");
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"[\x1b[32m{i + 1}\x1b[0m] - {options[i]}");
            }
            Console.WriteLine("\x1b[33mNote: Press a required key\x1b[0m");
            if (listMode)
            {
                Console.WriteLine("Press any other key to reset");
            }
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    return options[0];
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    return options[1];

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    return options[2];
                default:
                    if (listMode == true)
                    {
                        return "";
                    }
                    else
                    {
                        
                        Console.WriteLine();
                        Console.WriteLine("Wrong input, try again in 1s");
                        Thread.Sleep(1000);
                        break;
                    }
                    
            }
        }
    }
    public static string SelectRoom(bool listMode)
    {
        string[] rooms = { "Meeting room", "Kitchen", "Bathroom" };
        return SelectOption("Room", rooms, listMode);
    }
    public static string SelectCategory(bool listMode)
    {
        string[] categories = { "Electronics", "Food", "Other" };
        return SelectOption("Category", categories, listMode);
    }

    private static void Register()
    {
        Console.Clear();
        Console.WriteLine("Registering a new shortage");
        Shortage shortage = new Shortage();
        Console.Write("Enter the Title: ");
        shortage.Title = Console.ReadLine();
        shortage.Name = username;
        shortage.Room = SelectRoom(false);
        shortage.Category = SelectCategory(false);
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Enter the priority (Number 1-10, \x1b[32m1\x1b[0m - not important, \x1b[31m10\x1b[0m - very important)");
            if (int.TryParse(Console.ReadLine(), out int input))
            {
                if (input >= 1 && input <= 10)
                {
                    shortage.Priority = input;
                    shortage.CreatedOn = DateTime.Now;
                    _shortageService.CreateShortage(shortage);
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong priority number, try again.\nPress any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Only numbers is acceptable, try again.\nPress any key to continue...");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
            }
        }
    }
    private static void Delete()
    {
        Console.Write("Enter the title of shortage you want to delete: ");
        string title = Console.ReadLine();
        string room = SelectRoom(false);
        _shortageService.DeleteShortage(title, room, username, isAdmin);
    }
    private static void SetDateFilter(ref DateTime? startDate, ref DateTime? endDate)
    {
        Console.Write("Leave Blank To Reset\nEnter the Start Date (yyyy-MM-dd): ");
        var startInput = Console.ReadLine();
        Console.Write("\nEnter the End Date (yyyy-MM-dd): ");
        var endInput = Console.ReadLine();
        if (DateTime.TryParse(startInput, out DateTime start) && DateTime.TryParse(endInput, out DateTime end))
        {
            startDate = start;
            endDate = end;
        }
        else if (startInput == "" || endInput == "")
        {
            startDate = null;
            endDate = null;
        }
        else
        {
            Console.WriteLine("Invalid date format. Please try again.");
            Thread.Sleep(1000);
        }
    }
    private static void ShowResults(string titleFilter, DateTime? startDateFilter, DateTime? endDateFilter, string categoryFilter, string roomFilter)
    {
        Console.Clear();
        var filteredList = _shortageService.GetAllShortages(username, isAdmin).AsQueryable();

        if (!string.IsNullOrEmpty(titleFilter))
        {
            filteredList = filteredList.Where(s => s.Title.Contains(titleFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (startDateFilter.HasValue)
        {
            filteredList = filteredList.Where(s => s.CreatedOn >= startDateFilter.Value);
        }

        if (endDateFilter.HasValue)
        {
            filteredList = filteredList.Where(s => s.CreatedOn <= endDateFilter.Value);
        }

        if (!string.IsNullOrEmpty(categoryFilter))
        {
            filteredList = filteredList.Where(s => s.Category.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(roomFilter))
        {
            filteredList = filteredList.Where(s => s.Room.Equals(roomFilter, StringComparison.OrdinalIgnoreCase));
        }
        var sortedList = filteredList.OrderByDescending(s => s.Priority).ToList();
        int nameWidth = Math.Max("Name".Length, sortedList.Any() ? sortedList.Max(s => s.Name.Length) : 0);
        int titleWidth = Math.Max("Title".Length, sortedList.Any() ? sortedList.Max(s => s.Title.Length) : 0);
        int dateWidth = "Created On".Length;
        int categoryWidth = Math.Max("Category".Length, sortedList.Any() ? sortedList.Max(s => s.Category.Length) : 0);
        int roomWidth = Math.Max("Room".Length, sortedList.Any() ? sortedList.Max(s => s.Room.Length) : 0);
        int priorityWidth = "Priority".Length;
        Console.WriteLine($"{"Name".PadRight(nameWidth)} | {"Title".PadRight(titleWidth)} | {"Created On".PadRight(dateWidth)} | {"Category".PadRight(categoryWidth)} | {"Room".PadRight(roomWidth)} | {"Priority".PadRight(priorityWidth)}");
        Console.WriteLine(new string('-', nameWidth + titleWidth + dateWidth + categoryWidth + roomWidth + priorityWidth + 15));
        foreach (var shortage in sortedList)
        {
            Console.WriteLine($"{shortage.Name.PadRight(nameWidth)} | {shortage.Title.PadRight(titleWidth)} | {shortage.CreatedOn:yyyy-MM-dd} | {shortage.Category.PadRight(categoryWidth)} | {shortage.Room.PadRight(roomWidth)} | {shortage.Priority.ToString().PadRight(priorityWidth)}");
        }
        Console.WriteLine("\nPress C to Create a shortage, Press D to Delete a shortage to, Press any key to return to the filter menu...");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.C:
                Register();
                ShowResults(titleFilter, startDateFilter, endDateFilter, categoryFilter, roomFilter);
                break;
            case ConsoleKey.D:
                Delete();
                ShowResults(titleFilter, startDateFilter, endDateFilter, categoryFilter, roomFilter);
                break;
            default:
                break;
        }

    }
    private static void ListAll()
    {
        string titleFilter = string.Empty;
        DateTime? startDateFilter = null;
        DateTime? endDateFilter = null;
        string roomFilter = string.Empty;
        string categoryFilter = string.Empty;
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Choose the filters you would like to Apply: \n" +
                $"[1] Filter by Title{(string.IsNullOrEmpty(titleFilter) ? " \x1b[32mNo Filter Applied\x1b[0m" : $" \x1b[33mApplied filter: {titleFilter}")}\x1b[0m\n" +
                $"[2] Filter by CreatedOn date{(startDateFilter == null && endDateFilter == null ? " \x1b[32mNo Filter Applied\x1b[0m" : $" \x1b[33m Applied filter: From {startDateFilter?.ToString("yyyy-MM-dd")} to {endDateFilter?.ToString("yyyy-MM-dd")}\x1b[0m")}\n" +
                $"[3] Filter by Category{(string.IsNullOrEmpty(categoryFilter) ? " \x1b[32mNo Filter Applied\x1b[0m" : $" \x1b[33mApplied filter: {categoryFilter}")}\x1b[0m\n" +
                $"[4] Filter by Room{(string.IsNullOrEmpty(roomFilter) ? " \x1b[32mNo Filter Applied\x1b[0m" : $" \x1b[33mApplied filter: {roomFilter}")}\u001b[0m\n" +
                $"[5] Show the results\n" +
                $"[6] Exit");
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Console.Clear();
                    Console.Write("Press Enter to reset\nEnter the Title: ");
                    titleFilter = Console.ReadLine();
                    Console.Clear();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Console.Clear();
                    SetDateFilter(ref startDateFilter, ref endDateFilter);
                    Console.Clear();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    categoryFilter = SelectCategory(true);
                    Console.Clear();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    roomFilter = SelectRoom(true);
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    ShowResults(titleFilter, startDateFilter, endDateFilter, categoryFilter, roomFilter);
                    break;
                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    return;
                default:
                    Console.WriteLine("Invalid input, please try again in 1s.");
                    Thread.Sleep(1000);
                    break;

            }
        }
    }
    

}
