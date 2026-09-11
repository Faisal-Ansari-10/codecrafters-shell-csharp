class Program
{
    static void Main()
    {
        do
        {
            Console.Write("$ ");
            var command = Console.ReadLine() ?? "";
            var splits = command.Split(" ");
            if (splits[0] == "exit")
            {
                break;
            }
            else if (splits[0] == "echo")
            {
                Console.WriteLine($"{string.Join(" ", splits[1..])}");
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }

        } while (true);

    }
}
