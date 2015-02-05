using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Sledge.DataStructures.Geometric;
using Sledge.Rendering.Cameras;
using Sledge.Rendering.OpenGL.Arrays;
using Sledge.Rendering.OpenGL.Shaders;
using Sledge.Rendering.OpenGL.Vertices;

namespace Sledge.Rendering.OpenGL
{
    public class OpenGLRenderer : IRenderer
    {
        private readonly Dictionary<IViewport, ViewportData> _viewportData;

        public OpenGLRenderer()
        {
            _viewportData = new Dictionary<IViewport, ViewportData>();
        }

        public IViewport CreateViewport()
        {
            var view = new OpenGLViewport();
            view.Render += RenderViewport;
            return view;
        }

        private struct Thing
        {
            public Vector3 Position;
        }

        private void RenderViewport(IViewport viewport, Frame frame)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            var data = GetViewportData(viewport);

            var array = new TestArray(new[]
                                      {
                                          new Face(Material.Flat(Color.Green), new List<Coordinate>
                                                                             {
                                                                                 new Coordinate(0, 0, 0) * 10,
                                                                                 new Coordinate(1, 0, 0) * 10,
                                                                                 new Coordinate(1, 1, 0) * 10,
                                                                                 new Coordinate(0, 1, 0) * 10,
                                                                             }),
                                      });
            var prog = new Passthrough();

            // Set up FBO
            data.Framebuffer.Size = new Size(viewport.Control.Width, viewport.Control.Height);
            data.Framebuffer.Bind();
            
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ((PerspectiveCamera)viewport.Camera).Position += new Coordinate(-0.1m, -0.1m, -0.1m);
            var vpMatrix = viewport.Camera.GetViewportMatrix(viewport.Control.Width, viewport.Control.Height);
            var camMatrix = viewport.Camera.GetCameraMatrix();

            prog.Bind();
            prog.CameraMatrix = camMatrix;
            prog.ViewportMatrix = vpMatrix;
            array.Render();
            prog.Unbind();

            // Set up camera
            GL.LoadIdentity();
            GL.Viewport(0, 0, viewport.Control.Width, viewport.Control.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref vpMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref camMatrix);

            // Do actual render
            var colours = new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };

            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < colours.Length; i++)
            {
                var a = i * 10;
                var b = (i + 1) * 10;
                GL.Color3(colours[i]);
                GL.Vertex3(a, 0, 0);
                GL.Vertex3(b, 0, 0);
                GL.Vertex3(0, a, 0);
                GL.Vertex3(0, b, 0);
                GL.Vertex3(0, 0, a);
                GL.Vertex3(0, 0, b);
            }
            GL.End();

            // Blit FBO
            data.Framebuffer.Unbind();
            data.Framebuffer.Render();

            prog.Dispose();
            array.Dispose();

        }

        private ViewportData GetViewportData(IViewport viewport)
        {
            if (!_viewportData.ContainsKey(viewport))
            {
                var data = new ViewportData(new Framebuffer(viewport.Control.Width, viewport.Control.Height));
                _viewportData.Add(viewport, data);
            }
            return _viewportData[viewport];
        }

        private class ViewportData
        {
            public Framebuffer Framebuffer { get; set; }

            public ViewportData(Framebuffer framebuffer)
            {
                Framebuffer = framebuffer;
            }
        }
    }

    public class TestArray : VertexArray<Face, SimpleVertex>
    {
        public TestArray(IEnumerable<Face> data) : base(data)
        {
        }

        public void Render()
        {
            foreach (var subset in GetSubsets(1))
            {
                Render(PrimitiveType.Triangles, subset);
            }
        }

        protected override void CreateArray(IEnumerable<Face> data)
        {
            StartSubset(1);
            foreach (var face in data)
            {
                var index = PushData(Convert(face));
                PushIndex(1, index, Triangulate(face.Vertices.Count));
            }
            PushSubset(1, (object) null);
        }

        private IEnumerable<SimpleVertex> Convert(Face face)
        {
            return face.Vertices.Select(x => new SimpleVertex
                                             {
                                                 Position = x.ToVector3(),
                                                 Color = face.Material.Color.ToArgb()
                                             });
        }
    }
}