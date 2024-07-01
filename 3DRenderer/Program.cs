using ILGPU.Runtime;
using ILGPU;
using System.Diagnostics;
using System.Numerics;
using Thingimajig;

int sceneSize = 512;
List<IObject> scene = new List<IObject>();

Camera cam = new Camera
{
    ResolutionHeight = sceneSize,
    Position = new Vector3(0, 1f, 29)
};

Material red = new Material
{
    EmittedLight = 0,
    Color = new Vector3(1, 0, 0),
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
    Color = new Vector3(0, 0, 1),
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
    Color = new Vector3(1, 1, 0.5f),
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
    Color = new Vector3(0, 1, 0),
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

var t = new Stopwatch();
t.Start();
var v = cam.GetViewingPlane(scene);
t.Stop();
Debug.WriteLine($"Render: {t.ElapsedMilliseconds}ms");

var p = new Renderer(sceneSize, scene, cam);
p.Draw(v);
p.Run();
t.Restart();

//Debug.WriteLine($"Create Renderer: {t.ElapsedMilliseconds}ms");

//t.Restart();
//Debug.WriteLine($"Create Bitmap: {t.ElapsedMilliseconds}ms");

//t.Restart();
//Debug.WriteLine($"Display Bitmap: {t.ElapsedMilliseconds}ms");


