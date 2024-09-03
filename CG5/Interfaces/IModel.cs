using OpenTK.Mathematics;

namespace CG5.OpenGL.Interfaces;

public interface IModel : IDisposable
{
    public Matrix4 ModelMatrix { get; set; }
    public void Render();
}