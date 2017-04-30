using System;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.PostProcessingFilters
{
    /// <summary>
    /// Bloom / Blur, 2016 TheKosmonaut
    /// 
    /// High-Quality Bloom filter for high-performance applications
    /// 
    /// Based largely on the implementations in Unreal Engine 4 and Call of Duty AW
    /// For more information look for
    /// "Next Generation Post Processing in Call of Duty Advanced Warfare" by Jorge Jimenez
    /// http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx
    /// 
    /// The idea is to have several rendertargets or one rendertarget with several mip maps
    /// so each mip has half resolution (1/2 width and 1/2 height) of the previous one.
    /// 
    /// 32, 16, 8, 4, 2
    /// 
    /// In the first step we extract the bright spots from the original image. If not specified otherwise thsi happens in full resolution.
    /// We can do that based on the average RGB value or Luminance and check whether this value is higher than our Threshold.
    ///     BloomUseLuminance = true / false (default is true)
    ///     BloomThreshold = 0.8f;
    /// 
    /// Then we downscale this extraction layer to the next mip map.
    /// While doing that we sample several pixels around the origin.
    /// We continue to downsample a few more times, defined in
    ///     BloomDownsamplePasses = 5 ( default is 5)
    /// 
    /// Afterwards we upsample again, but blur in this step, too.
    /// The final output should be a blur with a very large kernel and smooth gradient.
    /// 
    /// The output in the draw is only the blurred extracted texture. 
    /// It can be drawn on top of / merged with the original image with an additive operation for example.
    /// 
    /// If you use ToneMapping you should apply Bloom before that step.
    /// </summary>
    public class BloomFilter : IDisposable
    {
        #region fields & properties

        #region private fields

        //resolution
        private int _width;
        private int _height;

        //RenderTargets
        private RenderTarget2D _bloomRenderTarget2DMip0;
        private RenderTarget2D _bloomRenderTarget2DMip1;
        private RenderTarget2D _bloomRenderTarget2DMip2;
        private RenderTarget2D _bloomRenderTarget2DMip3;
        private RenderTarget2D _bloomRenderTarget2DMip4;
        private RenderTarget2D _bloomRenderTarget2DMip5;

        //Objects
        private GraphicsDevice _graphicsDevice;
        private FullScreenTriangle _fullScreenTriangle;

        //Shader + variables
        private Effect _bloomEffect;

        private EffectPass _bloomPassExtract;
        private EffectPass _bloomPassExtractLuminance;
        private EffectPass _bloomPassDownsample;
        private EffectPass _bloomPassUpsample;

        private EffectParameter _bloomParameterScreenTexture;
        private EffectParameter _bloomInverseResolutionParameter;
        private EffectParameter _bloomRadiusParameter;
        private EffectParameter _bloomStrengthParameter;
        private EffectParameter _bloomStreakLengthParameter;
        private EffectParameter _bloomThresholdParameter;

        //Preset variables for different mip levels
        private float _bloomRadius1 = 1.0f;
        private float _bloomRadius2 = 1.0f;
        private float _bloomRadius3 = 1.0f;
        private float _bloomRadius4 = 1.0f;
        private float _bloomRadius5 = 1.0f;

        private float _bloomStrength1 = 1.0f;
        private float _bloomStrength2 = 1.0f;
        private float _bloomStrength3 = 1.0f;
        private float _bloomStrength4 = 1.0f;
        private float _bloomStrength5 = 1.0f;
        
        private float _radiusMultiplier = 1.0f;


        #endregion

        #region public fields + enums

        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        //enums
        public enum BloomPresets
        {
            Wide,
            Focussed,
            Small,
            SuperWide,
            Cheap
        };

        #endregion

        #region properties
        public BloomPresets BloomPreset
        {
            get { return _bloomPreset; }
            set
            {
                if (_bloomPreset == value) return;

                _bloomPreset = value;
                SetBloomPreset(_bloomPreset);
            }
        }
        private BloomPresets _bloomPreset;


        private Texture2D BloomScreenTexture { set { _bloomParameterScreenTexture.SetValue(value); } }
        private Vector2 BloomInverseResolution
        {
            get { return _bloomInverseResolutionField; }
            set
            {
                if (value != _bloomInverseResolutionField)
                {
                    _bloomInverseResolutionField = value;
                    _bloomInverseResolutionParameter.SetValue(_bloomInverseResolutionField);
                }
            }
        }
        private Vector2 _bloomInverseResolutionField;

        private float BloomRadius
        {
            get
            {
                return _bloomRadius;
            }

            set
            {
                if (Math.Abs(_bloomRadius - value) > 0.001f)
                {
                    _bloomRadius = value;
                    _bloomRadiusParameter.SetValue(_bloomRadius * _radiusMultiplier);
                }

            }
        }
        private float _bloomRadius;

        private float BloomStrength
        {
            get { return _bloomStrength; }
            set
            {
                if (Math.Abs(_bloomStrength - value) > 0.001f)
                {
                    _bloomStrength = value;
                    _bloomStrengthParameter.SetValue(_bloomStrength);
                }

            }
        }
        private float _bloomStrength;

        public float BloomStreakLength
        {
            get { return _bloomStreakLength; }
            set
            {
                if (Math.Abs(_bloomStreakLength - value) > 0.001f)
                {
                    _bloomStreakLength = value;
                    _bloomStreakLengthParameter.SetValue(_bloomStreakLength);
                }
            }
        }
        private float _bloomStreakLength;

        public float BloomThreshold
        {
            get { return _bloomThreshold; }
            set {
                if (Math.Abs(_bloomThreshold - value) > 0.001f)
                {
                    _bloomThreshold = value;
                    _bloomThresholdParameter.SetValue(_bloomThreshold);
                }
            }
        }
        private float _bloomThreshold;

        #endregion

        #endregion

        #region initialize

        //Initialize graphicsDevice
        public void Initialize(GraphicsDevice graphicsDevice, int width, int height, FullScreenTriangle fullScreenTriangle)
        {
            _fullScreenTriangle = fullScreenTriangle;

            _graphicsDevice = graphicsDevice;
            UpdateResolution(width, height);

        }

        /// <summary>
        /// Loads all needed components for the BloomEffect. This effect won't work without calling load
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="content"></param>
        /// <param name="width">initial value for creating the rendertargets</param>
        /// <param name="height">initial value for creating the rendertargets</param>
        /// <param name="quadRenderer">if you already have quadRenderer you may reuse it here</param>
        public void Load( ContentManager content)
        {
            //Load the shader parameters and passes for cheap and easy access
            _bloomEffect = content.Load<Effect>("Shaders/BloomFilter/Bloom");
            _bloomInverseResolutionParameter = _bloomEffect.Parameters["InverseResolution"];
            _bloomRadiusParameter = _bloomEffect.Parameters["Radius"];
            _bloomStrengthParameter = _bloomEffect.Parameters["Strength"];
            _bloomStreakLengthParameter = _bloomEffect.Parameters["StreakLength"];
            _bloomThresholdParameter = _bloomEffect.Parameters["Threshold"];
            _bloomParameterScreenTexture = _bloomEffect.Parameters["ScreenTexture"];

            _bloomPassExtract = _bloomEffect.Techniques["Extract"].Passes[0];
            _bloomPassExtractLuminance = _bloomEffect.Techniques["ExtractLuminance"].Passes[0];
            _bloomPassDownsample = _bloomEffect.Techniques["Downsample"].Passes[0];
            _bloomPassUpsample = _bloomEffect.Techniques["Upsample"].Passes[0];
            
            //An interesting blendstate for merging the initial image with the bloom.
            //BlendStateBloom = new BlendState();
            //BlendStateBloom.ColorBlendFunction = BlendFunction.Add;
            //BlendStateBloom.ColorSourceBlend = Blend.BlendFactor;
            //BlendStateBloom.ColorDestinationBlend = Blend.BlendFactor;
            //BlendStateBloom.BlendFactor = new Color(0.5f, 0.5f, 0.5f);

            //Default threshold.
            BloomThreshold = 0.8f;
            //Setup the default preset values.
            BloomPreset = BloomPresets.SuperWide;
            SetBloomPreset(BloomPreset);

            BloomDownsamplePasses = 5;
        }

        /// <summary>
        /// A few presets with different values for the different mip levels of our bloom.
        /// </summary>
        /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
        private void SetBloomPreset(BloomPresets preset)
        {
            switch(preset)
            {
                case BloomPresets.Wide:
                {
                        _bloomStrength1 = 0.5f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 2;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 4.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 1.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                }
                case BloomPresets.SuperWide:
                    {
                        _bloomStrength1 = 0.9f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 2;
                        _bloomStrength5 = 6;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Focussed:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Small:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 1;
                        _bloomRadius5 = 1;
                        _bloomRadius4 = 1;
                        _bloomRadius3 = 1;
                        _bloomRadius2 = 1;
                        _bloomRadius1 = 1;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Cheap:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 2;
                        _bloomRadius2 = 2;
                        _bloomRadius1 = 2;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 2;
                        break;
                    }
            }
        }

        #endregion

        /// <summary>
        /// Main draw function
        /// </summary>
        /// <param name="inputTexture">the image from which we want to extract bright parts and blur these</param>
        /// <param name="width">width of our target. If different to the input.Texture width our final texture will be smaller/larger.
        /// For example we can use half resolution. Input: 1280px wide -> width = 640px
        /// The smaller this value the better performance and the worse our final image quality</param>
        /// <param name="height">see: width</param>
        /// <returns></returns>
        public Texture2D Draw(Texture2D inputTexture, int width, int height)
        { 
            //Check if we are initialized
            if(_graphicsDevice==null)
                throw new Exception("Module not yet Loaded / Initialized. Use Load() first");

            ApplyGameSettings();

            //Change renderTarget resolution if different from what we expected. If lower than the inputTexture we gain performance.
            if (width != _width || height != _height)
            {
                UpdateResolution(width, height);

                //Adjust the blur so it looks consistent across diferrent scalings
                _radiusMultiplier = (float)width / inputTexture.Width;
                
                //Update our variables with the multiplier
                SetBloomPreset(BloomPreset);
            }

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;

            //EXTRACT  //Note: Is setRenderTargets(binding better?)
            //We extract the bright values which are above the Threshold and save them to Mip0
            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);

            BloomScreenTexture = inputTexture;
            BloomInverseResolution = new Vector2(1.0f / _width, 1.0f / _height);
            
            if (BloomUseLuminance) _bloomPassExtractLuminance.Apply(); 
            else _bloomPassExtract.Apply();
            _fullScreenTriangle.Draw(_graphicsDevice);
            
            //Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                BloomInverseResolution *= 2;
                //DOWNSAMPLE TO MIP1
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);

                BloomScreenTexture = _bloomRenderTarget2DMip0;
                //Pass
                _bloomPassDownsample.Apply();
                _fullScreenTriangle.Draw(_graphicsDevice);

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halfed, so our inverse 1/res. must be doubled
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);

                    BloomScreenTexture = _bloomRenderTarget2DMip1;
                    //Pass
                    _bloomPassDownsample.Apply();
                    _fullScreenTriangle.Draw(_graphicsDevice);

                    if (BloomDownsamplePasses > 2)
                    {
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);

                        BloomScreenTexture = _bloomRenderTarget2DMip2;
                        //Pass
                        _bloomPassDownsample.Apply();
                        _fullScreenTriangle.Draw(_graphicsDevice);
                        
                        if (BloomDownsamplePasses > 3)
                        {
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);

                            BloomScreenTexture = _bloomRenderTarget2DMip3;
                            //Pass
                            _bloomPassDownsample.Apply();
                            _fullScreenTriangle.Draw(_graphicsDevice);

                            if (BloomDownsamplePasses > 4)
                            {
                                BloomInverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip5);

                                BloomScreenTexture = _bloomRenderTarget2DMip4;
                                //Pass
                                _bloomPassDownsample.Apply();
                                _fullScreenTriangle.Draw(_graphicsDevice);

                                ChangeBlendState();

                                //UPSAMPLE TO MIP4
                                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip4);
                                BloomScreenTexture = _bloomRenderTarget2DMip5;
                                
                                BloomStrength = _bloomStrength5;
                                BloomRadius = _bloomRadius5;
                                _bloomPassUpsample.Apply();
                                _fullScreenTriangle.Draw(_graphicsDevice);

                                BloomInverseResolution /= 2;
                            }
                            
                            ChangeBlendState();

                            //UPSAMPLE TO MIP3
                            _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip3);
                            BloomScreenTexture = _bloomRenderTarget2DMip4;

                            BloomStrength = _bloomStrength4;
                            BloomRadius = _bloomRadius4;
                            _bloomPassUpsample.Apply();
                            _fullScreenTriangle.Draw(_graphicsDevice);

                            BloomInverseResolution /= 2;

                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP2
                        _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip2);
                        BloomScreenTexture = _bloomRenderTarget2DMip3;

                        BloomStrength = _bloomStrength3;
                        BloomRadius = _bloomRadius3;
                        _bloomPassUpsample.Apply();
                        _fullScreenTriangle.Draw(_graphicsDevice);

                        BloomInverseResolution /= 2;

                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP1
                    _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip1);
                    BloomScreenTexture = _bloomRenderTarget2DMip2;

                    BloomStrength = _bloomStrength2;
                    BloomRadius = _bloomRadius2;
                    _bloomPassUpsample.Apply();
                    _fullScreenTriangle.Draw(_graphicsDevice);

                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP0
                _graphicsDevice.SetRenderTarget(_bloomRenderTarget2DMip0);
                BloomScreenTexture = _bloomRenderTarget2DMip1;

                BloomStrength = _bloomStrength1;
                BloomRadius = _bloomRadius1;

                _bloomPassUpsample.Apply();
                _fullScreenTriangle.Draw(_graphicsDevice);
            }

            //Note the final step could be done as a blend to the final texture.
            
            return _bloomRenderTarget2DMip0;
        }

        private void ApplyGameSettings()
        {
            _bloomRadius1 = GameSettings.g_BloomRadius1;
            _bloomRadius2 = GameSettings.g_BloomRadius2;
            _bloomRadius3 = GameSettings.g_BloomRadius3;
            _bloomRadius4 = GameSettings.g_BloomRadius4;
            _bloomRadius5 = GameSettings.g_BloomRadius5;

            _bloomStrength1 = GameSettings.g_BloomStrength1;
            _bloomStrength2 = GameSettings.g_BloomStrength2;
            _bloomStrength3 = GameSettings.g_BloomStrength3;
            _bloomStrength4 = GameSettings.g_BloomStrength4;
            _bloomStrength5 = GameSettings.g_BloomStrength5;

            BloomThreshold = GameSettings.g_BloomThreshold * 0.1f;
        }

        private void ChangeBlendState()
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// Update the InverseResolution of the used rendertargets. This should be the InverseResolution of the processed image
        /// We use SurfaceFormat.Color, but you can use higher precision buffers obviously.
        /// </summary>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        public void UpdateResolution(int width, int height)
        {
            _width = width;
            _height = height;

            if (_bloomRenderTarget2DMip0 != null)
            {
                Dispose();
            }

            _bloomRenderTarget2DMip0 = new RenderTarget2D(_graphicsDevice,
                (int) (width),
                (int) (height), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip1 = new RenderTarget2D(_graphicsDevice,
                (int) (width/2),
                (int) (height/2), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip2 = new RenderTarget2D(_graphicsDevice,
                (int) (width/4),
                (int) (height/4), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip3 = new RenderTarget2D(_graphicsDevice,
                (int) (width/8),
                (int) (height/8), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip4 = new RenderTarget2D(_graphicsDevice,
                (int) (width/16),
                (int) (height/16), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _bloomRenderTarget2DMip5 = new RenderTarget2D(_graphicsDevice,
                (int) (width/32),
                (int) (height/32), false, SurfaceFormat.HalfVector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        /// <summary>
        //Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
        /// </summary>
        public void Dispose()
        {
            _bloomRenderTarget2DMip0?.Dispose();
            _bloomRenderTarget2DMip1?.Dispose();
            _bloomRenderTarget2DMip2?.Dispose();
            _bloomRenderTarget2DMip3?.Dispose();
            _bloomRenderTarget2DMip4?.Dispose();
            _bloomRenderTarget2DMip5?.Dispose();
            _graphicsDevice?.Dispose();
            _bloomEffect?.Dispose();
        }
    }
}
