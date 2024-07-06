using System.Diagnostics;
using System.Numerics;
using Thingimajig;

int screenSize = 512;
List<Sphere> sceneSphere = new List<Sphere>();
List<Triangle> sceneTriangle = new List<Triangle>();

#region Scene Setup
Material red = new Material
{
    EmittedLight = 0,
    Color = new Vector3(255, 0, 0),
    Reflectivity = 0.5f
};
Sphere redSphere = new Sphere
{
    Material = red,
    Position = new Vector3(-0.5f, 0, 24),
    Radius = 1
};
Triangle redTriangle = new Triangle
{
    PositionA = new Vector3(0, -1, 24),
    PositionB = new Vector3(-0.5f, 1, 21f),
    PositionC = new Vector3(-1, -1, 24),
    Material = red,
};

Material blue = new Material
{
    EmittedLight = 0,
    Color = new Vector3(0, 0, 255),
    Reflectivity = 0.5f
};
Sphere blueSphere = new Sphere
{
    Material = blue,
    Position = new Vector3(1, 0, 25),
    Radius = 1
};
Triangle blueTriangle = new Triangle
{
    PositionA = new Vector3(1, 1, 21f),
    PositionB = new Vector3(0.5f, -1, 24),
    PositionC = new Vector3(1.5f, -1, 24),
    Material = blue,
};

Material light = new Material
{
    EmittedLight = 1,
    Color = new Vector3(255, 255, 128),
};
Sphere sunSphere = new Sphere
{
    Material = light,
    Position = new Vector3(-10, 18, 28),
    Radius = 16
};
Triangle sunTriangle = new Triangle
{
    PositionA = new Vector3(-20, 5, 45),
    PositionB = new Vector3(0, 7, 16),
    PositionC = new Vector3(20, 7, 16),
    Material = light,
};

Material green = new Material
{
    EmittedLight = 0,
    Color = new Vector3(0, 255, 0),
    Reflectivity = 0.5f
};
Sphere greenSphere = new Sphere
{
    Material = green,
    Position = new Vector3(0, -51f, 25),
    Radius = 50
};
Triangle greenTriangle = new Triangle
{
    PositionA = new Vector3(12, -1.5f, 28),
    PositionB = new Vector3(0, -0.5f, 12),
    PositionC = new Vector3(-12, -1f, 28),
    Material = green,
};

sceneSphere.Add(redSphere);
sceneSphere.Add(blueSphere);
sceneSphere.Add(greenSphere);
sceneSphere.Add(sunSphere);

sceneTriangle.Add(redTriangle);
sceneTriangle.Add(blueTriangle);
sceneTriangle.Add(greenTriangle);
sceneTriangle.Add(sunTriangle);
#endregion

Camera cam = new Camera(screenSize, true, sceneTriangle)
{
    Position = new Vector3(0, 2f, 29),
};

var t = new Stopwatch();
t.Start();
var v = cam.GetViewingPlane();
t.Stop();
Debug.WriteLine($"Render: {t.ElapsedMilliseconds}ms");

var p = new Renderer(screenSize, cam);
p.Draw(v);
p.Run();