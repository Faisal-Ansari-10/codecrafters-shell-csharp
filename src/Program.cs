class Program
{
    static void Main()
    {
        List<string> builtIncommands = ["echo", "exit", "type"];
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
            } else if(command.StartsWith("type "))
            {
                var argCommand = command[5..];
                if(builtIncommands.Contains(argCommand))
                {
                    Console.WriteLine($"{argCommand} is a shell builtin");
                } else
                {
                    Console.WriteLine($"{argCommand}: not found");
                }          
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }

        } while (true);

    }
}
