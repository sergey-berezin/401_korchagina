using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace GenTournament
{

    public class Tournament
    {
        private readonly int N, K, M;
        private readonly Random random;
        private readonly int Population_size, Num_of_crossovers_and_mutated;
        private List<int[,]> population;
        List<int> scores;
        public int[,] best_result;
        public int best_score;

        public Tournament(int n, int k, int m, int population_size, int num_of_crossovers_and_mutated)
        {
            N = n;
            K = k;
            M = m;
            Population_size = population_size;
            Num_of_crossovers_and_mutated = num_of_crossovers_and_mutated;
            scores = (new int[Population_size + Num_of_crossovers_and_mutated]).ToList();
            random = new Random();
            population = create_first_population(); 
        }

        private List<int[,]> create_first_population(){
            // Создание начальной популяции
            var population = new List<int[,]>();
            for (int i = 0; i < Population_size; i++)
            {
                population.Add(create_population());
            }
            return population;
        }
        public List<int[,]> TournamentSelectionWithRanking()
        {
            var combinedData = population.Select((table, index) => new { Table = table, Score = scores[index] });
            var rankedPopulation = combinedData.OrderByDescending(item => item.Score).Select(item => item.Table).ToList();
            best_result = rankedPopulation.First();
            best_score = Calculate_score(best_result);

            ConcurrentBag<int[,]> selectedPopulation = new ConcurrentBag<int[,]>();
            Parallel.For(0, Population_size, i =>
            {
                // Выбираем случайный индекс в пределах турнирного размера
                int tournamentIndex = Random.Shared.Next(Population_size);
                // Добавляем особь с этим индексом из ранжированной популяции
                selectedPopulation.Add(rankedPopulation[tournamentIndex]);
            });
            return selectedPopulation.ToList();
        }
        public void Calculate_scores_general()
        {
            ConcurrentBag<int> paral_scores = new ConcurrentBag<int>();
            Parallel.For(0, population.Count, i =>
            {
                int score = Calculate_score(population[i]);
                scores[i] = score;
            });
        }
        public string[,] Show(int[,] shedule)
        {
            var view = new string[K, M];
            for (int i = 0; i < K; i++) {
                for (int j = 0; j < M; j++)
                {
                    int first = 0;
                    int second = 0;
                    int count = 0;
                    //int k = 0;
                    for (int k = 0; k < N; ++k)
                    {
                        if (shedule[i, k] == j + 1)
                        {
                            if (count == 0)
                            {
                                first = k;
                                count = 1;
                            }
                            else
                            {
                                second = k;
                                count = 2;
                                break;
                            }
                        }
                    }
                    if (count == 0)
                        view[i, j] = "X";
                    else
                    {
                        first++;
                        second++;
                        view[i, j] = "(" + first.ToString() + ", " + second.ToString() + ")";
                    }
                }
            }
            return view;
        }
        public void Generate_best_schedule()
        {
            population = generate_module(population);
            Calculate_scores_general();
            population = TournamentSelectionWithRanking();
            //return population.OrderByDescending(Calculate_score).First();
        }
        public void write(int generation, int[,] best_population)
        {
            Console.Write($"Поколение №{generation + 1}\n");
            Console.Write($"Score: {Calculate_score(best_population)}\n");
            for (int i = 0; i < N; i++)
            {
                Console.Write($"\t{i + 1}");
            }
            Console.WriteLine();
            for (int i = 0; i < K; i++) //туры
            {
                Console.Write(i + 1);
                for (int j = 0; j < N; j++) //участники
                {
                    Console.Write($"\t{best_population[i, j]}"); //площадка
                }
                Console.WriteLine();
            }
        }

        private int[,] create_population()
        {
            var matrix = new int[K, N];
            Array.Clear(matrix, 0, matrix.Length);

            for (int i = 0; i < K; i++)
            {
                //Перемешивание
                var players = Enumerable.Range(0, N).ToList();
                var shuffled_players = new List<int>();
                // Random random = new Random();
                while (players.Count > 0)
                {
                    int randomIndex = random.Next(players.Count);
                    int player = players[randomIndex];
                    players.RemoveAt(randomIndex);
                    shuffled_players.Add(player);
                }
                players = shuffled_players;

                //Выбор площадки
                var id_areas = Enumerable.Range(1, M + 1).ToList();
                for (int j = 0; j < N / 2; j++)
                {
                    int id_area = random.Next(id_areas.Count);
                    int area = id_areas[id_area];
                    matrix[i, players[2 * j]] = area;
                    matrix[i, players[2 * j + 1]] = area;
                    id_areas.RemoveAt(id_area);
                }
            }
            return matrix;
        }

        public int Calculate_score(int[,] matrix)
        {
            var opponents = new Dictionary<int, HashSet<int>>();
            var areas = new Dictionary<int, HashSet<int>>();

            for (int i = 0; i < N; i++)
            {
                opponents[i] = new HashSet<int>();
                areas[i] = new HashSet<int>();
            }

            for (int i = 0; i < K; i++)
            {
                var area_to_players = new Dictionary<int, List<int>>(); //<площадка, список игроков в туре>
                for (int j = 0; j < N; j++)
                {
                    int area = matrix[i, j];
                    if (area != 0)
                    {
                        if (!area_to_players.ContainsKey(area))
                            area_to_players[area] = new List<int>();
                        area_to_players[area].Add(j);
                    }
                }

                // Заполняем посещенные площадки
                foreach (var val in area_to_players)
                {
                    int area = val.Key;
                    foreach (var player in val.Value)
                    {
                        areas[player].Add(area);
                    }
                }

                // Заполняем соперников
                foreach (var players in area_to_players.Values)
                {
                    for (int j = 0; j < players.Count; j++)
                    {
                        for (int k = j + 1; k < players.Count; k++)
                        {
                            opponents[players[j]].Add(players[k]);
                            opponents[players[k]].Add(players[j]);
                        }
                    }
                }
            }

            int minOpponents = opponents.Values.Min(x => x.Count);
            int minVenues = areas.Values.Min(x => x.Count);
            return minOpponents * 1000 + minVenues;
        }

        private List<int[,]> generate_module(List<int[,]> selected)
        {
            ConcurrentBag<int[,]> paral_list = new ConcurrentBag<int[,]>();
            var new_population = new List<int[,]>(selected);
            Parallel.For(0, Num_of_crossovers_and_mutated, _ =>
            {
                int parent1_index = random.Next(selected.Count);
                int parent2_index = random.Next(selected.Count);
                var parent1 = selected[parent1_index];
                var parent2 = selected[parent2_index];

                //Смешивание родителей и мутация
                var child = crossover(parent1, parent2);
                mutate(child);

                // Добавляем ребенка в новую популяцию
                paral_list.Add(child);
            });
            new_population.AddRange(paral_list);

            return new_population;
        }

        private int[,] crossover(int[,] parent1, int[,] parent2)
        {
            // Выбор родителей в каждом туре 
            int[,] child = new int[K, N];
            for (int i = 0; i < K; i++)
            {
                int[,] sourceParent = random.Next(2) == 0 ? parent1 : parent2;
                for (int j = 0; j < N; j++)
                {
                    child[i, j] = sourceParent[i, j]; //выбор площадки
                }
            }
            return child;
        }

        private void mutate(int[,] child)
        {
            for (int i = 0; i < K; i++)
            {
                if (random.NextDouble() < 0.1) // 10% шанс мутации поменять площадки местами
                {
                    // Находим две случайные пары, играющие на разных площадках и меняем местами
                    int player1 = random.Next(N);
                    int player2 = 0;
                    while (player2 == player1 || child[i, player1] == child[i, player2])
                    {
                        player2 = random.Next(N);
                    }
                    swap_players(child, i, player1, player2);

                    int player3 = 0;
                    int player4 = 0;
                    for (int j = 0; j < N; j++)
                    {
                        if (j != player1 && child[i, j] == child[i, player1])
                        {
                            player3 = j;
                        }
                        if (j != player2 && child[i, j] == child[i, player2])
                        {
                            player4 = j;
                        }
                    }
                    swap_players(child, i, player3, player4);
                }
            }
        }

        private void swap_players(int[,] child, int round, int player1, int player2)
        {
            int temp = child[round, player1];
            child[round, player1] = child[round, player2];
            child[round, player2] = temp;
        }

    }
}