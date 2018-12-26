using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication1;

namespace Hungry_Snake
{
    enum Mode
    {
        Survival,
        Arcade,
        Challenge
    };

    enum Direction
    {
        Left = -1,
        Right = 1,
        Up = 10,
        Down = -10
    };

    static class Program
    {
        private const int width                 = 78; //ширина вікна консолі
        private const int height                = 24; //довжина вікна консолі

        private static char[,] gameField        = new char[height, width];
        private static List<Point> snake        = new List<Point>();
        private static List<Point> cloneSnake;

        private static Direction myDir, botDir;
        private static Mode gameMode;

        private static bool FOODEXIST           = false;
        private static bool GAMEOVER            = false;
        private static bool IEAT                = false;
        private static bool PAUSE               = false;
        private static bool BOTEAT              = false;
        private static Point fragment, head, clone_head, meal;
        private static int tx                   = 0;
        private static int ty                   = 0;
        private static int speed                = 100;
        private static int select               = 0;
        private static int level                = 1;
        private static long score               = 0;

        //private static Task t = new Task(StreamArcadeGame);
        private static Thread backgrnd          = new Thread(StreamSurvivalGame); //поток 1
        private static Thread backgroundArcade  = new Thread(StreamArcadeGame); //поток 2
        private static Thread backgroundClone   = new Thread(StreamChallengeGame); //поток 3
        private static Random randomize         = new Random();
        private static ConsoleKeyInfo keyInfo;
        /// <summary>
        /// Ігровий цикл
        /// </summary>
        static void Main(string[] args)
        {
            Console.Title = "[Game]Hungry snake in the console";
            Console.SetWindowSize(width + 1, height + 1);
            Console.SetBufferSize(width + 1, height + 1);
            Console.CursorVisible = false;
            Graphics.ShowLogo(); //показати стартовий екран

            while (true) //обробник натискання клавіш на стартовому екрані
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                    Environment.Exit(0); //вихід із программи
                if (keyInfo.Key == ConsoleKey.D1)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Graphics.Blink(1);
                    }
                    gameMode = Mode.Survival;
                    break;
                }
                if (keyInfo.Key == ConsoleKey.D2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Graphics.Blink(2);
                    }
                    gameMode = Mode.Arcade;
                    break;
                }
                if (keyInfo.Key == ConsoleKey.D3)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Graphics.Blink(3);
                    }
                    gameMode = Mode.Challenge;
                    break;
                }
                    for (int i = 0; i < 3; i++)
                    {
                        Graphics.Blink(0);
                    }
            }

            Console.Clear();

            if (gameMode == Mode.Survival)
                SetSurvivalMode();
            else if (gameMode == Mode.Arcade)
                SetArcadeMode();
            else if (gameMode == Mode.Challenge)
                SetChallenge();

        }
        /// <summary>
        /// Первинна промальовка змійки(method of class Snake)
        /// </summary>
        static void InitiateSnake()
        {
            myDir = Direction.Left;

            snake = new List<Point>();
            head = new Point(35, 12);
            snake.Add(head);
            head = new Point(36, 12);
            snake.Add(head);
            head = new Point(37, 12);
            snake.Add(head);
            head = snake[0];

            for (int i = 0; i < 23; i++)
            {
                for (int j = 0; j < 77; j++)
                {
                    gameField[i, j] = ' ';
                }
            }

            for (int i = 0; i < snake.Count; i++)
            {
                gameField[snake[i].Y, snake[i].X] = 'O';
                Console.SetCursorPosition(snake[i].X + 1, snake[i].Y + 1);
                Console.Write('O');
            }
        }
        /// <summary>
        /// Первинна промальовка змійки-суперника(method of class Snake)
        /// </summary>
        static void InitiateSnakebot()
        {
            botDir = Direction.Right;

            cloneSnake = new List<Point>();
            clone_head = new Point(59, 7);
            cloneSnake.Add(clone_head);
            clone_head = new Point(60, 7);
            cloneSnake.Add(clone_head);
            clone_head = new Point(61, 7);
            cloneSnake.Add(clone_head);
            clone_head = cloneSnake[0];

            for (int i = 0; i < cloneSnake.Count; i++)
            {
                gameField[cloneSnake[i].Y, cloneSnake[i].X] = '*';
                Console.SetCursorPosition(cloneSnake[i].X + 1, cloneSnake[i].Y + 1);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write('*');
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Генератор їжі на випадкових координатах ігрового поля(method of class FoodFactory)
        /// </summary>
        static void CreateFood()
        {
            tx = randomize.Next(0, width-1);
            ty = randomize.Next(0, height-1);
            while (gameField[ty, tx] == 'O' || gameField[ty, tx] == 'X')
            {
                tx = randomize.Next(0, width-1);
                ty = randomize.Next(0, height-1);
            }
            meal = new Point(tx, ty);
            gameField[ty, tx] = '@';
            Console.SetCursorPosition(tx + 1, ty + 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('@');
            Console.ForegroundColor = ConsoleColor.White;
            FOODEXIST = true;
        }
        /// <summary>
        /// Процесс руху змійки у ігровому полі(method of class Snake)
        /// </summary>
        static void MoveSnake()
        {
            if (myDir == Direction.Left)
            {
                if (head.X == 0)
                    head = new Point(76, head.Y);
                else
                    head = new Point(head.X - 1, head.Y);
            }
            else if (myDir == Direction.Right)
            {
                if (head.X == 76)
                    head = new Point(0, head.Y);
                else
                    head = new Point(head.X + 1, head.Y);
            }
            else if (myDir == Direction.Up)
            {
                if (head.Y == 0)
                    head = new Point(head.X, 22);
                else
                    head = new Point(head.X, head.Y - 1);
            }
            else if (myDir == Direction.Down)
            {
                if (head.Y == 22)
                    head = new Point(head.X, 0);
                else
                    head = new Point(head.X, head.Y + 1);
            }

            gameField[snake[snake.Count - 1].Y, snake[snake.Count - 1].X] = ' ';
            Console.SetCursorPosition(snake[snake.Count - 1].X + 1, snake[snake.Count - 1].Y + 1);
            Console.Write(' ');
            snake.RemoveAt(snake.Count - 1);
            if (gameField[head.Y, head.X] == 'X')
                GAMEOVER = true;
            else
                gameField[head.Y, head.X] = 'O';
            Console.SetCursorPosition(head.X + 1, head.Y + 1);
            Console.Write('O');
            snake.Insert(0, head);
        }
        /// <summary>
        /// Процесс руху змійки-суперника(method of class Snakebot : Snake)
        /// </summary>
        static void MoveSnakebot()
        {
            if (botDir == Direction.Left)
            {
                if (clone_head.X == 0)
                    clone_head = new Point(76, clone_head.Y);
                else
                    clone_head = new Point(clone_head.X - 1, clone_head.Y);
            }
            else if (botDir == Direction.Right)
            {
                if (clone_head.X == 76)
                    clone_head = new Point(0, clone_head.Y);
                else
                    clone_head = new Point(clone_head.X + 1, clone_head.Y);
            }
            else if (botDir == Direction.Up)
            {
                if (clone_head.Y == 0)
                    clone_head = new Point(clone_head.X, 22);
                else
                    clone_head = new Point(clone_head.X, clone_head.Y - 1);
            }
            else if (botDir == Direction.Down)
            {
                if (clone_head.Y == 22)
                    clone_head = new Point(clone_head.X, 0);
                else
                    clone_head = new Point(clone_head.X, clone_head.Y + 1);
            }

            gameField[cloneSnake[cloneSnake.Count - 1].Y, cloneSnake[cloneSnake.Count - 1].X] = ' ';
            Console.SetCursorPosition(cloneSnake[cloneSnake.Count - 1].X + 1, cloneSnake[cloneSnake.Count - 1].Y + 1);
            Console.Write(' ');
            cloneSnake.RemoveAt(cloneSnake.Count - 1);
            if (gameField[clone_head.Y, clone_head.X] == 'X')
                GAMEOVER = true;
            else
                gameField[clone_head.Y, clone_head.X] = '*';
            Console.SetCursorPosition(clone_head.X + 1, clone_head.Y + 1);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write('*');
            Console.ForegroundColor = ConsoleColor.White;
            cloneSnake.Insert(0, clone_head);
        }
        /// <summary>
        /// Процесс зміни напрямку змійки-суперника(method of Snakebot : Snake)
        /// </summary>
        static void DirectSnakebot()
        {
            if (FOODEXIST)
            {
                if (clone_head.X > meal.X && clone_head.Y > meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Left;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Left;
                    }
                }
                else if (clone_head.X > meal.X && clone_head.Y < meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Left;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Left;
                    }
                }
                else if (clone_head.X < meal.X && clone_head.Y > meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Right;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Right;
                    }
                }
                else if (clone_head.X < meal.X && clone_head.Y < meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Right;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Right;
                    }
                }
                else if (clone_head.X == meal.X && clone_head.Y > meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Up)
                    {

                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Left;
                    }
                }
                else if (clone_head.X > meal.X && clone_head.Y == meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Left)
                    {

                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Left;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Left;
                    }
                }
                else if (clone_head.X == meal.X && clone_head.Y < meal.Y)
                {
                    if (botDir == Direction.Right)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Down;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Left;
                    }
                    else if (botDir == Direction.Down)
                    {

                    }
                }
                else if (clone_head.X < meal.X && clone_head.Y == meal.Y)
                {
                    if (botDir == Direction.Right)
                    {

                    }
                    else if (botDir == Direction.Left)
                    {
                        botDir = Direction.Up;
                    }
                    else if (botDir == Direction.Up)
                    {
                        botDir = Direction.Right;
                    }
                    else if (botDir == Direction.Down)
                    {
                        botDir = Direction.Right;
                    }
                }
            }
        }
        /// <summary>
        /// Обчислення довжини змійки
        /// </summary>
        /// <returns>кількість сегментів змійки</returns>
        static int SnakeLength()
        {
            return snake.Count;
        }
        /// <summary>
        /// Генератор первинного ігрового поля(method of Walls)
        /// </summary>
        static void DrawField()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(" --------------------");
            Console.Write("--------------------");
            Console.Write("--------------------");
            Console.Write("-----------------");
            Console.SetCursorPosition(0, height);
            Console.Write(" --------------------");
            Console.Write("--------------------");
            Console.Write("--------------------");
            Console.Write("-----------------");
            for (int i = 1; i < height; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write('|');
                Console.SetCursorPosition(width, i);
                Console.Write('|');
            }
        }
        /// <summary>
        /// Вираовує процес поїдання їжі змійкою(method of Snake)
        /// </summary>
        static void EatFood()
        {
            if (!(meal.X == snake[snake.Count - 1].X && meal.Y == snake[snake.Count - 1].Y))
                IEAT = true;
            else
            {
                IEAT = false;
                FOODEXIST = false;
                Thread.Sleep(speed);
                //добавляем в качестве головы ещё 1 секцию
                MoveSnake();
                snake.Add(meal);
                Console.SetCursorPosition(meal.X + 1, meal.Y + 1);
                Console.Write('O');
            }
        }
        /// <summary>
        /// Вираховує процес поїдання їжі суперником(method of class Snakebot : Snake)
        /// </summary>
        static void EatFoodSnakebot()
        {
            if (!(meal.X == cloneSnake[cloneSnake.Count - 1].X && meal.Y == cloneSnake[cloneSnake.Count - 1].Y))
                BOTEAT = true;
            else
            {
                BOTEAT = false;
                FOODEXIST = false;
                Thread.Sleep(speed);
                //добавляем в качестве головы ещё 1 секцию
                MoveSnakebot();
                cloneSnake.Add(meal);
                Console.SetCursorPosition(meal.X + 1, meal.Y + 1);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write('*');
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        /// <summary>
        /// Описує хід гри режиму виживання(method of class Engine)
        /// </summary>
        static void ControlSurvivalGame()
        {
            while (backgrnd.IsAlive)
            {
                if (GAMEOVER)
                    break;
                else if (backgrnd.IsAlive)
                {
                    keyInfo = Console.ReadKey(true);

                    if (GAMEOVER)
                        break;
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        if (myDir != Direction.Left)
                        {
                            if (snake[1].X == (head.X - 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Right;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (myDir != Direction.Down)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y + 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Up;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (myDir != Direction.Up)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y - 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Down;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.P)
                        PAUSE = true;
                    else if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        if (myDir != Direction.Right)
                        {
                            if (snake[1].X == (head.X + 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Left;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Описує хід гри режиму аркади(method of class Engine)
        /// </summary>
        static void ControlArcadeGame()
        {
            while (backgroundArcade.IsAlive)
            {
                if (GAMEOVER)
                    break;
                else if (backgroundArcade.IsAlive)
                {
                    keyInfo = Console.ReadKey(true);

                    if (GAMEOVER)
                        break;
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        if (myDir != Direction.Left)
                        {
                            if (snake[1].X == (head.X + 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Right;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (myDir != Direction.Down)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y - 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Up;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (myDir != Direction.Up)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y + 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Down;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.P)
                        PAUSE = true;
                    else if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        if (myDir != Direction.Right)
                        {
                            if (snake[1].X == (head.X - 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Left;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Описує хід гри режиму протистояння( method of class Engine)
        /// </summary>
        static void ControlChallengeGame()
        {
            while (backgroundClone.IsAlive)
            {
                if (GAMEOVER)
                    break;
                else if (backgroundClone.IsAlive)
                {
                    keyInfo = Console.ReadKey(true);

                    if (GAMEOVER)
                        break;
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        if (myDir != Direction.Left)
                        {
                            if (snake[1].X == (head.X - 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Right;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (myDir != Direction.Down)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y + 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Up;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (myDir != Direction.Up)
                        {
                            if (snake[1].X == head.X && snake[1].Y == (head.Y - 1))
                                Thread.Sleep(speed);
                            myDir = Direction.Down;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.P)
                        PAUSE = true;
                    else if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        if (myDir != Direction.Right)
                        {
                            if (snake[1].X == (head.X + 1) && snake[1].Y == head.Y)
                                Thread.Sleep(speed);
                            myDir = Direction.Left;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Виконавчий метод потоку backgrnd(method of class Engine)
        /// </summary>
        static void StreamSurvivalGame()
        {
            while (!GAMEOVER)
            {
                if (PAUSE)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("GAME PAUSED");

                    keyInfo = Console.ReadKey(true);
                    while (keyInfo.Key != ConsoleKey.P)
                    {
                        keyInfo = Console.ReadKey(true);
                    }

                    PAUSE = false;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("            ");
                }

                for (int i = 1; i < snake.Count; i++)
                {
                    if (head.X == snake[i].X && head.Y == snake[i].Y)
                        GAMEOVER = true;
                }
                if (GAMEOVER)
                    break;
                if (FOODEXIST == false)
                    CreateFood();
                MoveSnake();
                if (head.X == meal.X && head.Y == meal.Y)
                {
                    IEAT = true;
                    score += Convert.ToInt64((Convert.ToDouble(snake.Count) / 3) * 10);
                    Console.SetCursorPosition(63, 0);
                    Console.Write(score);
                }
                if (IEAT)
                {
                    EatFood();
                }
                Thread.Sleep(speed);
            }
        }
        /// <summary>
        /// Виконавчий метод потоку backgroundArcage(method of class Engine)
        /// </summary>
        static void StreamArcadeGame()
        {
            while (!GAMEOVER)
            {
                if (PAUSE)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("GAME PAUSED");

                    keyInfo = Console.ReadKey(true);
                    while (keyInfo.Key != ConsoleKey.P)
                    {
                        keyInfo = Console.ReadKey(true);
                    }

                    PAUSE = false;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("            ");
                }

                for (int i = 1; i < snake.Count; i++)
                {
                    if (head.X == snake[i].X && head.Y == snake[i].Y)
                        GAMEOVER = true;
                }
                if (GAMEOVER)
                    break;
                if (FOODEXIST == false)
                    CreateFood();
                MoveSnake();
                if (head.X == meal.X && head.Y == meal.Y)
                {
                    IEAT = true;
                    score += Convert.ToInt64((Convert.ToDouble(snake.Count) / 3) * 10);
                    Console.SetCursorPosition(63, 0);
                    Console.Write(score);

                    if (score >= 200 && level < 2)
                    {
                        level = 2;
                        Console.Clear();

                        InitiateSnake();
                        DrawField();

                        Console.SetCursorPosition(56, 0);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("SCORE: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(63, 0);
                        Console.Write(score);

                        DrawLevel2();
                        FOODEXIST = false;
                    }
                    else if (score >= 700 && level < 3)
                    {
                        level = 3;
                        Console.Clear();

                        InitiateSnake();
                        DrawField();

                        Console.SetCursorPosition(56, 0);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("SCORE: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(63, 0);
                        Console.Write(score);

                        Drawlevel3();
                        FOODEXIST = false;
                    }
                    else if (score >= 1500 && level < 4)
                    {
                        level = 4;
                        Console.Clear();

                        InitiateSnake();
                        DrawField();

                        Console.SetCursorPosition(56, 0);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("SCORE: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(63, 0);
                        Console.Write(score);

                        Drawlevel4();
                        FOODEXIST = false;
                    }
                }
                if (IEAT)
                {
                    EatFood();
                }
                Thread.Sleep(speed);
            }
        }
        /// <summary>
        /// Виконавчий метод потоку backgroundClone(method of class Engine)
        /// </summary>
        static void StreamChallengeGame()
        {
            while (!GAMEOVER)
            {
                if (PAUSE)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("[GAME PAUSED!]");

                    keyInfo = Console.ReadKey(true);
                    while (keyInfo.Key != ConsoleKey.P)
                    {
                        keyInfo = Console.ReadKey(true);
                    }

                    PAUSE = false;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(30, 12);
                    Console.Write("                 ");
                }

                for (int i = 1; i < snake.Count; i++) //самосьедание
                {
                    if (head.X == snake[i].X && head.Y == snake[i].Y)
                        GAMEOVER = true;
                }

                for (int i = 0; i < snake.Count; i++) //сьедание клоном
                {
                    if (snake[i].X == clone_head.X && snake[i].Y == clone_head.Y)
                        GAMEOVER = true;
                }

                for (int i = 0; i < cloneSnake.Count; i++) //сьедание клона
                {
                    if (cloneSnake[i].X == head.X && cloneSnake[i].Y == head.Y)
                        GAMEOVER = true;
                }

                if (GAMEOVER)
                    break;
                if (FOODEXIST == false)
                    CreateFood();

                MoveSnake();
                MoveSnakebot();
                DirectSnakebot();

                if (head.X == meal.X && head.Y == meal.Y)
                {
                    IEAT = true;
                    score += Convert.ToInt64((Convert.ToDouble(snake.Count) / 3) * 10);
                    Console.SetCursorPosition(63, 0);
                    Console.Write(score);
                }
                else if (clone_head.X == meal.X && clone_head.Y == meal.Y)
                    BOTEAT = true;

                if (IEAT)
                {
                    EatFood();
                }
                else if (BOTEAT)
                {
                    EatFoodSnakebot();
                }

                Thread.Sleep(speed);
            }
        }
        /// <summary>
        /// Ініціалізує хід гри режиму виживання(method of class Engine)
        /// </summary>
        static void SetSurvivalMode()
        {
            do
            {
                InitiateSnake();
                DrawField();
                select = randomize.Next(1, 5);

                if (select == 1)
                {
                    Console.SetCursorPosition(3, 0);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("LEVEL 1");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (select == 2)
                    DrawLevel2();
                else if (select == 3)
                    Drawlevel3();
                else if (select == 4)
                    Drawlevel4();

                Console.SetCursorPosition(56, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("SCORE: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(63, 0);
                Console.Write(score);
                
                backgrnd.Start();
                backgrnd.IsBackground = true;

                ControlSurvivalGame();

                Console.Clear();
                Graphics.ShowEnd(score, SnakeLength());

            } while (RepeatGame(GAMEOVER, 1));
            Environment.Exit(0);
        }
        /// <summary>
        /// Ініціалізує хід гри режиму аркади(method of class Engine)
        /// </summary>
        static void SetArcadeMode()
        {
            do
            {
                InitiateSnake();
                DrawField();

                Console.SetCursorPosition(3, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("LEVEL 1");
                Console.ForegroundColor = ConsoleColor.White;

                Console.SetCursorPosition(56, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("SCORE: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(63, 0);
                Console.Write(score);

                backgroundArcade.Start();
                backgroundArcade.IsBackground = true;

                ControlArcadeGame();

                Console.Clear();
                Graphics.ShowEnd(score, SnakeLength());

            } while (RepeatGame(GAMEOVER, 2));
            Environment.Exit(0);
        }
        /// <summary>
        /// Ініціалізує хід гри режиму протистояння(method of class Engine)
        /// </summary>
        static void SetChallenge()
        {
            do
            {
                InitiateSnake();
                InitiateSnakebot();
                DrawField();

                Console.SetCursorPosition(3, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("LEVEL 1");
                Console.ForegroundColor = ConsoleColor.White;

                Console.SetCursorPosition(56, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("SCORE: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(63, 0);
                Console.Write(score);

                backgroundClone.Start();
                backgroundClone.IsBackground = true;
                
                ControlChallengeGame();

                Console.Clear();
                Graphics.ShowEnd(score,SnakeLength());

            } while (RepeatGame(GAMEOVER, 3));
            Environment.Exit(0); 
        }
        /// <summary>
        /// Малює ігрове поле для рівня 2(method of Walls)
        /// </summary>
        static void DrawLevel2()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(1, 1);
            for (int i = 1; i < width; i++)
            {
                Console.Write('X');
                gameField[0, i] = 'X';
            }
            Console.SetCursorPosition(1, height);
            for (int i = 1; i < width; i++)
            {
                Console.Write('X');
                gameField[23, i] = 'X';
            }
            for (int i = 2; i < height-1; i++)
            {
                Console.SetCursorPosition(1, i);
                Console.Write('X');
                gameField[i - 1, 0] = 'X';
                Console.SetCursorPosition(width-1, i);
                Console.Write('X');
                gameField[i - 1, width-1] = 'X';
            }

            Console.SetCursorPosition(3, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("LEVEL 2");

            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Малює ігрове поле для рівня 3(method of Walls)
        /// </summary>
        static void Drawlevel3()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(1, 1);
            for (int i = 0; i < width-1; i++)
            {
                Console.Write('X');
                gameField[0, i] = 'X';
            }
            Console.SetCursorPosition(1, 23);
            for (int i = 0; i < width-1; i++)
            {
                Console.Write('X');
                gameField[22, i] = 'X';
            }

            for (int i = 7; i < 18; i++)
            {
                Console.SetCursorPosition(5, i);
                for (int j = 5; j < 27; j++)
                {
                    gameField[i - 1, j - 1] = 'X';
                    Console.Write('X');
                }
            }

            for (int i = 4; i < 9; i++)
            {
                Console.SetCursorPosition(45, i);
                for (int j = 45; j < 60; j++)
                {
                    gameField[i - 1, j - 1] = 'X';
                    Console.Write('X');
                }
            }

            for (int i = 16; i < 22; i++)
            {
                Console.SetCursorPosition(49, i);
                for (int j = 49; j < 64; j++)
                {
                    gameField[i - 1, j - 1] = 'X';
                    Console.Write('X');
                }
            }

            Console.SetCursorPosition(3, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("LEVEL 3");

            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Малює ігрове поле для рівня 4(method of Walls)
        /// </summary>
        static void Drawlevel4()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(1, 1);
            for (int i = 0; i < width-1; i++)
            {
                Console.Write('X');
                gameField[0, i] = 'X';
            }
            Console.SetCursorPosition(1, 23);
            for (int i = 0; i < width-1; i++)
            {
                Console.Write('X');
                gameField[22, i] = 'X';
            }

            for (int i = 2; i < height-10; i++)
            {
                Console.SetCursorPosition(4, i);
                for (int j = 4; j < 9; j++)
                {
                    gameField[i - 1, j - 1] = 'X';
                    Console.Write('X');
                }
            }

            for (int i = 11; i < height-1; i++)
            {
                Console.SetCursorPosition(13, i);
                for (int j = 13; j < 18; j++)
                {
                    gameField[i - 1, j - 1] = 'X';
                    Console.Write('X');
                }
            }

            for (int i = 6; i < 16; i++)
            {
                Console.SetCursorPosition(45, i);
                gameField[i - 1, 44] = 'X';
                Console.Write('X');
            }

            Console.SetCursorPosition(47, 4);
            Console.Write("XXXXXXXXXXXXXXXXXXX");
            for (int i = 0; i < 19; i++)
            {
                gameField[3, 46 + i] = 'X';
            }

            Console.SetCursorPosition(47, 17);
            Console.Write("XXXXXXXXXXXXXXXXXXX");
            for (int i = 0; i < 19; i++)
            {
                gameField[16, 46 + i] = 'X';
            }

            Console.SetCursorPosition(17, 8);
            Console.Write("XXXXXXXXXXXXXXXXXXX");
            for (int i = 0; i < 19; i++)
            {
                gameField[7, 16 + i] = 'X';
            }

            for (int i = 2; i < 8; i++)
            {
                Console.SetCursorPosition(35, i);
                gameField[i - 1, 34] = 'X';
                Console.Write('X');
            }

            Console.SetCursorPosition(3, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("LEVEL 4");

            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Инициирует повторный запуск игры
        /// </summary>
        /// <param name="isGameover">статус игры</param>
        /// <param name="stream">номер потока</param>
        /// <returns></returns>
        static bool RepeatGame(bool isGameover, int stream)
        {
            if (isGameover)
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.N)
                    return false;
                else
                {
                    switch (stream)
                    {
                        case 1:
                            if (backgrnd.IsAlive)
                            {
                                backgrnd.Abort();
                                Console.Clear();
                            }     
                            backgrnd = new Thread(StreamSurvivalGame);
                            GAMEOVER = false;
                            score = 0;
                            break;
                        case 2:
                            if (backgroundArcade.IsAlive)
                            {
                                backgroundArcade.Abort();
                                Console.Clear();
                            }
                            backgroundArcade = new Thread(StreamArcadeGame);
                            GAMEOVER = false;
                            score = 0;
                            break;
                        case 3:
                            if (backgroundClone.IsAlive)
                            {
                                backgroundClone.Abort();
                                Console.Clear();
                            }
                            backgroundClone = new Thread(StreamChallengeGame);
                            GAMEOVER = false;
                            score = 0;
                            break;
                        default:
                            throw new NotImplementedException("-Wrong input parameter!");
                      } 
                }
            }
            Console.Clear();
            return true;
        }
    }
}