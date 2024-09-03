using OpenTK.Mathematics;

namespace CG5.Interfaces;

public interface IModel : IDisposable
{
    public Matrix4 ModelMatrix { get; set; }
    public void Render();
}