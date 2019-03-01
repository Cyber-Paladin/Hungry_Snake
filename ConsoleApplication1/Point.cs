namespace SnakeSimulator
{
    /// <summary>
    /// Структура яка формує координату точки на консолі
    /// </summary>
    struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Printer { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
            Printer = string.Empty;
        }

        public Point(int x, int y, string str)
        {
            X = x;
            Y = y;
            Printer = str;
        }
    }
}