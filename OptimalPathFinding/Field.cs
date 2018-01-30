using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Drawing;

namespace OptimalPathFinding
{
    public enum GroundType
    {
        wall = -1,
        ground = 0,
        door = 1,
        door_locked = 2
    }
    public struct GroundPlate
    {
        public static ConsoleColor[] groundColor = new ConsoleColor[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.White };
        static ConsoleColor wallColor = ConsoleColor.DarkGray;

        public Point point;
        public GroundType _type;
        int _light;
        string _wasLighted;
        public GroundType type { get { return _type; } set { _type = value; } }
        public int light { get { return _light; } set { _light = value; } }
        public string wasLighted { get { return _wasLighted; } set { _wasLighted = value; } }


        public bool LightBlocking()
        {
            return (type == GroundType.door_locked || type == GroundType.wall);
        }
        public bool PathBlocking()
        {
            return (type == GroundType.wall);
        }
        public char Symbol
        {
            get
            {
                switch (type)
                {
                    case GroundType.wall:
                        return 'X';
                    case GroundType.ground:
                        return ' ';
                    case GroundType.door_locked:
                        return '#';
                    case GroundType.door:
                        return 'П';
                    default:
                        return '?';
                }
            }
        }
        public GroundPlate(GroundType gt, int I, int J)
        {
            _type = gt;
            point = new Point(I, J);
            _light = 0;
            _wasLighted = "nn";
        }
        public void ConsoleWrite()
        {
            switch (type)
            {
                case GroundType.wall:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = wallColor;
                    break;
                case GroundType.ground:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = groundColor[0];
                    break;
                default:
                    break;
            }
            Console.Write(Symbol);
        }
    }
    public class Field
    {
        List<List<GroundPlate>> ground;
        List<Point> stack;
        List<Point> path;
        List<Point> everWas;

        void LightIn(GroundPlate gp)
        {
            Console.SetCursorPosition(gp.point.Y, gp.point.X);
            Console.BackgroundColor = GroundPlate.groundColor[gp.light];
            Console.Write((gp.type == GroundType.ground) ? ((gp.wasLighted[1] == 'y') ? ' ' : '.') : gp.Symbol);
        }
        static List<List<GroundPlate>> wasLight = new List<List<GroundPlate>>();
        void TraceLight()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < ground.Count; i++)
                for (int j = 0; j < ground[i].Count; j++)
                {
                    if (wasLight.Count == 0 || wasLight[i][j].light != ground[i][j].light)
                        LightIn(ground[i][j]);
                }

