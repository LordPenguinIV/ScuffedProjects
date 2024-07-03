using System.Diagnostics;
using System.Numerics;
using Thingimajig;

int screenSize = 512;
List<Sphere> scene = new List<Sphere>();

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

Material light = new Material
{
    EmittedLight = 1,
    Color = new Vector3(255, 255, 128),
};
Sphere sun = new Sphere
{
    Material = light,
    Position = new Vector3(-10, 15, 28),
    Radius = 12
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

scene.Add(redSphere);
scene.Add(blueSphere);
scene.Add(greenSphere);
scene.Add(sun);
#endregion

Camera cam = new Camera(screenSize, true, scene)
{
    Position = new Vector3(0, 1f, 29),
};

var t = new Stopwatch();
t.Start();
var v = cam.GetViewingPlane();
t.Stop();
Debug.WriteLine($"Render: {t.ElapsedMilliseconds}ms");

var p = new Renderer(screenSize, cam);
p.Draw(v);
p.Run();
