using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalPathFinding
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(Console.LargestWindowWidth * 6 / 7, Console.LargestWindowHeight);
            Console.WriteLine("Size Distract_percent Common_percent");
            string param = Console.ReadLine();
            int siz = 50, pown = 5, pcom = 20;
            try {
                string[] pars = param.Split(' ');
                siz = int.Parse(pars[0]);
                pown = int.Parse(pars[1]);
                pcom = int.Parse(pars[2]);
            }
            catch (Exception E){  }
            while (true)
            {
                Random rnd = new Random();
                Field f = new Field(siz, rnd, pcom, pown);
                //f.FindPathes();
                //f.LightPath(20);
                f.ExploreAll(20);
                Console.ReadLine();
            }
        }
    }
}
