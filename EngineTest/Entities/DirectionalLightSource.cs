using System;
using EngineTest.Entities;
using EngineTest.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class DirectionalLightSource : TransformableObject
    {
        public Color Color;
        public float Intensity;
        private Vector3 _direction;
        private Vector3 _position;
        public bool HasChanged;

        public bool DrawShadows;
        public float ShadowSize;
        public float ShadowDepth;
        public int ShadowResolution;
        public bool StaticShadow;

        public RenderTarget2D ShadowMap;
        public Matrix ShadowViewProjection;

        public ShadowFilteringTypes ShadowFiltering;
        public bool ScreenSpaceShadowBlur;

        public Matrix LightViewProjection;

        public enum ShadowFilteringTypes
        {
            PCF, SoftPCF3x, SoftPCF5x, Poisson, VSM
        };

        /// <summary>
        /// Create a Directional light, shadows are optional
        /// </summary>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="direction"></param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowSize"></param>
        /// <param name="shadowDepth"></param>
        /// <param name="shadowResolution"></param>
        public DirectionalLightSource(Color color, float intensity, Vector3 direction,Vector3 position = default(Vector3), bool drawShadows = false, float shadowSize = 100, float shadowDepth = 100, int shadowResolution = 512, ShadowFilteringTypes shadowFiltering = ShadowFilteringTypes.Poisson, bool screenspaceshadowblur = false, bool staticshadows = false)
        {
            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;

            DrawShadows = drawShadows;

            DrawShadows = drawShadows;
            ShadowSize = shadowSize;
            ShadowDepth = shadowDepth;
            ShadowResolution = shadowResolution;
            StaticShadow = staticshadows;

            ScreenSpaceShadowBlur = screenspaceshadowblur;

            ShadowFiltering = shadowFiltering;

            Position = position;

            Id = IdGenerator.GetNewId();

            TransformDirectionToAngles();
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                HasChanged = true;
            }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                HasChanged = true;
            }
        }

        private int _id;

        public override int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private double _angleZ = 0;
        public override double AngleZ {
            get
            {
                return _angleZ;
            }
            set
            {
                TransformAnglesToDirection(0,0, (float)(value - _angleZ));
                _angleZ = value;
            }
        }

        private double _angleX = 0;
        public override double AngleX {
            get
            {
                return _angleX;
            }
            set
            {
                TransformAnglesToDirection((float)(value - _angleX), 0,0);
                _angleX = value;
            }
        }

        private double _angleY = 0;

        public override double AngleY
        {
            get
            {
                return _angleY;
            }
            set
            {
                TransformAnglesToDirection(0,(float)(value-_angleY), 0);
                _angleY = value;
            }
        }

        private void TransformAnglesToDirection(float angleX, float angleY, float angleZ)
        {
            RotationMatrix = Matrix.CreateRotationX((float)angleX) * Matrix.CreateRotationY((float)angleY) *
                                  Matrix.CreateRotationZ((float)angleZ);

            
            Direction = Vector3.Transform(Direction, RotationMatrix);
            
            return;
        }

        private Matrix Trafo;
        private void TransformDirectionToAngles()
        {
            Trafo = Matrix.CreateLookAt(Vector3.Zero, Direction, Vector3.UnitZ);
        }

        public Matrix RotationMatrix;

        public override TransformableObject Clone
        {
            get { return new DirectionalLightSource(Color, Intensity, Direction, Position, DrawShadows, ShadowSize, ShadowDepth, ShadowResolution, ShadowFiltering, false, StaticShadow); }
        }

        public virtual void ApplyShader()
        {
            if (DrawShadows)
            {
                //Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                //Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(shadowMap);
                if (ScreenSpaceShadowBlur)
                {
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightSSShadowed.Passes[0].Apply();  
                }
                else
                {
                    Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                    Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(ShadowMap);
                    Shaders.deferredDirectionalLightParameter_ShadowFiltering.SetValue((int)ShadowFiltering);
                    Shaders.deferredDirectionalLightParameter_ShadowMapSize.SetValue((float)ShadowResolution);
                    Shaders.deferredDirectionalLightShadowed.Passes[0].Apply();   
                }
            }
            else
            {
                Shaders.deferredDirectionalLightUnshadowed.Passes[0].Apply();  
            }
        }
    }
}
