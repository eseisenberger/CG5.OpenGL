using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GetProgramParameterName = OpenTK.Graphics.OpenGL4.GetProgramParameterName;
using GL = OpenTK.Graphics.OpenGL4.GL;
using MemoryBarrierFlags = OpenTK.Graphics.OpenGL4.MemoryBarrierFlags;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace CG5.OpenGL.Classes.Template;

public class Shader : IDisposable
{
    public const string ResourcesPath = "CG5.OpenGl.Resources";
    public int Handle { get; private set; }
    private Dictionary<string, int> Uniforms { get; } = new();

    public Shader(params (string path, ShaderType type)[] paths)
    {
        var sources = new List<(string source, ShaderType type)>();
        foreach (var (path, type) in paths)
        {
            sources.Add((ReadSource(path), type));
        }

        var shaders = new List<int>();
        foreach (var (source, type) in sources)
        {
            shaders.Add(CreateShader(source, type));
        }

        foreach (var shader in shaders)
        {
            CompileShader(shader);
        }

        CreateProgram(shaders.ToArray());

        CleanupShaders(shaders.ToArray());

        InitializeUniformsMap();
    }

    private string ReadSource(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{ResourcesPath}.{path}");
        if (stream == null) throw new Exception("Shader not found!");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private int CreateShader(string source, ShaderType type)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        return shader;
    }

    private void CompileShader(int shader)
    {
        GL.CompileShader(shader);

        var log = GL.GetShaderInfoLog(shader);
        if (log != string.Empty) Console.WriteLine(log);
    }

    private void CreateProgram(params int[] shaders)
    {
        Handle = GL.CreateProgram();

        foreach (var shader in shaders)
        {
            GL.AttachShader(Handle, shader);
        }

        GL.LinkProgram(Handle);
    }

    private void CleanupShaders(params int[] shaders)
    {
        foreach (var shader in shaders)
        {
            GL.DetachShader(Handle, shader);
            GL.DeleteShader(shader);
        }
    }

    private void InitializeUniformsMap()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniforms);
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformMaxLength, out int maxUniformLength);
        for (int i = 0; i < uniforms; i++)
        {
            GL.GetActiveUniform(Handle, i, maxUniformLength,
                out _, out int size, out _, out string nameTemplate);

            for (int j = 0; j < size; j++)
            {
                string name = Regex.Replace(nameTemplate, @"\[0\]$", $"[{j}]");
                Uniforms[name] = GL.GetUniformLocation(Handle, name);
            }
        }
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    private int GetUniformLocation(string name)
    {
        bool found = Uniforms.TryGetValue(name, out int location);
        if (found) return location;
        Console.WriteLine($"Uniform with name {name} not found.");
        return -1;
    }

    public int GetUniformBlockIndex(string name)
    {
        return GL.GetUniformBlockIndex(Handle, name);
    }

    public void LoadInteger(string name, int value)
    {
        GL.ProgramUniform1(Handle, GetUniformLocation(name), value);
    }

    public void LoadFloat(string name, float value)
    {
        GL.ProgramUniform1(Handle, GetUniformLocation(name), value);
    }

    public void LoadFloat2(string name, Vector2 value)
    {
        GL.ProgramUniform2(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadFloat2(string name, ref Vector2 value)
    {
        GL.ProgramUniform2(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadFloat3(string name, Vector3 value)
    {
        GL.ProgramUniform3(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadFloat3(string name, ref Vector3 value)
    {
        GL.ProgramUniform3(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadFloat4(string name, Vector4 value)
    {
        GL.ProgramUniform4(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadFloat4(string name, ref Vector4 value)
    {
        GL.ProgramUniform4(Handle, GetUniformLocation(name), ref value);
    }

    public void LoadMatrix2(string name, Matrix2 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix2(Handle, GetUniformLocation(name), transpose, ref value);
    }

    public void LoadMatrix2(string name, ref Matrix2 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix2(Handle, GetUniformLocation(name), transpose, ref value);
    }

    public void LoadMatrix3(string name, Matrix3 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix3(Handle, GetUniformLocation(name), transpose, ref value);
    }


    public void LoadMatrix3(string name, ref Matrix3 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix3(Handle, GetUniformLocation(name), transpose, ref value);
    }

    public void LoadMatrix4(string name, Matrix4 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix4(Handle, GetUniformLocation(name), transpose, ref value);
    }

    public void LoadMatrix4(string name, ref Matrix4 value, bool transpose = false)
    {
        GL.ProgramUniformMatrix4(Handle, GetUniformLocation(name), transpose, ref value);
    }

    public void Dispatch(int x, int y, int z)
    {
        GL.DispatchCompute(x, y, z);
    }

    public void Wait(MemoryBarrierFlags flags = MemoryBarrierFlags.AllBarrierBits)
    {
        GL.MemoryBarrier(flags);
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
        GC.SuppressFinalize(this);
    }
}