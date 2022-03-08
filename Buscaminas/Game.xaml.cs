using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Buscaminas
{
    public partial class Game : Window
    {
        private static readonly DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        private int bombs = 10;
        private int markedBombs = 0;
        private int seed = 0;
        private int seconds = 0;
        private int rows = 8;
        private int cols = 8;
        private int counter = 0;
        private int boxes = 0;
        public Game()
        {
            InitializeComponent();
            // Define how many boxes exists
            boxes = rows * cols;

            // Create Seed
            seed = new Random().Next(0, 29259);

            // Print total quantity of bombs
            lbBombs.Content = bombs - markedBombs;

            // Start timer
            timer.Tick += Timer_Tick;
            timer.Start();

            // Button printer
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Box box = new Box { Name = $"btn{counter}" };
                    box.SetValue(Grid.RowProperty, i);
                    box.SetValue(Grid.ColumnProperty, j);
                    box.position = counter;
                    box.FontWeight = FontWeights.Bold;
                    box.FontSize = 16;
                    box.Foreground = box.color;
                    box.Click += new RoutedEventHandler(LeftClick);
                    box.MouseRightButtonUp += new MouseButtonEventHandler(RightClick);
                    box.row = i;
                    box.col = j;

                    boxContainer.Children.Add(box);
                    counter++;
                }
            }

            // Create algorythm for selecting the bombs position based on seed number
            Random rnd = new Random(seed);
            int[] bombsPositions = new int[10];
            for (int i = 0; i < bombs; i++)
            {
                int bombPos = rnd.Next(0,63);
                while (true)
                {
                    if (bombsPositions.Contains(bombPos)) bombPos = rnd.Next(0, 63); else break;
                }
                bombsPositions[i] = bombPos;
                Box box = (Box)boxContainer.Children[bombPos];
                box.isBomb = true;
            }

            // Detect how many bombs are around
            for(int i = 0; i < boxes; i++)
            {
                Box box = (Box)boxContainer.Children[i];
                if (box.isBomb)
                {
                    for (int r = (box.row - 1); r <= (box.row + 1); r++)
                    {
                        for (int c = (box.col -1); c <= (box.col + 1); c++)
                        {
                            if (r >= 0 && c >= 0 && r < rows && c < cols)
                            {
                                int nearPosition = 8 * r + c;
                                Box nearBox = (Box)boxContainer.Children[nearPosition];
                                nearBox.bombsAround = nearBox.isBomb ? 0 : nearBox.bombsAround + 1;
                            }
                        }
                    }
                }   
            }

            // Change Foreground based on how many bombs around
            for(int i = 0; i < boxes; i++)
            {
                Box box = (Box)boxContainer.Children[i];

                switch (box.bombsAround)
                {
                    case 0: box.color = new SolidColorBrush(Colors.Black); break;
                    case 1: box.color = new SolidColorBrush(Colors.Blue); break;
                    case 2: box.color = new SolidColorBrush(Colors.Green); break;
                    case 3: box.color = new SolidColorBrush(Colors.Red); break;
                    case 4: box.color = new SolidColorBrush(Colors.Purple); break;
                    default: box.color = new SolidColorBrush(Colors.Orange); break;
                }
            }
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            Box box = (Box)sender;
            if (!box.isMarked)
            {
                ClickBox(box);
                if (box.bombsAround == 0)
                {
                    for (int i = (box.row - 1); i <= (box.row + 1); i++)
                    {
                        for (int j = (box.col - 1); j <= (box.col + 1); j++)
                        {
                            if (i >= 0 && j >= 0 && i <= 7 && j <= 7)
                            {
                                int pos = 8 * i + j;
                                Box nearBox = (Box)boxContainer.Children[pos];
                                if (!nearBox.isBomb && nearBox != box) ClickBox(nearBox);
                            }
                        }
                    }
                }
            }
        }

        private void ClickBox(Box box)
        {
            box.IsEnabled = false;
            box.Foreground = box.color;
            box.isHidden = false;
            if (box.isBomb)
            {
                box.Content = "☻";
                box.Background = new SolidColorBrush(Colors.Red);
                GameOver();
            }
            else
            {
                box.Content = box.bombsAround != 0 ? box.bombsAround.ToString() : "";

                if (box.isMarked)
                {
                    box.isMarked = false;
                    MarkBox(box);
                }
                box.isMarked = false;
            }
        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            Box box = (Box)sender;
            box.isMarked = !box.isMarked;
            if (box.isHidden) MarkBox(box);
        }

        private void MarkBox(Box box)
        {
            markedBombs = box.isMarked ? markedBombs+1 : markedBombs-1;
            box.Content = box.isMarked ? "🚩" : null;
            lbBombs.Content = bombs - markedBombs;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds++;
            lbSeconds.Content = seconds.ToString().PadLeft(3, '0');
        }

        // Define Box Class
        public class Box : Button
        {
            public bool isBomb { get; set; } = false;
            public bool isHidden { get; set; } = true;
            public bool isMarked { get; set; } = false;
            public int bombsAround { get; set; } = 0;
            public int position { get; set; } = 0;
            public int row { get; set; } = 0;
            public int col { get; set; } = 0;
            public SolidColorBrush color = new SolidColorBrush(Colors.Red);
        }

        // Game Over function
        private void GameOver()
        {
            foreach (Box box in boxContainer.Children)
            {
                box.IsEnabled = false;

                box.Foreground = box.color;

                if (box.isBomb)
                {
                    box.Content = "☻";
                }else if (box.bombsAround > 0)
                {
                    box.Content = box.bombsAround;
                }
                else
                {
                    box.Content = null;
                }

            }
            timer.Stop();
            btnStart.Content = "Restart";
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            new Game().Show();
            Close();
        }
    }
}
