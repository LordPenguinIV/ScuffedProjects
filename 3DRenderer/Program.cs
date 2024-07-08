using System.Diagnostics;
using System.Numerics;
using Thingimajig;

int screenSize = 720;
List<Triangle> sceneTriangle = new List<Triangle>();

#region Scene Setup
Material light = new Material
{
    EmittedLight = 1,
    Color = new Vector3(255, 255, 128),
};
Triangle sunTriangle1 = new Triangle(
    new Vector3(10, 2, 10),
    new Vector3(-10, 2, 10),
    new Vector3(10, 2, -10),
    light
);
Triangle sunTriangle2 = new Triangle(
    new Vector3(-10, 2, -10),
    new Vector3(10, 2, -10),
    new Vector3(-10, 2, 10),
    light
);

Material ground = new Material
{
    EmittedLight = 0,
    Color = new Vector3(0, 200, 0),
};
Triangle groundTriangle1 = new Triangle(
    new Vector3(-5, -2, 5),
    new Vector3(5, -2, 5),
    new Vector3(5, -2, -5),
    ground
);
Triangle groundTriangle2 = new Triangle(
    new Vector3(5, -2, -5), 
    new Vector3(-5, -2, -5),
    new Vector3(-5, -2, 5),
    ground
);


Material brown = new Material
{
    EmittedLight = 0,
    Color = new Vector3(101, 67, 33),
};

Model monkey = new Model(@"C:\Users\GotPe\Downloads\Monkey.stl", brown, rotation: new Vector3(0, 90, 180));
monkey.BuildTriangles();

sceneTriangle.Add(groundTriangle1);
sceneTriangle.Add(groundTriangle2);
sceneTriangle.Add(sunTriangle1);
sceneTriangle.Add(sunTriangle2);
sceneTriangle.AddRange(monkey.Triangles);
#endregion

Camera cam = new Camera(screenSize, true, sceneTriangle)
{
    Position = new Vector3(0, 0, -4),
};

var t = new Stopwatch();
t.Start();
var v = cam.GetViewingPlane();
t.Stop();
Debug.WriteLine($"Render: {t.ElapsedMilliseconds}ms");

var p = new Renderer(screenSize, cam);
p.Draw(v);
p.Run();