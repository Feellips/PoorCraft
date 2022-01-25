using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PoorCraft.Blocks;
using PoorCraft.Math;
using System;
using System.Collections.Generic;

namespace PoorCraft
{
    // In this tutorial we take a look at how we can use textures to make the light settings we set up in the last episode
    // different per fragment instead of making them per object.
    // Remember to check out the shaders for how we converted to using textures there.
    public class Game : GameWindow
    {
        // Since we are going to use textures we of course have to include two new floats per vertex, the texture coords.

        private Block _block = new Grass(new Vector3());
        private Block _block2 = new Dirt(new Vector3());

        private List<Block> _blocks;

        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;
        private int _vaoGrass;
        private int _vaoDirt;
        private int _vaoLamp;

        private Shader _lampShader;
        private Shader _lightingShader;

        private Texture _diffuseMap;

        private Camera _camera;
        private NoiseGenerator _noiseGenerator;

        private bool _firstMove = true;

        private Vector2 _lastPos;
        private int _vertexBufferObject2;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _noiseGenerator = new NoiseGenerator(new Random().Next());

            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _block.Length * sizeof(float), _block.Data, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
            _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            {
                _vaoGrass = GL.GenVertexArray();
                GL.BindVertexArray(_vaoGrass);

                // All of the vertex attributes have been updated to now have a stride of 8 float sizes.
                var positionLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                var texCoordLocation = _lightingShader.GetAttribLocation("aTexCoords");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            }

            {
                _vertexBufferObject2 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject2);
                GL.BufferData(BufferTarget.ArrayBuffer, _block2.Length * sizeof(float), _block2.Data, BufferUsageHint.StaticDraw);

                _vaoDirt = GL.GenVertexArray();
                GL.BindVertexArray(_vaoDirt);

                // All of the vertex attributes have been updated to now have a stride of 8 float sizes.
                var positionLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                var texCoordLocation = _lightingShader.GetAttribLocation("aTexCoords");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            }

            {
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                // The lamp shader should have its stride updated aswell, however we dont actually
                // use the texture coords for the lamp, so we dont need to add any extra attributes.
                var positionLocation = _lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            }

            var cubeSize = 50;

            _blocks = new List<Block>(cubeSize * cubeSize);

            for (int i = 0; i < cubeSize; i++)
                for (int j = 0; j < cubeSize; j++)
                {
                    var z = (float)System.Math.Ceiling(_noiseGenerator.Noise(i * 0.1f, j * 0.1f) * 10);
                    _blocks.Add(new Grass(new Vector3(j, z, i)));

                    for (int k = (int)z; -10 < k; k--)
                    {
                        _blocks.Add(new Dirt(new Vector3(j, k, i)));
                    }
                }

            _diffuseMap = Texture.LoadSpriteSheetFromFile("DefaultPack.png");

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vaoGrass);

            _diffuseMap.Use(TextureUnit.Texture0);
            _lightingShader.Use();

            var grass = Matrix4.Identity;
            grass *= Matrix4.CreateScale(0.2f);

            _lightingShader.SetMatrix4("model", grass);
            _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _lightingShader.SetVector3("viewPos", _camera.Position);

            _lightingShader.SetInt("material.diffuse", 0);
            _lightingShader.SetFloat("material.shininess", 32.0f);

            _lightingShader.SetVector3("light.position", _lightPos);
            _lightingShader.SetVector3("light.ambient", new Vector3(0.2f));
            _lightingShader.SetVector3("light.diffuse", new Vector3(0.5f));

            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i] is Dirt)
                {
                    GL.BindVertexArray(_vaoDirt);
                }
                else
                {
                    GL.BindVertexArray(_vaoGrass);
                }

                Matrix4 model = Matrix4.CreateTranslation(_blocks[i].Position);
                model *= Matrix4.CreateScale(0.2f);
                _lightingShader.SetMatrix4("model", model);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }


            GL.BindVertexArray(_vaoGrass);

            _lampShader.Use();

            Matrix4 lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(_lightPos);

            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            if (mouse.IsButtonDown(MouseButton.Button1))
            {
                if (Cast(_camera) is Block block)
                    _blocks.Remove(block);
            }
        }

        private Block Cast(Camera player)
        {
            throw new NotImplementedException();

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < _blocks.Count; j++)
                {
                    var x1 = _blocks[i].Position.X;
                    var y1 = _blocks[i].Position.Y;
                    var x2 = _blocks[i].Position.X;
                    var y2 = _blocks[i].Position.Y;

                    var x3 = player.Position.X;
                    var y3 = player.Position.Y;
                    var x4 = player.Position.X + player.Front.X;
                    var y4 = player.Position.Y + player.Front.Y;

                    float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                    if (den == 0)
                    {
                        return null;
                    }

                    float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
                    float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
                    if (t > 0 && t < 1 && u > 0)
                    {
                        Vector3 pt = new Vector3();
                        pt.X = x1 + t * (x2 - x1);
                        pt.Y = y1 + t * (y2 - y1);
                        return pt;
                    }
                    else
                    {
                        return null;
                    }
                }

            return null;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            //_camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}
