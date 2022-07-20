using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK_Marcher;

namespace OpenTK_Marcher
{
    public class Window : GameWindow
    {
        // Define variables to hold handles for the vertex buffer/array and Shader/Texture objects
        private int vertexBufferHandle, vertexArrayHandle, timeLocation, camLocation, mouseLocation, sceneLocation;
        private Vector2 mouse = new Vector2();
        private bool cursorGrabbed = false;
        private float time = 0.0f;
        private byte[] scene;
        private SceneBuilder sceneBuilder;
        Shader shader;

        DateTime _lastTime; // marks the beginning the measurement began
        int _framesRendered; // an increasing count
        int _fps; // the FPS calculated from the last measurement

        // Constructor for the window forcing a min/max size created in main function
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            this.CenterWindow();
            this.MaximumSize = Program.winSize;
            this.MinimumSize = Program.winSize;
        }

        // Most of this is setting up a single triangle for fast display
        // The bitmap class is quite slow compared to OpenGL
        // In reality most of the graphics processing will be handled by a fragment shader
        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(1f, 1f, 0f, 1f));

            // Define vertex array for full pixel manipulation via overscanned triangle
            float[] vertices = new float[]
            {
                 -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
                 3.0f, -1.0f, 0.0f, 2.0f, 0.0f, // bottom right
                -1.0f, 3f, 0.0f, 0.0f, 2.0f, // top left
            };

            // Bind vertex array
            this.vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayHandle);

            // Create handle for vertex buffer and flush vertex data to GPU for display
            this.vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Create shader, compilation done by constructor
            shader = new Shader(@".\Shaders\shader.vert", @".\Shaders\shader.frag");
            shader.Use();

            // Enable both vertex data and UV mapping for drawing
            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            // Pass enivronmenntal variables to fragment shader
            camLocation = shader.GetUniformLocation("cam_pos");
            float[] cam = new float[]
            {
                0.0f,0.0f,0.0f
            };
            GL.Uniform3(camLocation, cam[0], cam[1], cam[2]);
            timeLocation = shader.GetUniformLocation("time");
            int resLocation = shader.GetUniformLocation("ratio");
            GL.Uniform1(resLocation, (float)Program.winSize.X / (float)Program.winSize.Y);
            mouseLocation = shader.GetUniformLocation("mouse");

            // Build scene of multiple sphere objects
            sceneBuilder = new SceneBuilder(shader, 6);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Bind nothing to OpenGL buffer and program to free resources before exiting
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(this.vertexBufferHandle);
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Check if the Escape button is currently being pressed.
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                // If it is, close the window.
                Close();
            }

            if (KeyboardState.IsKeyPressed(Keys.LeftAlt))
            {
                cursorGrabbed = !cursorGrabbed;
                this.CursorState = cursorGrabbed ? CursorState.Grabbed : CursorState.Normal;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            mouse = MouseState.Position;
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Load vertex array to memory in GPU, apply shader, draw to screen
            GL.BindVertexArray(this.vertexArrayHandle);
            time += 0.01f;
            GL.Uniform1(timeLocation, time);
            GL.Uniform2(mouseLocation, mouse.X, mouse.Y);

            shader.Use();

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            this.Context.SwapBuffers();

            _framesRendered++;

            if ((DateTime.Now - _lastTime).TotalSeconds >= 1)
            {
                // one second has elapsed 

                _fps = _framesRendered;
                _framesRendered = 0;
                _lastTime = DateTime.Now;
            }
            this.Title = ($"OpenTK Ray Marcher FPS:{_fps} {mouse.X}");

            base.OnRenderFrame(e);
        }
    }
}
