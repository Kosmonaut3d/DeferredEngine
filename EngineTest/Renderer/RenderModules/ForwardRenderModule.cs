using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules
{
    public class ForwardRenderModule : IRenderModule, IDisposable
    {
        private const int MAXLIGHTS = 40;

        private Effect _shader;
        
        private EffectParameter _worldParam;
        private EffectParameter _worldViewProjParam;
        private EffectParameter _worldViewITParam;

        private EffectParameter _lightAmountParam;

        private EffectParameter _lightPositionWSParam;
        private EffectParameter _lightRadiusParam;
        private EffectParameter _lightIntensityParam;
        private EffectParameter _lightColorParam;

        private EffectParameter _cameraPositionWSParam;

        private Vector3[] LightPositionWS;
        private float[] LightRadius;
        private float[] LightIntensity;
        private Vector3[] LightColor;

        private EffectPass _pass1;
        
        public Matrix World { set { _worldParam.SetValue(value); } }
        public Matrix WorldViewProj { set { _worldViewProjParam.SetValue(value); } }
        public Matrix WorldViewIT { set { _worldViewITParam.SetValue(value); } }

        public ForwardRenderModule(ContentManager content, string shaderPath)
        {
            Load(content, shaderPath);
            Initialize();
        }

        public void Initialize()
        {
            _worldParam = _shader.Parameters["World"];
            _worldViewProjParam = _shader.Parameters["WorldViewProj"];
            _worldViewITParam = _shader.Parameters["WorldViewIT"];

            _lightAmountParam = _shader.Parameters["LightAmount"];
            
            _lightPositionWSParam = _shader.Parameters["LightPositionWS"];
            _lightRadiusParam = _shader.Parameters["LightRadius"];
            _lightIntensityParam = _shader.Parameters["LightIntensity"];
            _lightColorParam = _shader.Parameters["LightColor"];

            _cameraPositionWSParam = _shader.Parameters["CameraPositionWS"];

            _pass1 = _shader.Techniques["Default"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);

        }

        public RenderTarget2D Draw(GraphicsDevice graphicsDevice, RenderTarget2D output, MeshMaterialLibrary meshMat, Matrix viewProjection, Camera camera, List<PointLight> pointLights)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            SetupLighting(camera, pointLights);

            meshMat.Draw(MeshMaterialLibrary.RenderType.Forward, viewProjection, renderModule: this);

            return output;
        }

        private void SetupLighting(Camera camera, List<PointLight> pointLights)
        {
            //Setup camera
            _cameraPositionWSParam.SetValue(camera.Position);

            int count = pointLights.Count > 40 ? MAXLIGHTS : pointLights.Count;

            if (LightPositionWS == null || pointLights.Count != LightPositionWS.Length)
            {
                
                LightPositionWS = new Vector3[count];
                LightColor = new Vector3[count];
                LightIntensity = new float[count];
                LightRadius = new float[count];

                _lightAmountParam.SetValue( count );
            }

            //Fill


            for (var index = 0; index < count; index++)
            {
                PointLight light = pointLights[index];

                LightPositionWS[index] = light.Position;
                LightColor[index] = light.ColorV3;
                LightIntensity[index] = light.Intensity;
                LightRadius[index] = light.Radius;
            }

            _lightPositionWSParam.SetValue(LightPositionWS);
            _lightColorParam.SetValue(LightColor);
            _lightIntensityParam.SetValue(LightIntensity);
            _lightRadiusParam.SetValue(LightRadius);
        }
        

        public void Dispose()
        {
            _shader?.Dispose();
        }

        public void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection)
        {
            //Matrix worldView = localWorldMatrix * (Matrix)view;
            World = localWorldMatrix;
            WorldViewProj = localWorldMatrix * viewProjection;
            WorldViewIT = Matrix.Transpose( Matrix.Invert(localWorldMatrix));

            _pass1.Apply();
            //_WorldViewProj.SetValue(localWorldMatrix * viewProjection);

            //worldView = Matrix.Transpose(Matrix.Invert(worldView));
            //_WorldViewIT.SetValue(worldView);
        }
    }
}
