using System.Numerics;
using Thingimajig;

public class Model
{
    public Model(string filePath, Material material, Vector3 position = default, Vector3 rotation = default)
    {
        FilePath = filePath;
        Material = material;
        Position = position;
        Rotation = rotation;

        Triangles = new List<Triangle>();
        RotationMatrix = GetRotationMatrix();
    }

    public string FilePath { get; set; }
    public Material Material { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public List<Triangle> Triangles { get; set; }
    public float[,] RotationMatrix { get; set; }

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

            if (Rotation != default)
            {
                Triangles.Add(new Triangle(GetRotatedVector(posA + Position), GetRotatedVector(posB + Position), GetRotatedVector(posC + Position), Material));
            }
            else
            {
                Triangles.Add(new Triangle(posA + Position, posB + Position, posC + Position, Material, normal));
            }
        }

        reader.Close();
        file.Close();
    }

    public Vector3 GetRotatedVector(Vector3 vector)
    {
        Vector3 rotated = new Vector3(vector.X * RotationMatrix[0, 0] + vector.Y * RotationMatrix[0, 1] + vector.Z * RotationMatrix[0, 2],
            vector.X * RotationMatrix[1, 0] + vector.Y * RotationMatrix[1, 1] + vector.Z * RotationMatrix[1, 2],
            vector.X * RotationMatrix[2, 0] + vector.Y * RotationMatrix[2, 1] + vector.Z * RotationMatrix[2, 2]);

        return rotated;
    }

    public float[,] GetRotationMatrix()
    {
        float degToRadScale = MathF.PI / 180;
        float alpha = Rotation.Z * degToRadScale;
        float beta = Rotation.X * degToRadScale; //supposed to by y
        float gamma = Rotation.Y * degToRadScale; // supposed to by x

        var r = new float[3, 3];

        r[0, 0] = MathF.Cos(alpha) * MathF.Cos(beta);
        r[0, 1] = MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) - MathF.Sin(alpha) * MathF.Cos(gamma);
        r[0, 2] = MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) + MathF.Sin(alpha) * MathF.Sin(gamma);
        r[1, 0] = MathF.Sin(alpha) * MathF.Cos(beta);
        r[1, 1] = MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) + MathF.Cos(alpha) * MathF.Cos(gamma);
        r[1, 2] = MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) - MathF.Cos(alpha) * MathF.Sin(gamma);
        r[2, 0] = -MathF.Sin(beta);
        r[2, 1] = MathF.Cos(beta) * MathF.Sin(gamma);
        r[2, 2] = MathF.Cos(beta) * MathF.Cos(gamma);

        return r;
    }

}