using System.Runtime.InteropServices;
using CG5.OpenGL.Classes;
using CG5.OpenGL.Classes.Template;
using CG5.OpenGL.Interfaces;
using OpenTK.Mathematics;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG5.OpenGL.Objects;

public class Cylinder : IModel
{
    public Matrix4 ModelMatrix { get; set; }
    public Mesh SideMesh { get; }
    public Mesh BottomMesh { get; }
    public Mesh TopMesh { get; }
    
    public Cylinder(float radius = 1, float height = 1, int vertexCount = 256)
    {
        TopMesh = GenerateTopMesh(radius, height, vertexCount);
        BottomMesh = GenerateBottomMesh(radius, height, vertexCount);

        var sideVertices = GenerateSideVertices(radius, height, vertexCount);
        var sideIndices = GenerateSideIndices(vertexCount);
        
        var indexBuffer = new IndexBuffer(sideIndices, sideIndices.Length * sizeof(short),
            DrawElementsType.UnsignedShort, sideIndices.Length);
        
        var vertexBuffer = new VertexBuffer(sideVertices, sideVertices.Length * Marshal.SizeOf<Vertex>(),
            sideVertices.Length, BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3) /* positions */,
            new VertexBuffer.Attribute(1, 2) /* texture coordinates */,
            new VertexBuffer.Attribute(2, 3) /* normal */);
        
        SideMesh = new Mesh(PrimitiveType.Triangles, indexBuffer, vertexBuffer);
        
        ModelMatrix = Matrix4.Identity;
    }

    public void Render()
    {
        TopMesh.Bind();
        TopMesh.RenderIndexed();
        
        BottomMesh.Bind();
        BottomMesh.RenderIndexed();
        
        SideMesh.Bind();
        SideMesh.RenderIndexed();
    }
    
    private static Mesh GenerateTopMesh(float radius, float height, int vertexCount)
    {
        var topVertices = GenerateBaseVertices(
            radius: radius, 
            height: -height / 2, 
            count: vertexCount, 
            texCenter: new Vector2(0.25f, 0.75f)
        );

        topVertices = topVertices.Select(x => x with { Normal = new Vector3(0,-1,0) }).ToArray();
        
        var topVertexBuffer = new VertexBuffer(topVertices, topVertices.Length * Marshal.SizeOf<Vertex>(),
            topVertices.Length, BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3) /* positions */,
            new VertexBuffer.Attribute(1, 2) /* texture coordinates */,
            new VertexBuffer.Attribute(2, 3) /* normals */);
        
        var topIndices = GenerateBaseIndices(vertexCount);
        
        var topIndexBuffer = new IndexBuffer(topIndices, topIndices.Length * sizeof(short),
            DrawElementsType.UnsignedShort, topIndices.Length);
        
        return new Mesh(PrimitiveType.Triangles, topIndexBuffer, topVertexBuffer);
    }

    private static Mesh GenerateBottomMesh(float radius, float height, int vertexCount)
    {
        var bottomVertices = GenerateBaseVertices(
            radius: radius, 
            height: height / 2, 
            count: vertexCount,
            texCenter: new Vector2(0.75f, 0.75f)
        );

        bottomVertices = bottomVertices.Select(x => x with { Normal = new Vector3(0,1,0) }).ToArray();
        
        var bottomVertexBuffer = new VertexBuffer(bottomVertices, bottomVertices.Length * Marshal.SizeOf<Vertex>(),
            bottomVertices.Length, BufferUsageHint.StaticDraw,
            new VertexBuffer.Attribute(0, 3) /* positions */,
            new VertexBuffer.Attribute(1, 2) /* texture coordinates */,
            new VertexBuffer.Attribute(2, 3) /* normals */);
        
        var bottomIndices = GenerateBaseIndices(vertexCount);
        
        var bottomIndexBuffer = new IndexBuffer(bottomIndices, bottomIndices.Length * sizeof(short),
            DrawElementsType.UnsignedShort, bottomIndices.Length);
        
        return new Mesh(PrimitiveType.Triangles, bottomIndexBuffer, bottomVertexBuffer);
    }
    
    private static Vertex[] GenerateBaseVertices(float radius, float height, int count, Vector2 texCenter)
    {
        var vertices = new Vertex[count + 2];
        
        var center = new Vertex(
            new Vector3(0, height, 0),
            texCenter
        );
        
        vertices[0] = center;
        
        var rotationChange = 2 * MathF.PI / count;
        var currentCylindricalPosition = new Vector3(radius, height, 0);
        
        for (var i = 1; i <= count + 1; i++)
        {
            var position = Utilities.CylindricalToCartesian(currentCylindricalPosition);
            var uv = Utilities.PolarToCartesian(center.TextureCoordinate, 0.25f, currentCylindricalPosition.Z);

            vertices[i] = new Vertex(position, uv);
            
            currentCylindricalPosition.Z += rotationChange;
        }

        return vertices;
    }
    
    private static short[] GenerateBaseIndices(int count)
    {
        short[] indices = new short[3 * count];

        for (short i = 0; i < count; i++)
        {
            indices[i * 3 + 0] = ((short)(0));
            indices[i * 3 + 1] = ((short)(i + 1));
            indices[i * 3 + 2] = ((short)(i + 2));
        }

        return indices;
    }

    private static Vertex[] GenerateSideVertices(float radius, float height, int count)
    {
        var vertices = new Vertex[2 * count + 2];
        
        var rotationChange = 2 * MathF.PI / count;
        var currentCylindricalPosition = new Vector3(radius, -height / 2, 0);
        
        for (var i = 0; i <= count; i++)
        {
            var position = Utilities.CylindricalToCartesian(currentCylindricalPosition);
            var uv = new Vector2((float)i / count, 0.5f);

            vertices[i * 2] = new Vertex(position, uv, position);
            vertices[i * 2 + 1] = new Vertex(position with { Y = height / 2 }, uv with { Y = 0.0f }, position);
            
            currentCylindricalPosition.Z += rotationChange;
        }

        return vertices;
    }

    private static short[] GenerateSideIndices(int vertexCount)
    {
        var indices = new List<short>();

        for (short i = 0; i < vertexCount * 2; i += 2)
        {
            //face 1
            indices.Add((short)(i + 0));
            indices.Add((short)(i + 1));
            indices.Add((short)(i + 2));
            
            //face 2
            indices.Add((short)(i + 1));
            indices.Add((short)(i + 2));
            indices.Add((short)(i + 3));
        }

        return indices.ToArray();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}