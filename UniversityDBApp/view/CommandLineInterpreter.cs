namespace UniversityDBApp.view;

public static class CommandLineInterpreter
{
    public static void Run()
    {
        Console.WriteLine("This is the terminal for the University DB Application");
        Console.WriteLine("Write your commands here\n");
            

        while (true)
        {
            Console.Write("> ");
            var command = Console.ReadLine();

            if (command == "exit" || command == "quit" || command == "Q")
            {
                Console.WriteLine("Exiting...");
                break;
            }

            Console.WriteLine($"Your command was: {command}");
        }
    }
}