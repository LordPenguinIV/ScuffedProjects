using MapGenerator;
using System.Diagnostics;
using System.Linq;

int x = 1920;
int y = 1080;

var map = new Map(x, y).GetDisplayMap();

Stopwatch sw = Stopwatch.StartNew();
Renderer renderer = new Renderer(x, y);
renderer.Draw(map);
sw.Stop();
Debug.WriteLine($"render: {sw.ElapsedMilliseconds}");

renderer.Run();
