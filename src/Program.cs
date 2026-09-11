class Program
{
    static void Main()
    {
        do
        {
            Console.Write("$ ");
            var command = Console.ReadLine();
            Console.WriteLine($"{command}: command not found");
        } while (true);

    }
}
