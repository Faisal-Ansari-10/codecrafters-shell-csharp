class Program
{
    static void Main()
    {
        do
        {
            Console.Write("$ ");
            var command = Console.ReadLine() ?? "";
            if (command == "exit")
            {
                break;
            }
            else if (command.StartsWith("echo "))
            {
                Console.WriteLine($"{command[5..]}");
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }

        } while (true);

    }
}
