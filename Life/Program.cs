using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Runtime.CompilerServices;

namespace cli_life
{
    public class Combination
    {
        public readonly List<Cell> Cells;
        public readonly string Name;
        public Combination(List<Cell> cells, string name)
        {
            Cells = cells;
            Name = name;
        }
    }

    public class ListCombinations
    {
        private List<Combination> Combinations = new List<Combination>();
        private Dictionary<string, int> Info = new Dictionary<string, int>();
        public void AddCombination(List<Cell> cells)
        {
            string name = Identification(cells);
            Combinations.Add(new Combination(cells, name));
            if (Info.ContainsKey(name))
            {
                Info[name]++;
            }
            else
            {
                Info.Add(name, 1);
            }
        }
        private string Identification(List<Cell> cells)
        {
            string name = "Unknown";
            return name;
        }
        public void ClearList()
        {
            if (Combinations.Count != 0)
            {
                Info.Clear();
                Combinations.Clear();
            }
        }
        public Dictionary<string, int> GetInfo()
        {
            return Info;
        }

        public int Count()
        {
            return Combinations.Count;
        }
    }
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;
        public readonly ListCombinations Combinations;
        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }
        private int Iteration;
        public Board(int width, int height, int cellSize, double liveDensity = .1, int iteration = 0)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors(ref Cells);
            Randomize(liveDensity);
            Iteration = iteration;
            Combinations = new ListCombinations();
            FoundCombinations();
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void SetInitialData(int[,] arrayCells, int iteration)
        {
            Iteration = iteration;
            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    Cells[i, j].IsAlive = arrayCells[j, i] == 1 ? true : false;
                }
            }
            FoundCombinations();
        } 
        public void Advance()
        {
            Iteration++;
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
            FoundCombinations();
        }
        public int GetIteration() { return Iteration; }
        public int GetCountLifeCells()
        {
            int count = 0;
            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (Cells[i, j].IsAlive) count++;
                }
            }
            return count;
        }

        private void FoundCombinations()
        {
            Combinations.ClearList();
            List<Cell> combination = new List<Cell>();
            Queue<Cell> queueCellsToAdd = new Queue<Cell>();
            Cell[,] cellsForIncluding = new Cell[Columns, Rows];

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    cellsForIncluding[x, y] = new Cell();
                    cellsForIncluding[x, y].IsAlive = Cells[x, y].IsAlive ? true : false;
                }
            }

            ConnectNeighbors(ref cellsForIncluding);

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (cellsForIncluding[x, y].IsAlive)
                    {     
                        queueCellsToAdd.Enqueue(cellsForIncluding[x, y]);
                        while (queueCellsToAdd.Count != 0)
                        {
                            foreach (var cell in queueCellsToAdd.Peek().neighbors)
                            {
                                if (cell.IsAlive)
                                { 
                                    queueCellsToAdd.Enqueue(cell);
                                }                                
                            }
                            combination.Add(queueCellsToAdd.Peek());
                            queueCellsToAdd.Peek().IsAlive = false;
                            queueCellsToAdd.Dequeue();
                        }
                        Combinations.AddCombination(combination);
                        combination.Clear();
                    }
                }
            }
        }
        private void ConnectNeighbors(ref Cell[,] Cells)
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }
    class Program
    {
        static Board board;
        static DateTime StartTime = DateTime.Now;
        static private void Reset()
        {   
            string currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(Path.Combine(currentPath, "BoardConditions")))
                Directory.CreateDirectory(Path.Combine(currentPath, "BoardConditions"));
            if (!Directory.Exists(Path.Combine(currentPath, "StartBoard")))
                Directory.CreateDirectory(Path.Combine(currentPath, "StartBoard"));

            string json = System.IO.File.ReadAllText(@"settings.json");
            board = JsonConvert.DeserializeObject<Board>(json);

            ReadBoardFromFile();

        }
        static void Render()
        {
            Console.WriteLine($"Iteration: {board.GetIteration()}");
            Console.WriteLine($"Count combination: {board.Combinations.Count()}");
            Console.WriteLine($"Count life cells: {board.GetCountLifeCells()}");
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }

        static void WriteBoardToFile()
        {
            List<string> BoardCondition = new List<string>();
            string RowStr;
            BoardCondition.Add($"Iteration: {board.GetIteration()}");
            for (int row = 0; row < board.Rows; row++)
            {
                RowStr = "";
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        RowStr += "1";
                    }
                    else
                    {
                        RowStr += "0";
                    }
                }
                BoardCondition.Add(RowStr);
            }
            System.IO.File.AppendAllLines(@"BoardConditions/BoardConditions" + StartTime.ToString("HHmmss") + ".txt", BoardCondition);
            System.IO.File.AppendAllText(@"BoardConditions/CountLifeCells" + StartTime.ToString("HHmmss") + ".txt", board.GetCountLifeCells().ToString()+"\n");

        }
        static void ReadBoardFromFile()
        {
            DirectoryInfo DI = new DirectoryInfo(@"StartBoard");
            string firstFileName = DI.EnumerateFiles()
                      .Select(f => f.Name)
                      .FirstOrDefault();
            Console.WriteLine(firstFileName);

            if (firstFileName != null)
            {
                string[] input = File.ReadAllLines(@"StartBoard/"+firstFileName);

                int iteration;
                bool resultParse = int.TryParse(input[0].Split(" ")[1], out iteration);
                if (resultParse == false) iteration = 0; 
                int[,] inputCells = new int[input.Length-1, input[1].Length];
                string[] inputStr;

                for (int i = 0; i < inputCells.GetLength(0); i++)
                {
                    inputStr = input[i+1].Select(ch => ch.ToString()).ToArray();
                    for (int j = 0; j < inputCells.GetLength(1); j++)
                    {
                        resultParse = int.TryParse(inputStr[j], out inputCells[i, j]);
                        if (resultParse == false) inputCells[i, j] = 0;
                    }
                }

                board.SetInitialData(inputCells, iteration);
            }
        }
        static void Main(string[] args)
        {
            Reset();
            while(true)
            for (int i=0; i<10000; i++)
            {
                WriteBoardToFile();
                Console.Clear();
                Render();              
                board.Advance();
                Thread.Sleep(1000);
            }
        }
    }
}
