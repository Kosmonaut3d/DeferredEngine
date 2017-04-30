using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.Helper.HelperGeometry
{
    //Singleton
    public class HelperGeometryManager
    {
        private static HelperGeometryManager _instance;

        private LineHelperManager _lineHelperManager;
        private OctahedronHelperManager _octahedronHelperManager;

        public HelperGeometryManager()
        {
            _lineHelperManager = new LineHelperManager();
            _octahedronHelperManager = new OctahedronHelperManager();
        }


        public static HelperGeometryManager GetInstance()
        {
            if (_instance == null) return _instance = new HelperGeometryManager();
            return _instance;
        }

        public void Draw(GraphicsDevice graphics, Matrix viewProjection, EffectParameter worldViewProjParam, EffectParameter globalColorParam, EffectPass vertexColorPass, EffectPass globalColorPass)
        {
            _lineHelperManager.Draw(graphics, viewProjection, worldViewProjParam, vertexColorPass);
            _octahedronHelperManager.Draw(graphics, viewProjection, worldViewProjParam, globalColorParam, globalColorPass);
        }

        public void AddLineStartDir(Vector3 start, Vector3 dir, short timer, Color startColor, Color endColor)
        {
            _lineHelperManager.AddLineStartDir(start, dir, timer, startColor, endColor);
        }

        public void CreateBoundingBoxLines(BoundingFrustum boundingFrustum)
        {
            _lineHelperManager.CreateBoundingBoxLines(boundingFrustum);
        }

        public void AddLineStartEnd(Vector3 startPosition, Vector3 EndPosition, short timer)
        {
            _lineHelperManager.AddLineStartEnd(startPosition, EndPosition, timer);
        }

        public void AddLineStartEnd(Vector3 start, Vector3 end, short timer, Color startColor, Color endColor)
        {
            _lineHelperManager.AddLineStartEnd(start,end, timer, startColor, endColor);
        }

        public void AddOctahedron(Vector3 position, Vector4 color)
        {
            _octahedronHelperManager.AddOctahedron(position, color);
        }

        public void AddBoundingBox(BasicEntity basicEntity)
        {
            _lineHelperManager.AddBoundingBox(basicEntity);
        }
    }
}
