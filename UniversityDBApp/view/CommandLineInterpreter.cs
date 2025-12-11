using UniversityDBApp.controller;
using UniversityDBApp.model;
using ConsoleTables;
using UniversityDBApp.view;
namespace UniversityDBApp.view;

public static class CommandLineInterpreter
{
    public static void Run()
    {
        Console.WriteLine("This is the terminal for the University DB Application");

        CommandHandler.Help(null);

        while (true)
        {
            try
            {
                Console.Write("> ");
                string[]? input = Console.ReadLine()?.Split(" ");
                string? command = input?[0];
                string[]? args = input?.Skip(1).ToArray();

                if (command == null) continue;

                switch (command)
                {
                    case "exit":
                        CommandHandler.Exit();
                        return;
                    case "help":
                        CommandHandler.Help(args?[0]);
                        break;
                    case "find" when args?.Length == 3:
                        CommandHandler.Find(args);
                        break;
                    case "update" when args?.Length == 4:
                        CommandHandler.Update(args);
                        break;
                    case "allocate" when args?.Length == 6:
                        CommandHandler.Allocate(args);
                        break;
                    case "deallocate" when args?.Length == 4:
                        CommandHandler.DeAllocate(args);
                        break;
                    case "create" when args?.Length >= 5:
                        CommandHandler.Create(args);
                        break;
                    default:
                        Console.WriteLine("Write a valid command. To see a list of valid commands write: help");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

