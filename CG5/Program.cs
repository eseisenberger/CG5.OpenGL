using System.Runtime.InteropServices;
using CG5.OpenGL.Classes.Template;
using CG5.OpenGL.Interfaces;
using CG5.OpenGL.Objects;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CG5.OpenGL;

public class Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    private bool IsLoaded { get; set; }
    private Shader Shader { get; set; } = null!;
    private ImGuiController ImGuiController { get; set; } = null!;
    private IModel CurrentModel { get; set; } = null!;
    private Camera Camera { get; set; } = null!;
    private Texture Texture { get; set; } = null!;
    
    private IModel LightSource { get; set; } = null!;
    private Vector3 LightPosition { get; set; } = new(1.2f, 1.0f, 2.0f);
    private Shader LightShader { get; set; } = null!;
    
    private DebugProc DebugProcCallback { get; } = OnDebugMessage;

    public static void Main(string[] _)
    {
        var gwSettings = GameWindowSettings.Default;
        var nwSettings = NativeWindowSettings.Default;
        nwSettings.NumberOfSamples = 16;

        using var program = new Program(gwSettings, nwSettings);
        program.Title = "Project Title";
        program.Size = new Vector2i(1280, 800);
        program.Run();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.DebugMessageCallback(DebugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        
        Shader = new Shader(
            paths:
            [
                ("shader.vert", ShaderType.VertexShader), 
                ("shader.frag", ShaderType.FragmentShader)
            ]
        );
        
        ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        Camera = new Camera(new OrbitingControl(5 * Vector3.UnitZ, Vector3.Zero), new PerspectiveProjection());

        CurrentModel = new Cuboid();
        
        LightShader = new Shader(
            paths:
            [
                ("lightShader.vert", ShaderType.VertexShader), 
                ("lightShader.frag", ShaderType.FragmentShader)
            ]
        );
        
        LightSource = new Cuboid(0.2f, 0.2f, 0.2f);
        LightSource.ModelMatrix *= Matrix4.CreateTranslation(LightPosition);
        
        StbImageSharp.StbImage.stbi_set_flip_vertically_on_load(1);
        Texture = new Texture("texture.jpg");

        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        
        GL.ClearColor(0.808f, 0.670f, 0.576f, 1.0f);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        IsLoaded = true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        CurrentModel.Dispose();
        ImGuiController.Dispose();
        Texture.Dispose();
        Shader.Dispose();

        IsLoaded = false;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (!IsLoaded) return;

        base.OnResize(e);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        ImGuiController.OnWindowResized(ClientSize.X, ClientSize.Y);
        Camera.Aspect = (float)ClientSize.X / ClientSize.Y;
    }

    private float _time;
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        _time += (float)args.Time;

        ImGuiController.Update((float)args.Time);
        Camera.Update((float)args.Time);

        CurrentModel.ModelMatrix = Matrix4.CreateRotationY(_time * 0.25f);

        if (ImGui.GetIO().WantCaptureMouse) return;

        var keyboard = KeyboardState.GetSnapshot();
        var mouse = MouseState.GetSnapshot();

        Camera.HandleInput((float)args.Time, keyboard, mouse);

        if (keyboard.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Shader.Use();
        
        var mvp = CurrentModel.ModelMatrix * Camera.ProjectionViewMatrix;
        
        Shader.LoadMatrix4("model", CurrentModel.ModelMatrix);
        Shader.LoadMatrix4("view", Camera.ViewMatrix);
        Shader.LoadMatrix4("projection", Camera.ProjectionMatrix);
        
        //Shader.LoadFloat3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
        Shader.LoadFloat3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
        Shader.LoadFloat3("lightPosition", LightPosition);
        Shader.LoadFloat3("viewPosition", Camera.Position);
        
        Texture.Bind();
        Shader.LoadInteger("texture1", 0);
        
        CurrentModel.Render();
        
        LightShader.Use();
        
        LightShader.LoadMatrix4("model", LightSource.ModelMatrix);
        LightShader.LoadMatrix4("view", Camera.ViewMatrix);
        LightShader.LoadMatrix4("projection", Camera.ProjectionMatrix);
        
        LightSource.Render();
        
        DebugMatrix(CurrentModel.ModelMatrix, "Model Matrix");
        DebugMatrix(Camera.ViewMatrix, "View Matrix");
        DebugMatrix(Camera.ProjectionMatrix, "Projection Matrix");
        DebugMatrix(mvp, "MVP Matrix");

        RenderGui();

        Context.SwapBuffers();
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        ImGuiController.OnKey(e, true);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);

        ImGuiController.OnKey(e, false);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        ImGuiController.OnMouseButton(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        ImGuiController.OnMouseButton(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        ImGuiController.OnMouseMove(e);
    }

    private void DebugMatrix(Matrix4 matrix, string name)
    {
        ImGui.Begin(name, ImGuiWindowFlags.AlwaysAutoResize);
        var c0 = new System.Numerics.Vector4(matrix.Column0.X, matrix.Column0.Y, matrix.Column0.Z, matrix.Column0.W);
        var c1 = new System.Numerics.Vector4(matrix.Column1.X, matrix.Column1.Y, matrix.Column1.Z, matrix.Column1.W);
        var c2 = new System.Numerics.Vector4(matrix.Column2.X, matrix.Column2.Y, matrix.Column2.Z, matrix.Column2.W);
        var c3 = new System.Numerics.Vector4(matrix.Column3.X, matrix.Column3.Y, matrix.Column3.Z, matrix.Column3.W);
        ImGui.PushID($"{name}_c0"); ImGui.InputFloat4("", ref c0); ImGui.PopID();
        ImGui.PushID($"{name}_c1"); ImGui.InputFloat4("", ref c1); ImGui.PopID();
        ImGui.PushID($"{name}_c2"); ImGui.InputFloat4("", ref c2); ImGui.PopID();
        ImGui.PushID($"{name}_c3"); ImGui.InputFloat4("", ref c3); ImGui.PopID();
        ImGui.End();
    }

    private static int _control = 1;
    private static int _projection;
    private void RenderGui()
    {
        ImGui.Begin("Camera", ImGuiWindowFlags.AlwaysAutoResize);
        if (ImGui.CollapsingHeader("Control"))
        {
            ImGui.Indent(10);
            if (ImGui.RadioButton("No Control", ref _control, 0))
                Camera.Control = new NoControl(Camera.Control);
            if (ImGui.RadioButton("Orbital Control", ref _control, 1))
                Camera.Control = new OrbitingControl(Camera.Position, Vector3.Zero);
            if (ImGui.RadioButton("FlyBy Control", ref _control, 2))
                Camera.Control = new FlyByControl(Camera.Control);

            ImGui.Indent(-10);
        }

        if (ImGui.CollapsingHeader("Projection"))
        {
            ImGui.Indent(10);
            if (ImGui.RadioButton("Perspective", ref _projection, 0))
                Camera.Projection = new PerspectiveProjection { Aspect = Camera.Aspect };
            if (ImGui.RadioButton("Orthographic", ref _projection, 1))
                Camera.Projection = new OrthographicProjection { Aspect = Camera.Aspect, Height = 5 };
            ImGui.Indent(-10);
        }

        if (ImGui.CollapsingHeader("Details"))
        {
            ImGui.Indent(10);
            var position = new System.Numerics.Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
            var front = new System.Numerics.Vector3(Camera.Front.X, Camera.Front.Y, Camera.Front.Z);
            var right = new System.Numerics.Vector3(Camera.Right.X, Camera.Right.Y, Camera.Right.Z);
            var up = new System.Numerics.Vector3(Camera.Up.X, Camera.Up.Y, Camera.Up.Z);
            ImGui.InputFloat3("Camera position", ref position);
            ImGui.InputFloat3("Camera front", ref front);
            ImGui.InputFloat3("Camera right", ref right);
            ImGui.InputFloat3("Camera up", ref up);
            ImGui.Indent(-10);
        }

        ImGui.End();

        ImGuiController.Render();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        ImGuiController.OnPressedChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        ImGuiController.OnMouseScroll(e);
    }

    private static void OnDebugMessage(
        DebugSource source,     // Source of the debugging message.
        DebugType type,         // Type of the debugging message.
        int id,                 // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length,             // Length of the string in pMessage.
        IntPtr pMessage,        // Pointer to message string.
        IntPtr pUserParam)      // The pointer you gave to OpenGL.
    {
        var message = Marshal.PtrToStringAnsi(pMessage, length);

        var log = $"[{severity} source={source} type={type} id={id}] {message}";

        Console.WriteLine(log);
    }
}

public struct Vertex(Vector3 position, Vector2 textureCoordinate, Vector3? normal = null)
{
    public Vector3 Position = position;
    public Vector2 TextureCoordinate = textureCoordinate;
    public Vector3 Normal = normal ?? Vector3.Zero;
}