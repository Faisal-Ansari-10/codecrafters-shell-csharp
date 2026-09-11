class Program
{
    static void Main()
    {
        do
        {
            Console.Write("$ ");
            var command = Console.ReadLine();
            if(command!.Equals("exit")) break;
            Console.WriteLine($"{command}: command not found");
            
        } while (true);

    }
}
