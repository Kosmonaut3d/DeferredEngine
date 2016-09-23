using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Main;
using EngineTest.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineTest.Renderer.Helper
{
    public class DebugScreen
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _sprFont;

        //private ScreenManager.ScreenStates _state;

        private static readonly List<string> StringList = new List<string>();
        public static readonly List<StringColor> AiDebugString = new List<StringColor>();
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

        // Console
        public static bool ConsoleOpen;
        private string _consoleString = "";
        private List<string> _consoleStringSuggestion = new List<string>();
        private Comparer<string> stringCompare = new SampleComparator();
        public static int ActiveLights = 0;

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
        }

        public void UnloadContent()
        {

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
                foreach (PropertyInfo info in infos)
                {
                    if (info.Name.Contains(_consoleString.Split(' ')[0]))
                    {
                        _consoleStringSuggestion.Add(info.Name + " (" + info.PropertyType.ToString().Substring(7) + ")  :  " + info.GetValue(null));

                        //return;
                    }
                }
                FieldInfo[] info2 = typeof(GameSettings).GetFields();
                foreach (FieldInfo info in info2)
                {
                    if (info.Name.Contains(_consoleString.Split(' ')[0]))
                    {
                        _consoleStringSuggestion.Add(info.Name + " (" + info.FieldType.ToString().Substring(7) + ")  :  " + info.GetValue(null));
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
                        string type = propinfo.PropertyType.ToString();
                        switch (type)
                        {
                            case "System.Double":
                                {
                                    value = Convert.ToDouble(cmds[1]);
                                    break;
                                }
                            case "System.Single":
                                {
                                    value = Convert.ToSingle(cmds[1]);
                                    break;
                                }
                            case "System.Int32":
                                {
                                    value = Convert.ToInt32(cmds[1]);
                                    break;
                                }
                            case "System.Boolean":
                                {
                                    value = Convert.ToBoolean(cmds[1]);
                                    break;
                                }
                        }
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
            if (GameSettings.ShowDisplayInfo > 0 || ConsoleOpen)
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

                _spriteBatch.Begin(SpriteSortMode.BackToFront);

                if (ConsoleOpen)
                {
                    Color consoleColor = Color.White;
                    if (_consoleErrorTimer > 0)
                    {
                        _consoleErrorTimer -= gameTime.ElapsedGameTime.Milliseconds;
                        consoleColor = Color.Lerp(Color.White, Color.Red, _consoleErrorTimer / ConsoleErrorTimerMax);
                    }
                    char ins = '*';
                    //CONSOLE
                    if (_offFrame)
                    {
                        ins = ' ';
                    }
                    _spriteBatch.DrawString(_sprFont,
                        "CONSOLE: " + _consoleString + ins,
                        new Vector2(10.0f, 105.0f), consoleColor);
                    Vector2 strLength = _sprFont.MeasureString("CONSOLE: " + _consoleString + ins);

                    for (int index = 0; index < _consoleStringSuggestion.Count; index++)
                    {
                        string suggestion = _consoleStringSuggestion[index];
                        _spriteBatch.DrawString(_sprFont, suggestion,
                            new Vector2(10.0f + strLength.X, 105.0f + strLength.Y * index), consoleColor);
                    }
                }


                float y = 80f;
                for (var i = 0; i < StringList.Count; i++)
                {
                    y += 15f;
                    _spriteBatch.DrawString(_sprFont, StringList[i],
                        new Vector2(10.0f, y), Color.White);
                }

                StringList.Clear();

                if (GameSettings.ShowDisplayInfo == 1) //most basic, only show fps
                {
                    _spriteBatch.DrawString(_sprFont,
                    string.Format(Math.Round(_smoothfps).ToString()),
                    new Vector2(10.0f, 10.0f), Color.White);
                }

                if (GameSettings.ShowDisplayInfo <= 1)
                {
                    _spriteBatch.End();
                    StringList.Clear();
                    return;
                }

                _spriteBatch.DrawString(_sprFont,
                    string.Format("Threads - Main: " + Math.Round(gameTime.ElapsedGameTime.TotalMilliseconds, 2) + " ms "),
                    new Vector2(10.0f, 10.0f), Color.White);

                _spriteBatch.DrawString(_sprFont,
                    " (FPS: " + Math.Round(_fps) + " ... " + Math.Round(_smoothfpsShow) + " > " + Math.Round(_minfps) + ")",
                    new Vector2(160.0f, 10.0f), Color.White);

                long totalmemory = GC.GetTotalMemory(false);
                if (_maxGcMemory < totalmemory) _maxGcMemory = totalmemory;
                _spriteBatch.DrawString(_sprFont, GameSettings.g_ScreenWidth +" x " + GameSettings.g_ScreenHeight + " | Memory (GC): " + totalmemory / 1024 + " ... " + _maxGcMemory / 1024, new Vector2(10, 25),
                    Color.White);

                // HELPERS

                if (GameSettings.ShowDisplayInfo <= 2)
                {
                    _spriteBatch.End();
                    StringList.Clear();
                    return;
                }

                _spriteBatch.DrawString(_sprFont,
                    string.Format("meshes: " + GameStats.MeshDraws + " materials: " + GameStats.MaterialDraws + " lights: " + GameStats.LightsDrawn
                    + " shadowMaps: " +GameStats.activeShadowMaps + "/"+ GameStats.shadowMaps ),
                    new Vector2(10.0f, 40.0f), Color.White);

                _spriteBatch.End();
            }
            else
            {

                StringList.Clear();
            }


        }

        public void UpdateResolution()
        {
            //
        }

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

        private static List<StringColorPosition> _aiStringColorPosition = new List<StringColorPosition>();
        private GraphicsDevice _graphicsDevice;

        public static void AddAiString(string info, Color color)
        {
            AiDebugString.Add(new StringColor(info, color));
        }

        private static bool _clearCommand;

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
                            output = Convert.ToDouble(input);
                            break;
                        }
                    case "System.Single":
                        {
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
    }

    class SampleComparator : Comparer<String>
    {

        public override int Compare(string x, string y)
        {
            return x.Length - y.Length;
        }
    }
}
