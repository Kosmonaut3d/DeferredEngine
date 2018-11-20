//#define SHOWTILES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeferredEngine.Entities;
using DeferredEngine.Logic;
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
        private const int MAXLIGHTSPERTILE = 40;

        private Effect _shader;
        
        private EffectParameter _worldParam;
        private EffectParameter _worldViewProjParam;
        private EffectParameter _worldViewITParam;

        private EffectParameter _lightAmountParam;

        private EffectParameter _lightPositionWSParam;
        private EffectParameter _lightRadiusParam;
        private EffectParameter _lightIntensityParam;
        private EffectParameter _lightColorParam;

        private EffectParameter _tiledListLengthParam;

        private EffectParameter _cameraPositionWSParam;

        private Vector3[] LightPositionWS;
        private float[] LightRadius;
        private float[] LightIntensity;
        private Vector3[] LightColor;

        private int[][] TiledList;
        private float[] TiledListLength;
        private BoundingFrustumEx _tileFrustum;
        private Vector3[] _tileFrustumCorners = new Vector3[8];

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

            _tiledListLengthParam = _shader.Parameters["TiledListLength"];

            _cameraPositionWSParam = _shader.Parameters["CameraPositionWS"];

            _pass1 = _shader.Techniques["Default"].Passes[0];
        }

        public void Load(ContentManager content, string shaderPath)
        {
            _shader = content.Load<Effect>(shaderPath);

        }

        /// <summary>
        /// Draw forward shaded, alpha blended materials. Very basic and unoptimized algorithm. Can be improved to use tiling in future.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="output"></param>
        /// <param name="meshMat"></param>
        /// <param name="viewProjection"></param>
        /// <param name="camera"></param>
        /// <param name="pointLights"></param>
        /// <param name="frustum"></param>
        /// <returns></returns>
        public RenderTarget2D Draw(GraphicsDevice graphicsDevice, RenderTarget2D output, MeshMaterialLibrary meshMat, Matrix viewProjection, Camera camera, List<PointLight> pointLights, BoundingFrustum frustum)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            SetupLighting(camera, pointLights, frustum);

            //Draw Frustum debug test
            //Matrix view = Matrix.CreateLookAt(new Vector3(-88, -11f, 4), new Vector3(38, 8, 32), Vector3.UnitZ);
            //Matrix projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), 1.6f, 1, 100);
            //BoundingFrustumEx frustum2 = new BoundingFrustumEx(view * projection);
            //LineHelperManager.AddFrustum(frustum2, 1, Color.Red);

            //Vector3[] corners = frustum.GetCorners();
            //BoundingFrustumEx frustum3 = new BoundingFrustumEx(ref corners);
            
            //TiledLighting(frustum, pointLights, 20, 10);

            meshMat.Draw(MeshMaterialLibrary.RenderType.Forward, viewProjection, renderModule: this);

            return output;
        }

        private void TiledLighting(BoundingFrustum frustum, List<PointLight> pointLights, int cols, int rows)
        {
            if (TiledList == null || TiledList.Length != cols * rows)
            {
                TiledList = new int[cols*rows][];
                TiledListLength = new float[cols*rows];

                for (var index = 0; index < TiledList.Length; index++)
                {
                    TiledList[index] = new int[MAXLIGHTSPERTILE];
                }
            }
            
            if(_tileFrustum==null)
                _tileFrustum = new BoundingFrustumEx(frustum.Matrix);

            Vector3[] mainfrustumCorners = frustum.GetCorners();
            
            for (float col = 0; col < cols; col++)
            {
                for (float row = 0; row < rows; row++)
                {

                    //top left
                    _tileFrustumCorners[0] = mainfrustumCorners[0] 
                        + (mainfrustumCorners[1]-mainfrustumCorners[0]) * col / cols 
                        + (mainfrustumCorners[3] - mainfrustumCorners[0]) * row / rows;

                    //top right
                    _tileFrustumCorners[1] = mainfrustumCorners[0] 
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col + 1) / cols
                        + (mainfrustumCorners[3] - mainfrustumCorners[0]) * row / rows; 


                    //bot right
                    _tileFrustumCorners[2] = mainfrustumCorners[0] 
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col + 1) / cols
                        + (mainfrustumCorners[2] - mainfrustumCorners[1]) * (row + 1) / rows;

                    //bot left
                    _tileFrustumCorners[3] = mainfrustumCorners[0]
                        + (mainfrustumCorners[1] - mainfrustumCorners[0]) * (col) / cols
                        + (mainfrustumCorners[2] - mainfrustumCorners[1]) * (row + 1) / rows;
                    
                    _tileFrustumCorners[4] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * col / cols
                                             + (mainfrustumCorners[7] - mainfrustumCorners[4]) * row / rows;
                    
                    _tileFrustumCorners[5] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col + 1) / cols
                                             + (mainfrustumCorners[7] - mainfrustumCorners[4]) * row / rows;

                    
                    _tileFrustumCorners[6] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col + 1) / cols
                                             + (mainfrustumCorners[6] - mainfrustumCorners[5]) * (row + 1) / rows;
                    
                    _tileFrustumCorners[7] = mainfrustumCorners[4]
                                             + (mainfrustumCorners[5] - mainfrustumCorners[4]) * (col) / cols
                                             + (mainfrustumCorners[6] - mainfrustumCorners[5]) * (row + 1) / rows;
                    
                    _tileFrustum.SetCorners(ref _tileFrustumCorners);
                    _tileFrustum.CreatePlanesFromCorners();

                    //Now we are ready to frustum cull... phew

                    int index = (int)(row * cols + col);

                    int numberOfLightsInTile = 0;

                    for (var i = 0; i < pointLights.Count; i++)
                    {
                        var pointLight = pointLights[i];
                        ContainmentType containmentType = _tileFrustum.Contains(pointLight.BoundingSphere);

                        if (containmentType == ContainmentType.Intersects ||
                            containmentType == ContainmentType.Contains)
                        {
                            TiledList[index][numberOfLightsInTile] = i;
                            numberOfLightsInTile++;
                        }

                        if (numberOfLightsInTile >= MAXLIGHTSPERTILE) break;
                    }

                    TiledListLength[index] = numberOfLightsInTile;

#if SHOWTILES
                    LineHelperManager.AddFrustum(_tileFrustum, 1,numberOfLightsInTile > 1 ? Color.Red : numberOfLightsInTile > 0 ? Color.Blue : Color.Green);
#endif
                }
            }

            //Note: This needs a custom monogame version, since the default doesn't like to pass int[];
            _tiledListLengthParam.SetValue(TiledListLength);
        }

        private void SetupLighting(Camera camera, List<PointLight> pointLights, BoundingFrustum frustum)
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
            }

            //Fill
            int lightsInBounds = 0;

            for (var index = 0; index < count; index++)
            {
                PointLight light = pointLights[index];
                
                //Check frustum culling
                if (frustum.Contains(light.BoundingSphere) == ContainmentType.Disjoint) continue;
                
                LightPositionWS[lightsInBounds] = light.Position;
                LightColor[lightsInBounds] = light.ColorV3;
                LightIntensity[lightsInBounds] = light.Intensity;
                LightRadius[lightsInBounds] = light.Radius;
                lightsInBounds++;
            }

            _lightAmountParam.SetValue(lightsInBounds);

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
