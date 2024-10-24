using GenTournament;
class Program
{
    static void Main(string[] args)
    {
        int N = 10, K = 9, M = 6;

        int population_size = 10;
        int num_of_crossovers_and_mutated = 10;
        int steps_of_algorithm = 80;
        bool CTRL_C_NOT_PRESSED = true;

        Tournament manager = new Tournament(N, K, M, population_size, num_of_crossovers_and_mutated);

        Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);
        void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            CTRL_C_NOT_PRESSED = false;
        }

        for (int i = 0; i < steps_of_algorithm; i++)
        {
            if (!CTRL_C_NOT_PRESSED)
                break;
            int[,] result = manager.Generate_best_schedule();
            manager.write(i, result);

        }
    }
}