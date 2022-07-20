using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Marcher
{
    internal class SceneBuilder
    {
        private int[] handles;
        private int sphereCount;
        private float[] sphereSizes, spherePositions, sphereColors;
        private Random random = new Random();

        public SceneBuilder(Shader shader, int sphereCount)
        {
            // Allocate sizes for sphere array attributes
            if(this.sphereCount !> 0 || this.sphereCount !< 64)
                this.sphereCount = 64;
            sphereSizes = new float[sphereCount];
            spherePositions = new float[sphereCount * 3];
            sphereColors = new float[sphereCount * 3];

            // Get opengl handles for attrubte locations
            handles = new int[4]{
                shader.GetUniformLocation("sphereCount"),
                shader.GetUniformLocation("sphereSizes"),
                shader.GetUniformLocation("spherePositions"),
                shader.GetUniformLocation("sphereColors")
            };

            // Give spheres random locations, sizes, and colors
            for (int i = 0; i < sphereCount; i++)
            {
                sphereSizes[i] = (float)random.Next(100, 400) / 100;
                spherePositions[i * 3] = (float)random.Next(-2000, 2000) / 100;
                spherePositions[i * 3 + 1] = (float)random.Next(-2000, 2000) / 100;
                spherePositions[i * 3 + 2] = (float)random.Next(1000,4000) / 100;
                spherePositions[i * 3 + 2] = 30;
                sphereColors[i * 3] = (float)random.Next(100) / 100;
                sphereColors[i * 3 + 1] = (float)random.Next(100) / 100;
                sphereColors[i * 3 + 2] = (float)random.Next(100) / 100;
            }

            // Send data to GPU for processing
            GL.Uniform1(handles[0], sphereCount);
            GL.Uniform1(handles[1], sphereCount, sphereSizes);
            GL.Uniform1(handles[2], sphereCount * 3, spherePositions);
            GL.Uniform1(handles[3], sphereCount * 3, sphereColors);
        }
    }
}
