using System.Numerics;
using Thingimajig;

public class Model
{
    public Model(string filePath, Material material, Vector3 position = default)
    {
        FilePath = filePath;
        Material = material;
        Triangles = new List<Triangle>();
        Position = position;
    }

    public string FilePath { get; set; }
    public Material Material { get; set; }
    public List<Triangle> Triangles { get; set; }
    public Vector3 Position { get; set; }

    public void BuildTriangles()
    {
        Stream file = File.OpenRead(FilePath);
        BinaryReader reader = new BinaryReader(file);

        byte[] header = reader.ReadBytes(80);
        uint numTri = reader.ReadUInt32();

        for (int i = 0; i < numTri; i++)
        {
            Vector3 normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 posA = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 posB = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 posC = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            ushort attribute = reader.ReadUInt16();

            Triangles.Add(new Triangle(posA + Position, posB + Position, posC + Position, Material, normal));
        }

        reader.Close();
        file.Close();
    }

}