            wasLight = new List<List<GroundPlate>>();
            for (int i = 0; i < ground.Count; i++)
            {
                wasLight.Add(new List<GroundPlate>());
                for (int j = 0; j < ground[i].Count; j++)
                    wasLight[i].Add(ground[i][j]);
            }
        }

        void WriteIn(int x, int y, bool mark, ConsoleColor clr)
        {
            Console.SetCursorPosition(y, x);
            Console.ForegroundColor = clr;
            Console.Write((mark) ? "*" : " ");

        }
        public void Trace()
        {
            prevPath = new List<Point>();
            Console.ResetColor();
            Console.Clear();
            for (int i = 0; i < ground.Count; i++, Console.WriteLine())
                for (int j = 0; j < ground[i].Count; j++)
                    ground[i][j].ConsoleWrite();
        }
        static List<Point> prevPath = new List<Point>();
        void TracePath(List<Point> curPath, ConsoleColor clr)
        {
            for (int i = 0; i < prevPath.Count; i++)
                WriteIn(prevPath[i].X, prevPath[i].Y, false, clr);
            for (int i = 0; i < curPath.Count; i++)
                WriteIn(curPath[i].X, curPath[i].Y, true, clr);
            prevPath = curPath;
            Thread.Sleep(50);
        }

        public List<GroundPlate> Near(int x, int y, int rad)
        {
            List<GroundPlate> res = new List<GroundPlate>();
            for (int i = -rad + x; i <= rad + x; i++)
                for (int j = -rad + y; j <= rad + y; j++)
                    if (i >= 0 && i < ground.Count && j >= 0 && j < ground[i].Count && !(i == x && j == y))
                        res.Add(ground[i][j]);
            return res;
        }
        bool AnyWallsIn(List<GroundPlate> list)
        {
            foreach (GroundPlate gp in list)
                if (gp.type == GroundType.wall)
                    return true;
            return false;
        }
        int bestWay;
        static bool isOptimal = true;
        public List<Point> FindPath(Point from, Point to)
        {
            everWas = new List<Point>();
            stack = new List<Point>();
            bestWay = int.MaxValue;
            RecursiveFind(from, to, 0);
            return path;
        }
        void RecursiveFind(Point nowin, Point to, int currentFistance)
        {
            if (bestWay < int.MaxValue)
                return;
            everWas.Add(nowin);
            stack.Add(nowin);
            if (nowin.X == to.X && nowin.Y == to.Y)
            {
                if (currentFistance < bestWay)
                {
                    bestWay = currentFistance;
                    path = new List<Point>();
                    for (int i = 0; i < stack.Count; i++)
                        path.Add(stack[i]);
                    return;
                }
            }

            // near in 0,0 only !!! BUG
            List<GroundPlate> near = Near(nowin.X, nowin.Y, 1);
            if (isOptimal) near = SortByDist(near, to);
            for (int i = 0; i < near.Count; i++)
                if (stack.IndexOf(near[i].point) < 0 && everWas.IndexOf(near[i].point) < 0 && !near[i].PathBlocking())
                    RecursiveFind(near[i].point, to, currentFistance + 1);

            stack.RemoveAt(stack.Count - 1);
            return;
        }
        List<GroundPlate> SortByDist(List<GroundPlate> original, Point to)
        {
            List<int> dists = new List<int>();
            List<GroundPlate> res = new List<GroundPlate>();
            for (int i = 0; i < original.Count; i++)
                dists.Add(Math.Abs(original[i].point.X - to.X) + Math.Abs(original[i].point.Y - to.Y));
            for (int j = 0; j < dists.Count; j++)
            {
                int nowmax = dists[0], bestInd = 0;
                for (int i = 1; i < dists.Count; i++)
                    if (dists[i] < nowmax)
                    { nowmax = dists[i]; bestInd = i; }
                res.Add(original[bestInd]);
                dists[bestInd] = int.MaxValue;
            }
            return res;
        }

        void TurnOffLightProbe(bool AlsoTurnOff)
        {
            for (int i = 0; i < ground.Count; i++)
                for (int j = 0; j < ground[i].Count; j++)
                    if (ground[i][j].wasLighted[0] == 'y')
                    {
                        GroundPlate lgp = ground[i][j];
                        lgp.wasLighted = "n" + lgp.wasLighted.Substring(1);
                        if (AlsoTurnOff) lgp.light = 0;
                        ground[i][j] = lgp;
                    }
        }

        void LightIn(int x, int y, int rad)
        {
            TurnOffLightProbe(false);
            double step = Math.PI / (rad * 2);
            for (double ang = 0; ang < Math.PI * 2; ang += step)
            {
                bool mov = false;
                for (float dist = 0; dist <= rad; dist += .1f)
                {
                    int xl = (int)(Math.Cos(ang) * dist + x),
                        yl = (int)(Math.Sin(ang) * dist + y);

                    if (xl < 0 || yl < 0 || xl >= ground.Count || yl >= ground[xl].Count)
                        break;

                    GroundPlate lgp = ground[xl][yl];

                    if (lgp.LightBlocking())
                        break;

                    if (lgp.wasLighted[0] == 'n')
                    {
                        lgp.light = Math.Min(2, lgp.light + ((dist < rad / 2) ? 2 : 1));
                        lgp.wasLighted = "yy" + lgp.wasLighted.Substring(2);
                        ground[xl][yl] = lgp;

                    }

                }
                if (mov) ang += 3 * step;
            }
        }

        public Field(int size, Random seed, int percentNearWall, int percentDistractWall)
        {
            wasLight = new List<List<GroundPlate>>();
            stack = new List<Point>();
            path = new List<Point>();
            everWas = new List<Point>();
            ground = new List<List<GroundPlate>>();
            for (int i = 0; i < size; i++)
            {
                ground.Add(new List<GroundPlate>());
                for (int j = 0; j < size; j++)
                {
                    GroundType gt = GroundType.ground;
                    ground[i].Add(new GroundPlate(gt, i, j));
                }
            }
            int x = size / 2, y = size / 2;
            for (int i = 0; i < size * size / 4; i++)
            {
                bool common = seed.Next(101) < percentNearWall;

                while ((x >= 0 && y >= 0 && x < ground.Count && y < ground[x].Count) && ground[x][y].PathBlocking())
                { if (seed.Next(2) == 0) x += seed.Next(2) * 2 - 1; else y += seed.Next(2) * 2 - 1; }

                if (!common) { x = seed.Next(ground.Count - 2) + 1; y = seed.Next(ground[x].Count - 2) + 1; }

                if (common || seed.Next(101) < percentDistractWall)// || (seed.Next(101) < percentNearWall && AnyWallsIn(Near(x, y, 1))))
                    if (x >= 0 && y >= 0 && x < ground.Count && y < ground[x].Count)//try
                    {
                        GroundPlate gp = ground[x][y];
                        gp.type = (seed.Next(101) > 5) ? GroundType.wall : GroundType.door;
                        ground[x][y] = gp;
                    }
                //catch (Exception E) { }
            }

            Trace();
        }
        public void FindPathes()
        {
            int size = Math.Min(ground.Count, ground[0].Count);
            isOptimal = false;
            FindPath(new Point(size - 1, size - 1), new Point(0, 0));
            TracePath(path, ConsoleColor.DarkYellow);
            Console.SetCursorPosition(0, size + 2); Console.WriteLine("<<< " + path.Count + " >>>");

            isOptimal = true;
            FindPath(new Point(size - 1, size - 1), new Point(0, 0));
            prevPath = new List<Point>(); TracePath(path, ConsoleColor.Red);
            Console.SetCursorPosition(0, size + 1); Console.WriteLine("<<< " + path.Count + " >>>");
        }
        public void LightPath(int Rad)
        {
            //Trace();
            for (int i = 0; i < path.Count; i++)
            {
                TurnOffLightProbe(true);
                LightIn(path[i].X, path[i].Y, Rad);
                TraceLight();
            }
        }

        static Random rndReveal = new Random();
        Point SelectNearestUnrevealPoint(Point from)
        {
            int i = 0, x = 0, y = 0, tries = 0;
            while (tries < 1000 && !(from.X + x >= 0 && from.Y + y >= 0 && from.X + x < ground.Count && from.Y + y < ground[from.X + x].Count && ground[from.X + x][from.Y + y].wasLighted[1] == 'n' && ground[from.X + x][from.Y + y].type == GroundType.ground))
            {
                i++; tries++;
                x = rndReveal.Next(i / 5) * (rndReveal.Next(2) * 2 - 1);
                y = rndReveal.Next(i / 5) * (rndReveal.Next(2) * 2 - 1);
            }
            if (i < ground.Count * 5)
                return new Point(from.X + x, from.Y + y);
            return new Point(-1,-1);
        }

        public void ExploreAll(int Rad)
        {
            Point nowin, to;
            nowin = new Point(0, 0);
            wasLight = new List<List<GroundPlate>>();
            Trace();
            while (true)
            {
                to = SelectNearestUnrevealPoint(nowin);
                if (to.X < 0) break;
                WriteIn(to.X, to.Y, true, ConsoleColor.Red);
                FindPath(nowin, to);
                Thread.Sleep(100);
                LightPath(Rad);
                nowin = to;
            }
            Console.ResetColor();
            Console.WriteLine("!FINISH!");
        }
    }
}
