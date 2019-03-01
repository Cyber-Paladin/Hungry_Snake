using System;
using System.Threading;

namespace SnakeSimulator
{
    static class Graphics
    {
        /// <summary>
        /// Відображує стартовий екран
        /// </summary>
        public static void ShowLogo()
        {
            PrintString(20, 3, ConsoleColor.Green, "< S N A K E  ^  W O R L D");
            PrintString(18, 5, ConsoleColor.Green, "C O N S O L E  |  S I M U L A T O R >");
            PrintString(20, 9, ConsoleColor.White, "Choose one of the game option:\n");
            PrintString(23, 10, ConsoleColor.White, ">>Press 1 for survival mode.");
            PrintString(23, 11, ConsoleColor.White, ">>Press 2 for arcade mode.");
            PrintString(23, 12, ConsoleColor.White, ">>Press 3 for challenge mode versus bot.");
            PrintString(23, 13, ConsoleColor.White, ">>Press <Ecs> to exit.");
            PrintString(7, 24, ConsoleColor.Yellow, "v 2.0.NET                     Yatsyn Mark, 2018");
        }

        /// <summary>
        /// Підтримує еффект мигання тексту
        /// </summary>
        public static void Blink( int variant )
        {
            switch (variant)
            {
                case 1:
                    {
                        PrintString(23, 10, ConsoleColor.Red, ">>Press 1 for survival mode.");
                        Thread.Sleep(75);
                        PrintString(23, 10, ConsoleColor.White, "                                   ");
                        Thread.Sleep(75);
                        break;
                    }
                case 2:
                    {
                        PrintString(23, 11, ConsoleColor.Red, ">>Press 2 for arcade mode.");
                        Thread.Sleep(75);
                        PrintString(23, 11, ConsoleColor.White, "                                       ");
                        Thread.Sleep(75);
                        break;
                    }
                case 3:
                    {
                        PrintString(23, 12, ConsoleColor.Red, ">>Press 3 for challenge mode versus bot.");
                        Thread.Sleep(75);
                        PrintString(23, 12, ConsoleColor.White, "                                              ");
                        Thread.Sleep(75);
                        break;
                    }
                default:
                    {
                        PrintString(23, 13, ConsoleColor.Red, ">>Invalid input! Try again!");
                        Thread.Sleep(75);
                        PrintString(23, 13, ConsoleColor.White, "                                     ");
                        Thread.Sleep(75);
                        break;
                    }
            }
        }

        /// <summary>
        /// Відображення фінальної ігрової статистики
        /// </summary>
        /// <param name="time">час гри</param>
        /// <param name="score">рахунок</param>
        /// <param name="length">довжина змійки</param>
        public static void ShowEnd(long score, int length, int[] period)
        {
            PrintString(25, 7, ConsoleColor.Red, "G A M E   O V E R !!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(30, 10);
            Console.Write("YOUR SCORE: ");
            Console.Write(score);
            Console.SetCursorPosition(30, 12);
            Console.Write("YOUR TIME: ");
            Console.Write($"{period[2]} sec.");
            Console.SetCursorPosition(30, 14);
            Console.Write("SNAKE LENGTH: ");
            Console.Write(length);
            PrintString(23, 17, ConsoleColor.DarkRed, "R E P E A T   G A M E ??");
            PrintString(18, 18, ConsoleColor.DarkGray, "press any key to repeat or Esc to exit");
        }

        /// <summary>
        /// Форматна печать тексту
        /// </summary>
        /// <param name="xpos">Х</param>
        /// <param name="ypos">У</param>
        /// <param name="fcolor">колір тексту</param>
        /// <param name="str">власне текст</param>
        public static void PrintString(int xpos, int ypos, ConsoleColor fcolor, string str)
        {
            Point point = new Point(xpos, ypos, str);
            Console.SetCursorPosition(point.X, point.Y);
            Console.ForegroundColor = fcolor;
            Console.Write(str);
        }
    }
}