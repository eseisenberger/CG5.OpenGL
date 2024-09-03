using System.Runtime.InteropServices;
using CG5.Classes.Template;
using CG5.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CG5.Objects;

public class Sphere : IModel
{
    public Matrix4 ModelMatrix { get; set; }
    private Mesh Mesh { get; }

    public Sphere(float radius = 1.0f, int latitudeSegments = 20, int longitudeSegments = 20)
    {
        Mesh = GenerateMesh(radius, latitudeSegments, longitudeSegments);
    }
    public void Render()
    {
        Mesh.Bind();
        Mesh.RenderIndexed();
    }

    private static Mesh GenerateMesh(float radius, int latitudeSegments, int longitudeSegments)
    {
        var vertices = GenerateSphereVertices(radius, latitudeSegments, longitudeSegments);
        var indices = GenerateSphereIndices(latitudeSegments, longitudeSegments);
        
        var indexBuffer = new IndexBuffer(
            indices, 
            indices.Length * sizeof(int), 
            DrawElementsType.UnsignedInt,
            indices.Length
        );

        var vertexBuffer = new VertexBuffer(
            vertices,
            vertices.Length * Marshal.SizeOf<Vertex>(),
            vertices.Length,
            BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3), /* positions */
            new VertexBuffer.Attribute(1, 2), /* texture coordinates */
            new VertexBuffer.Attribute(2, 3) /* normal */
        );

        return new Mesh(PrimitiveType.Triangles, indexBuffer, vertexBuffer);
    }

    private static Vertex[] GenerateSphereVertices(float radius, int latitudeSegments, int longitudeSegments)
    {
        var vertices = new List<Vertex>();

        for (var lat = 0; lat <= latitudeSegments; lat++)
        {
            var theta = MathF.PI * lat / latitudeSegments;
            var sinTheta = MathF.Sin(theta);
            var cosTheta = MathF.Cos(theta);

            for (var lon = 0; lon <= longitudeSegments; lon++)
            {
                var phi = 2 * MathF.PI * lon / longitudeSegments;
                var sinPhi = MathF.Sin(phi);
                var cosPhi = MathF.Cos(phi);

                var x = cosPhi * sinTheta;
                var y = cosTheta;
                var z = sinPhi * sinTheta;
                var position = new Vector3(x, y, z) * radius;

                var normal = Vector3.Normalize(new Vector3(x, y, z));

                var u = (float)lon / longitudeSegments;
                var v = (float)lat / latitudeSegments;
                var textureCoordinate = new Vector2(u, v);

                // Create the vertex
                var vertex = new Vertex(position, textureCoordinate, normal);
                vertices.Add(vertex);
            }
        }

        return vertices.ToArray();
    }

    private static int[] GenerateSphereIndices(int latitudeSegments, int longitudeSegments)
    {
        var indices = new List<int>();

        for (var lat = 0; lat < latitudeSegments; lat++)
        {
            for (var lon = 0; lon < longitudeSegments; lon++)
            {
                var current = lat * (longitudeSegments + 1) + lon;
                var next = current + longitudeSegments + 1;

                // Triangle 1 (upper triangle)
                indices.Add(current);
                indices.Add(next);
                indices.Add(current + 1);

                // Triangle 2 (lower triangle)
                indices.Add(current + 1);
                indices.Add(next);
                indices.Add(next + 1);
            }
        }

        return indices.ToArray();
    }

    public void Dispose()
    {
        Mesh.Dispose();
    }
}