using GenTournament;
class Program
{
    static void Main(string[] args)
    {
        int N = 13, K = 15, M = 6;

        Tournament manager = new Tournament(N, K, M);
        int[,] matrix = manager.Generate_choise();
        //int[,] matrix = manager.GenerateSchedule();

        Console.WriteLine("---------Итоговое расписание:-------------");
        for (int i = 0; i < N; i++){
            Console.Write($"\t{i + 1}");
        }
        Console.WriteLine();
        for (int i = 0; i < K; i++) //туры
        {
            Console.Write(i + 1);
            for (int j = 0; j < N; j++) //участники
            {
                Console.Write($"\t{matrix[i, j]}"); //площадка
            }
            Console.WriteLine();
        }
    }
}