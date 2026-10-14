using AnimalLabOnline.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnimalLabOnline.Simulation
{
    public class Simulation
    {
        private readonly IConfiguration _config;

        public Simulation(IConfiguration config)
        {
            _config = config;
        }

        static Dictionary<string, double[]> Organs { get; } = new Dictionary<string, double[]>
        {
            // ep0: in, ep1: out, ep2: in, ep3: out
            //idx { "name",      new[] { x,    y,  scale, /*0*/ epx, epy, cpx, cpy, /*1*/ epx, epy, cpx, cpy } },
            /*0*/ { "brain",     new[] { 0.50, 0.05, 0.8, /*0*/ 0.88, 0.40, 0.80, 0.00, /*1*/ 0.10, 0.40, 0.80, 0.00 } },
            /*1*/ { "lung",      new[] { 0.50, 0.17, 1.2, /*0*/ 0.80, 0.88, 0.00, 0.40, /*1*/ 0.60, 0.80, 0.00,-0.40 } },
            /*2*/ { "_rlung",    new[] { 0.50, 0.17, 1.2, /*2*/ 0.20, 0.88, 0.00, 0.40, /*3*/ 0.40, 0.80, 0.00,-0.40 } },
            /*3*/ { "heart",     new[] { 0.50, 0.38, 0.9, /*0*/ 0.38, 0.10, 0.30, 0.40, /*1*/ 0.50, 0.05, 0.00,-0.30 } },
            /*4*/ { "_rheart",   new[] { 0.50, 0.38, 0.9, /*0*/ 0.22, 0.14,-0.90,-0.60, /*1*/ 0.70, 0.22,-0.40, 0.20, /*2*/ 0.35, 0.90,-0.10, 0.40 } },
            /*5*/ { "liver",     new[] { 0.48, 0.55, 1.0, /*0*/ 0.50, 0.60, 0.00, 0.20, /*1*/ 0.40, 0.25, 0.00, 0.10, /*2*/ 0.75, 0.52, 0.30, 0.20 } },
            /*6*/ { "stomach",   new[] { 0.65, 0.63, 1.0, /*0*/ 0.70, 0.35, 0.10,-0.40, /*1*/ 0.60, 0.82, 0.10,-0.50, /*2*/ 0.37, 0.06, 0.00,-0.20 } },
            /*7*/ { "kidney",    new[] { 0.15, 0.65, 1.0, /*0*/ 0.61, 0.52,-0.40, 0.30, /*1*/ 0.60, 0.41, 0.20, 0.20 } },
            /*8*/ { "_rkidney",  new[] { 0.15, 0.65, 1.0, /*0*/ 0.35, 0.53, 0.40, 0.40, /*1*/ 0.39, 0.45,-0.10, 0.30 } },
            /*9*/ { "spleen",    new[] { 0.85, 0.70, 0.5, /*0*/ 0.05, 0.65,-0.40, 0.20, /*1*/ 0.12, 0.75, 1.50,-1.50 } },
            /*10*/{ "intestine", new[] { 0.55, 0.89, 1.2, /*0*/ 0.85, 0.40, 0.20, 0.00, /*1*/ 0.40, 0.20, 0.00, 0.30 } },
            /*11*/{ "_aorta",    new[] { 0.70, 0.35, 0.5, /*0*/ 0.50, 0.50,-0.30,-0.50, /*1*/ 0.50, 0.50,-0.50, 0.50 } },
            /*12*/{ "_celiac",   new[] { 0.85, 0.48, 0.5, /*0*/ 0.50, 0.50,-0.30,-0.50, /*1*/ 0.50, 0.50, 0.50,-0.40 } },
            /*13*/{ "_renalis",  new[] { 0.95, 0.80, 0.5, /*0*/ 0.50, 0.50, 0.40,-0.70, /*1*/ 0.50, 0.50, 0.50,-0.20 } },
            /*14*/{ "_portal",   new[] { 0.50, 0.66, 0.5, /*0*/ 0.50, 0.50, 0.10, 0.40, /*1*/ 0.50, 0.50,-0.30,-0.20 } },
        };

        public static string[] OrganNames { get; } = Organs.Keys.ToArray();

        public static Dictionary<string, int[]> Selectors { get; } = new Dictionary<string, int[]>
        {
            { "Stomach", new[] { 6, 2 } },
            { "Portal Vein", new[] { 14, 1 } },
            { "Brain", new[] { 0, 0 } },
        };

        static int[][] Connections { get; } = new int[][]
        {
            // [0 source, 1 destination, 2 source connector, 3 destination connector, 4 blood type(0:red, 1:blue, 2:purple)]
            // red blood
            new[] { 1, 3, 1, 1, 0 }, // lung-heart
            new[] { 2, 3, 1, 1, 0 }, // lung-heart
            new[] { 3,11, 0, 0, 0 }, // heart-aorta
            new[] {11, 0, 1, 0, 0 }, // aorta-brain
            new[] {11,12, 0, 0, 0 }, // aorta-celiac
            new[] {12, 5, 1, 2, 0 }, // celiac-liver
            new[] {12, 6, 1, 0, 0 }, // celiac-stomach
            new[] {12, 9, 1, 0, 0 }, // celiac-spleen
            new[] {12,13, 0, 0, 0 }, // celiac-renalis
            new[] {13, 7, 1, 0, 0 }, // renalis-kidney
            new[] {13, 8, 1, 0, 0 }, // renalis-kidney
            new[] {13,10, 0, 0, 0 }, // renalis-intestine
            // blue blood
            new[] { 0, 4, 1, 0, 1 }, // brain-heart
            new[] { 5, 4, 1, 2, 1 }, // liver-heart
            new[] { 7, 4, 1, 2, 1 }, // kidney-heart
            new[] { 8, 4, 1, 2, 1 }, // kidney-heart
            new[] { 4, 1, 1, 0, 1 }, // heart-lung
            new[] { 4, 2, 1, 0, 1 }, // heart-lung
            // purple blood
            new[] { 6, 14, 1, 0, 2 }, // stomach-portal
            new[] { 9, 14, 1, 0, 2 }, // spleen-portal
            new[] {10, 14, 1, 0, 2 }, // intestine-portal
            new[] {14, 5, 0, 0, 2 }, // portal-liver
        };

        IEnumerable<double[]> ToCanvasCoordinates(int canvasWidth, int canvasHeight, Dictionary<string, double[]> dict)
        {
            int organWidth = _config.GetValue<int>("SimulationOptions:OrganWidth");
            int organHeight = _config.GetValue<int>("SimulationOptions:OrganHeight");

            foreach (var item in dict)
            {
                double x = item.Value[0] * canvasWidth, y = item.Value[1] * canvasHeight, scale = item.Value[2];
                var values = new double[item.Value.Length + 1];

                values[0] = x;
                values[1] = y;
                values[2] = organWidth * scale;
                values[3] = organHeight * scale;

                for (int i = 3; i + 4 <= item.Value.Length; i += 4)
                {
                    values[i + 1] = x + (item.Value[i + 0] - 0.5) * organWidth * scale;
                    values[i + 2] = y + (item.Value[i + 1] - 0.5) * organHeight * scale;
                    values[i + 3] = x + (item.Value[i + 0] + item.Value[i + 2] - 0.5) * organWidth * scale;
                    values[i + 4] = y + (item.Value[i + 1] + item.Value[i + 3] - 0.5) * organHeight * scale;
                }

                yield return values;
            }
        }

        double Lerp(double v1, double v2, double pos)
        {
            return v1 * (1 - pos) + v2 * pos;
        }

        double Length(double epx1, double epy1, double epx2, double epy2, double cpx1, double cpy1, double cpx2, double cpy2)
        {
            double res = 0;
            double lastx = epx1, lasty = epy1;
            for (double i = 0; i <= 1; i += 0.01)
            {
                var x = Lerp(Lerp(Lerp(epx1, cpx1, i), Lerp(cpx1, cpx2, i), i), Lerp(Lerp(cpx1, cpx2, i), Lerp(cpx2, epx2, i), i), i);
                var y = Lerp(Lerp(Lerp(epy1, cpy1, i), Lerp(cpy1, cpy2, i), i), Lerp(Lerp(cpy1, cpy2, i), Lerp(cpy2, epy2, i), i), i);
                res += Math.Sqrt((x - lastx) * (x - lastx) + (y - lasty) * (y - lasty));
                lastx = x;
                lasty = y;
            }
            return res;
        }

        /// <summary>
        /// Get an array of organs in the shape of:
        /// [0: organ center x, 1: organ center y, 2: organ width, 3: organ height,
        ///  4 + n + 0: x coordinate of connector n, 4 + n + 1: y coordinate of connector n,
        ///  4 + n + 2: control point x of connector n, 4 + n + 3: control point y of connector n][]
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public double[][] GetCanvasOrgans(int width, int height)
        {
            return ToCanvasCoordinates(width, height, Organs).ToArray();
        }

        /// <summary>
        /// Get connections between organs in the shape of:
        /// [0 source node id, 1 destination node id, 2 source connector id, 3 destination connector id,
        ///  4 blood type(0:red, 1:blue, 2:purple), 5 connection length][]
        /// </summary>
        /// <returns></returns>
        public int[][] GetOrganConnections(double[][] canvasOrgans)
        {
            int dist = _config.GetValue<int>("SimulationOptions:HemocyteDistance");

            return Connections.Select(o => o.Append((int)(GetBezierLength(canvasOrgans, o) / dist)).ToArray()).ToArray();
        }

        double GetBezierLength(double[][] organs, int[] arr)
        {
            int b = 4;
            double epx1 = organs[arr[0]][b + arr[2] * 4 + 0], epy1 = organs[arr[0]][b + arr[2] * 4 + 1];
            double cpx1 = organs[arr[0]][b + arr[2] * 4 + 2], cpy1 = organs[arr[0]][b + arr[2] * 4 + 3];
            cpx1 = epx1 + epx1 - cpx1; cpy1 = epy1 + epy1 - cpy1;
            double epx2 = organs[arr[1]][b + arr[3] * 4 + 0], epy2 = organs[arr[1]][b + arr[3] * 4 + 1];
            double cpx2 = organs[arr[1]][b + arr[3] * 4 + 2], cpy2 = organs[arr[1]][b + arr[3] * 4 + 3];
            return Length(epx1, epy1, epx2, epy2, cpx1, cpy1, cpx2, cpy2);
        }

        public SimulationViewModelBuilder GetSimulationViewModelBuilder()
        {
            return new SimulationViewModelBuilder(this);
        }
    }
}
