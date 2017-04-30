using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeferredEngine.Logic
{
    public class DebugScreen : IDisposable
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _sprFont;
        private SpriteFont _monospaceFont;

        private readonly MngStringBuilder _mngStringBuilder = new MngStringBuilder(2048);

        //private ScreenManager.ScreenStates _state;

        private static readonly List<string> StringList = new List<string>();
        public static readonly List<StringColor> AiDebugString = new List<StringColor>();

        public Color consoleColor = Color.Coral;

        //private GraphicsDevice _graphicsDevice;

        private long _maxGcMemory;

        private double _fps;
        private double _smoothfps = 60;
        private double _frame;
        private double _smoothfpsShow = 60;
        private double _minfps = 1000;
        private double _minfpsshort = 1000;
        private int _minfpstick;

        private bool _offFrame = true;
        private static bool _clearCommand;


        //SB

        private readonly StringBuilder sb_frameTime = new StringBuilder("Threads - Main: ");
        private readonly StringBuilder sb_ms = new StringBuilder(" ms ");

        private readonly StringBuilder sb_fps = new StringBuilder(" (FPS: ");
        private readonly StringBuilder sb_dotdotdot = new StringBuilder(" ... ");
        private readonly StringBuilder sb_greaterthan = new StringBuilder(" > ");
        private readonly StringBuilder sb_closeBracket = new StringBuilder(")");
        private readonly StringBuilder sb_multipliedBy = new StringBuilder(" x ");
        private readonly StringBuilder sb_emptySpace = new StringBuilder(" ");
        private readonly StringBuilder sb_memoryGc = new StringBuilder(" | Memory(GC): ");

        private readonly StringBuilder sb_meshes = new StringBuilder("\nmeshes: ");
        private readonly StringBuilder sb_materials = new StringBuilder(" materials: ");
        private readonly StringBuilder sb_lights = new StringBuilder(" lights: ");
        private readonly StringBuilder sb_emissive = new StringBuilder(" emissive: ");
        private readonly StringBuilder sb_shadowmaps = new StringBuilder(" shadowmaps: ");
        private readonly StringBuilder sb_slash = new StringBuilder("/");
        private readonly StringBuilder sb_sdfgeneration = new StringBuilder("\n sdf generation: ");

        public StringBuilder[] RenderModesSB =
        {
            new StringBuilder( ((Renderer.Renderer.RenderModes)0).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)1).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)2).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)3).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)4).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)5).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)6).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)7).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)8).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)9).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)10).ToString() ),
            new StringBuilder( ((Renderer.Renderer.RenderModes)11).ToString() ),
        };

        // Console
        public static bool ConsoleOpen;
        private string _consoleString = "";
        private readonly List<string> _consoleStringSuggestion = new List<string>();
        private readonly Comparer<string> stringCompare = new SampleComparator();

        private float _consoleErrorTimer;
        private const float ConsoleErrorTimerMax = 500;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            //_state = state;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _graphicsDevice = graphicsDevice;
            
        }

        public void LoadContent(ContentManager content)
        {
            _sprFont = content.Load<SpriteFont>("Fonts/defaultFont");
            _monospaceFont = content.Load<SpriteFont>("Fonts/monospace");
        }

        public void Update(GameTime gameTime)
        {
            _offFrame = gameTime.TotalGameTime.Milliseconds % 1000 <= 500;

            if (Input.WasKeyPressed(Keys.OemPipe))
            {
                ConsoleOpen = !ConsoleOpen;
                _consoleString = "";
            }
            else if (ConsoleOpen)
            {
                if (Input.WasKeyPressed(Keys.Back))
                {
                    if (_consoleString.Length > 0)
                        _consoleString = _consoleString.Remove(_consoleString.Length - 1);
                }

                if (Input.WasKeyPressed(Keys.Enter))
                {
                    if (UseConsoleCommand())
                    {
                        _consoleString = "";
                    }
                    else
                    {
                        _consoleErrorTimer = ConsoleErrorTimerMax;
                    }
                    return;
                }
                if (_consoleStringSuggestion.Count > 0 && Input.WasKeyPressed(Keys.Tab))
                {
                    _consoleString = _consoleStringSuggestion[0].Split(' ')[0];
                }
                else _consoleString += Input.GetKeyPressed();
            }

            _consoleStringSuggestion.Clear();
            if (_consoleString.Length > 0)
            {
                PropertyInfo[] infos = typeof(GameSettings).GetProperties();
                for (var index = 0; index < infos.Length; index++)
                {
                    PropertyInfo info = infos[index];
                    if (info.Name.Contains(_consoleString.Split(' ')[0]))
                    {
                        _consoleStringSuggestion.Add(info.Name + " (" + info.PropertyType.ToString().Substring(7) +
                                                     ")  :  " + info.GetValue(null));

                        //return;
                    }
                }
                FieldInfo[] info2 = typeof(GameSettings).GetFields();
                for (var index = 0; index < info2.Length; index++)
                {
                    FieldInfo info = info2[index];
                    if (info.Name.Contains(_consoleString.Split(' ')[0]))
                    {
                        _consoleStringSuggestion.Add(info.Name + " (" + info.FieldType.ToString().Substring(7) +
                                                     ")  :  " + info.GetValue(null));
                        //return;
                    }
                }

                _consoleStringSuggestion.Sort(stringCompare);
            }



        }

        private bool UseConsoleCommand()
        {
            string[] cmds = _consoleString.Split(' ');
            FieldInfo prop = typeof(GameSettings).GetField(cmds[0], BindingFlags.Public | BindingFlags.Static);

            if (cmds.Length < 2) return false;
            if (prop != null)
            {
                
                Object value = ConvertStringToType(cmds[1], prop.FieldType);
                
                if (cmds.Length > 0 && value != null)
                {
                    prop.SetValue(null, value);
                    return true;
                    //typeof (GameSettings).InvokeMember(cmds[0], BindingFlags.Public | BindingFlags.SetProperty,
                    //    Type.DefaultBinder, GameSettings, cmds);
                }
            }
            else
            {
                PropertyInfo propinfo = typeof(GameSettings).GetProperty(cmds[0], BindingFlags.Public | BindingFlags.Static);
                Object value = null;
                try
                {
                    if (propinfo != null)
                    {
                        //string type = propinfo.PropertyType.ToString();
                        value = ConvertStringToType(cmds[1], propinfo.PropertyType);
                    }
                }
                catch (Exception)
                {
                    value = null;
                }

                if (cmds.Length > 0 && propinfo != null && value != null)
                {
                    propinfo.SetValue(null, value);
                    return true;
                    //typeof (GameSettings).InvokeMember(cmds[0], BindingFlags.Public | BindingFlags.SetProperty,
                    //    Type.DefaultBinder, GameSettings, cmds);
                }

            }
            return false;
        }

        public void Draw(GameTime gameTime)
        {
            if (GameSettings.u_showdisplayinfo > 0 || ConsoleOpen)
            {
                _fps = 0.9 * _fps + 0.1 * (1000 / gameTime.ElapsedGameTime.TotalMilliseconds);

                _smoothfps = 0.98 * _smoothfps + 0.02 * _fps;
                if (gameTime.TotalGameTime.TotalMilliseconds - _frame > 500)
                {
                    _smoothfpsShow = _smoothfps;
                    _frame = gameTime.TotalGameTime.TotalMilliseconds;
                    _maxGcMemory -= 1024;

                    _minfpstick++;
                    if (_minfpstick == 4) // all 2 seconds
                    {
                        _minfpsshort = (_minfpsshort + _smoothfps) / 2;
                        _minfpstick = 0;
                    }
                }


                if (_fps < _minfpsshort && gameTime.TotalGameTime.TotalSeconds > 1) _minfpsshort = _fps;

                if (Math.Abs(_minfpsshort - _minfps) > 0.1f) _minfps = _minfpsshort;

                _spriteBatch.Begin();

                LoadProfilerStrings();

                if (ConsoleOpen)
                {
                    Color consoleColorLocal = this.consoleColor;
                    if (_consoleErrorTimer > 0)
                    {
                        _consoleErrorTimer -= gameTime.ElapsedGameTime.Milliseconds;
                        consoleColorLocal = Color.Lerp(Color.White, Color.Red, _consoleErrorTimer / ConsoleErrorTimerMax);
                    }
                    char ins = '*';
                    //CONSOLE
                    if (_offFrame)
                    {
                        ins = ' ';
                    }

                    //For console we don't care about performance
                    _spriteBatch.DrawString(_sprFont,
                        "CONSOLE: " + _consoleString + ins,
                        new Vector2(11.0f, 106.0f), Color.Black);
                    _spriteBatch.DrawString(_sprFont,
                        "CONSOLE: " + _consoleString + ins,
                        new Vector2(10.0f, 105.0f), consoleColorLocal);

                    Vector2 strLength = _sprFont.MeasureString("CONSOLE: " + _consoleString + ins);

                    for (int index = 0; index < _consoleStringSuggestion.Count; index++)
                    {
                        string suggestion = _consoleStringSuggestion[index];
                        _spriteBatch.DrawString(_sprFont, suggestion,
                            new Vector2(11.0f + strLength.X, 106.0f + strLength.Y * index), Color.Black);
                        _spriteBatch.DrawString(_sprFont, suggestion,
                            new Vector2(10.0f + strLength.X, 105.0f + strLength.Y * index), consoleColorLocal);
                    }

                    
                }


                float y = 65.0f;
                for (var i = 0; i < StringList.Count; i++)
                {
                    y += 15f;
                    _spriteBatch.DrawString(_sprFont, StringList[i],
                        new Vector2(10.0f, y), Color.White);
                }

                StringList.Clear();

                if (GameSettings.u_showdisplayinfo == 1) //most basic, only show fps
                {
                    _spriteBatch.DrawString(_sprFont,
                    string.Format(Math.Round(_smoothfps).ToString()),
                    new Vector2(10.0f, 10.0f), Color.White);
                }

                if (GameSettings.u_showdisplayinfo <= 1)
                {
                    _spriteBatch.End();
                    StringList.Clear();
                    return;
                }

                long totalmemory = GC.GetTotalMemory(false);
                if (_maxGcMemory < totalmemory) _maxGcMemory = totalmemory;
                
                //clear
                _mngStringBuilder.Length = 0;

                _mngStringBuilder.Append(sb_frameTime);
                _mngStringBuilder.AppendTrim(gameTime.ElapsedGameTime.TotalMilliseconds);
                _mngStringBuilder.Append(sb_ms);

                _mngStringBuilder.AppendAt(30, sb_fps);
                _mngStringBuilder.Append((int) Math.Round(_fps));
                _mngStringBuilder.Append(sb_dotdotdot);
                _mngStringBuilder.Append((int) Math.Round(_smoothfpsShow));
                _mngStringBuilder.Append(sb_greaterthan);
                _mngStringBuilder.Append((int) Math.Round(_minfps));
                _mngStringBuilder.AppendLine(sb_closeBracket);
                    
                _mngStringBuilder.Append(GameSettings.g_screenwidth);
                _mngStringBuilder.Append(sb_multipliedBy);
                _mngStringBuilder.Append(GameSettings.g_screenheight);
                _mngStringBuilder.Append(sb_emptySpace);
                _mngStringBuilder.Append(RenderModesToString( GameSettings.g_rendermode));
                _mngStringBuilder.Append(sb_memoryGc);
                _mngStringBuilder.Append(totalmemory/1024);
                _mngStringBuilder.Append(sb_dotdotdot);
                _mngStringBuilder.Append(_maxGcMemory/1024);

                _mngStringBuilder.Append(sb_meshes);
                _mngStringBuilder.Append(GameStats.MeshDraws);
                _mngStringBuilder.Append(sb_materials);
                _mngStringBuilder.Append(GameStats.MaterialDraws);
                _mngStringBuilder.Append(sb_lights);
                _mngStringBuilder.Append(GameStats.LightsDrawn);
                _mngStringBuilder.Append(sb_emissive);
                _mngStringBuilder.Append(GameStats.EmissiveMeshDraws);
                _mngStringBuilder.Append(sb_shadowmaps);
                _mngStringBuilder.Append(GameStats.activeShadowMaps);
                _mngStringBuilder.Append(sb_slash);
                _mngStringBuilder.Append(GameStats.shadowMaps);

                if (GameStats.sdf_load > 0)
                {
                    _mngStringBuilder.Append(sb_sdfgeneration);
                    _mngStringBuilder.Append(GameStats.sdf_load);
                }

                _spriteBatch.DrawString(_monospaceFont, _mngStringBuilder.StringBuilder,
                    new Vector2(11.0f, 11.0f), Color.Black);
                _spriteBatch.DrawString(_monospaceFont, _mngStringBuilder.StringBuilder,
                    new Vector2(10.0f, 10.0f), consoleColor);

                _spriteBatch.End();
            }
            else
            {

                StringList.Clear();
            }


        }
        
        /// <summary>
        /// We want to extract all the strings starting with d_profile
        /// </summary>
        private void LoadProfilerStrings()
        {
            if (!GameSettings.d_profiler) return;

            FieldInfo[] info2 = typeof(GameStats).GetFields();
            int foundIndex = 0;
            for (int index = 0; index < info2.Length; index++)
            {
                FieldInfo info = info2[index];
                if (info.Name.Contains("d_profile"))
                {
                    _spriteBatch.DrawString(_sprFont, info.Name +" "+ info.GetValue(null), new Vector2(10.0f, 55.0f + foundIndex*15),
                        Color.White);
                    foundIndex++;
                    //return;
                }
            }
        }


        //public void UpdateResolution()
        //{
        //    //
        //}

        public struct StringColor
        {
            public string String;
            public Color Color;

            public StringColor(string s, Color color)
            {
                String = s;
                Color = color;
            }
        }

        internal struct StringColorPosition
        {
            public StringColor StringColor;
            public Vector3 Position;

            public StringColorPosition(StringColor stringColor, Vector3 position)
            {
                StringColor = stringColor;
                Position = position;
            }
        }

        private static readonly List<StringColorPosition> _aiStringColorPosition = new List<StringColorPosition>();
        private GraphicsDevice _graphicsDevice;

        public static void AddAiString(string info, Color color)
        {
            AiDebugString.Add(new StringColor(info, color));
        }

        public static void ClearAiString()
        {
            _aiStringColorPosition.Clear();
            AiDebugString.Clear();
        }


        public static void AddString(string info)
        {
                StringList.Add(info);
        }

        public static void AddAiStringPosition(string info, Color color, Vector3 position)
        {
            _aiStringColorPosition.Add(new StringColorPosition(new StringColor(info, color), position));
        }


        public void DrawScreenSpace(Matrix projection, Matrix view)
        {
            
            if (_clearCommand)
            {
                AiDebugString.Clear();
                _aiStringColorPosition.Clear();
                _clearCommand = false;
            }

            try
            {
                _spriteBatch.Begin(SpriteSortMode.BackToFront);
                for (var i = 0; i < AiDebugString.Count; i++)
                {
                    if (AiDebugString[i].String != null)
                        _spriteBatch.DrawString(_sprFont, AiDebugString[i].String, new Vector2(800, i * 20),
                            AiDebugString[i].Color);
                }
                for (var i = 0; i < _aiStringColorPosition.Count; i++)
                {

                    if (i < _aiStringColorPosition.Count)
                        if (_aiStringColorPosition[i].StringColor.String != null)
                        {
                            Vector3 position = _graphicsDevice.Viewport.Project(_aiStringColorPosition[i].Position,
                                projection,
                                view, Matrix.Identity);

                            _spriteBatch.DrawString(_sprFont, _aiStringColorPosition[i].StringColor.String,
                                new Vector2(position.X, position.Y), _aiStringColorPosition[i].StringColor.Color);
                        }
                }
                _spriteBatch.End();
            }
            catch (Exception)
            {
                _spriteBatch.End();
                //don't care.
            }
        }

        public static object ConvertStringToType(string input, Type OutputType)
        {
            object output = null;
            string type = OutputType.ToString();
            try
            {
                switch (type)
                {
                    case "System.Double":
                        {
                            if (input.Contains('.'))
                                input = input.Replace('.', ',');
                            output = Convert.ToDouble(input);
                            break;
                        }
                    case "System.Single":
                    {
                        if(input.Contains('.'))
                            input = input.Replace('.', ',');
                        output = Convert.ToSingle(input);
                            break;
                        }
                    case "System.Int32":
                        {
                            output = Convert.ToInt32(input);
                            break;
                        }
                    case "System.Boolean":
                        {
                            output = Convert.ToBoolean(input);
                            break;
                        }
                }
            }
            catch (Exception)
            {
                output = null;
            }
            return output;
        }


        private StringBuilder RenderModesToString(Renderer.Renderer.RenderModes render)
        {
            return RenderModesSB[(int)render];
        }

        public void Dispose()
        {
            _spriteBatch?.Dispose();
            _graphicsDevice?.Dispose();
        }
    }

    class SampleComparator : Comparer<String>
    {

        public override int Compare(string x, string y)
        {
            return x.Length - y.Length;
        }
    }
}
