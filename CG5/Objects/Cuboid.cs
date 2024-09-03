using System.Runtime.InteropServices;
using CG5.Classes.Template;
using CG5.Interfaces;
using OpenTK.Mathematics;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG5.Objects;

public class Cuboid : IModel
{
    public Matrix4 ModelMatrix { get; set; }
    public void Render()
    {
        Mesh.Bind();
        Mesh.RenderIndexed();
    }

    public Mesh Mesh { get; }

    public Cuboid(float width = 1, float height = 1, float depth = 1)
    {
        Mesh = GenerateMesh(width / 2, height / 2, depth / 2);
        ModelMatrix = Matrix4.Identity;
    }

    private Mesh GenerateMesh(float halfWidth, float halfHeight, float halfDepth)
    {
        var leftTopFront = new Vector3(-halfWidth, -halfHeight, halfDepth);
        var rightTopFront = new Vector3(halfWidth, -halfHeight, halfDepth);
        var leftBottomFront = new Vector3(-halfWidth, halfHeight, halfDepth);
        var rightBottomFront = new Vector3(halfWidth, halfHeight, halfDepth);
        var leftTopBack = new Vector3(-halfWidth, -halfHeight, -halfDepth);
        var rightTopBack = new Vector3(halfWidth, -halfHeight, -halfDepth);
        var leftBottomBack = new Vector3(-halfWidth, halfHeight, -halfDepth);
        var rightBottomBack = new Vector3(halfWidth, halfHeight, -halfDepth);

        var topNormal = new Vector3(0, -1, 0);     
        var bottomNormal = new Vector3(0, 1, 0); 
        var frontNormal = new Vector3(0, 0, 1);   
        var backNormal = new Vector3(0, 0, -1);   
        var leftNormal = new Vector3(-1, 0, 0);   
        var rightNormal = new Vector3(1, 0, 0);   

        
        Vertex[] vertices = 
        [
            // Front face
            new Vertex(rightTopFront, new Vector2(1f/3, 1f/2), frontNormal),
            new Vertex(leftTopFront, new Vector2(0, 1f/2), frontNormal),
            new Vertex(leftBottomFront, new Vector2(0, 0), frontNormal),
            new Vertex(rightBottomFront, new Vector2(1f/3, 0), frontNormal),

            // Top face
            new Vertex(rightTopBack, new Vector2(2f/3, 1), topNormal),
            new Vertex(leftTopBack, new Vector2(1f/3, 1), topNormal),
            new Vertex(leftTopFront, new Vector2(1f/3, 1f/2), topNormal),
            new Vertex(rightTopFront, new Vector2(2f/3, 1f/2), topNormal),
            
            // Right face
            new Vertex(rightTopBack, new Vector2(2f/3, 1f/2), rightNormal),
            new Vertex(rightTopFront, new Vector2(1f, 1f/2), rightNormal),
            new Vertex(rightBottomFront, new Vector2(1f, 0), rightNormal),
            new Vertex(rightBottomBack, new Vector2(2f/3, 0), rightNormal),

            // Back face
            new Vertex(leftTopBack, new Vector2(0, 1), backNormal),
            new Vertex(rightTopBack, new Vector2(1f/3, 1), backNormal),
            new Vertex(rightBottomBack, new Vector2(1f/3, 1f/2), backNormal),
            new Vertex(leftBottomBack, new Vector2(0, 1f/2), backNormal),

            // Bottom face
            new Vertex(leftBottomFront, new Vector2(1f/3, 1f/2), bottomNormal),
            new Vertex(rightBottomFront, new Vector2(2f/3, 1f/2), bottomNormal),
            new Vertex(rightBottomBack, new Vector2(2f/3, 0), bottomNormal),
            new Vertex(leftBottomBack, new Vector2(1f/3, 0), bottomNormal),

            // Left face
            new Vertex(leftTopFront, new Vector2(2f/3, 1), leftNormal),
            new Vertex(leftTopBack, new Vector2(1, 1), leftNormal),
            new Vertex(leftBottomBack, new Vector2(1, 1f/2), leftNormal),
            new Vertex(leftBottomFront, new Vector2(2f/3, 1f/2), leftNormal)
        ];
        
        short[] indices =
        [
            // Front face
            0, 1, 2, 
            0, 2, 3,

            // Back face
            4, 5, 6, 
            4, 6, 7,

            // Left face
            8, 9, 10, 
            8, 10, 11,

            // Right face
            12, 13, 14,
            12, 14, 15,

            // Top face
            16, 17, 18, 
            16, 18, 19,

            // Bottom face
            20, 21, 22, 
            20, 22, 23
        ];

        var indexBuffer = new IndexBuffer(
            indices, 
            indices.Length * sizeof(short), 
            DrawElementsType.UnsignedShort,
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

    public void Dispose()
    {
        Mesh.Dispose();
    }
}