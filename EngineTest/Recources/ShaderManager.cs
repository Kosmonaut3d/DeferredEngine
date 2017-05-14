/*
    HotSwap shader sytem for MonoGame

    https://gist.githubusercontent.com/jackmott/98690081046e2c49387e49794b8061a7/raw/3d5c6bb115c97d31dfb8eb16a9e2998a07de3bd5/ShaderManager.cs

    originally from jackmott

    HotSwap code only exists for debug builds
    Edit paths to match your project
    Construct in your Initialize method
    Add shaders in LoadContent (or whenever)
    Call CheckForChanges in Update() or periodically however you like
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using HelperSuite.ContentLoader;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{


    public class ShaderManager
    {

#if DEBUG
        ContentManager TempContent;
        DateTime LastUpdate;
        const string mgcbPathExe = "C:/program Files (x86)/MSBuild/MonoGame/v3.0/Tools/mgcb.exe";
#endif

        string contentBuiltPath;
        string contentExecutablePath;
        public List<ShaderDefinition> ShaderCollection = new List<ShaderDefinition>();
        ContentManager Content;
        GraphicsDevice Device;

        public class ShaderDefinition
        {
            public Effect Effect;
            public string Path;
            public string CompletePath;
            public bool HasChanged;

            public ShaderDefinition(Effect effect, string path, string completePath)
            {
                Effect = effect;
                Path = path;
                CompletePath = completePath;
                HasChanged = false;
            }
            
        }

        public ShaderManager(ContentManager content, GraphicsDevice device)
        {
            contentBuiltPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))) + "/Content/";
            contentExecutablePath = Directory.GetCurrentDirectory() + "/Content/";

            contentBuiltPath = contentBuiltPath.Replace("\\", "/");
            contentExecutablePath = contentExecutablePath.Replace("\\", "/");

            Content = content;
            Device = device;
#if DEBUG
            TempContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            LastUpdate = DateTime.Now;
#endif
        }

#if DEBUG
        public void CheckForChanges()
        {
#if DEBUG
            if (!GameSettings.d_hotreloadshaders) return;

            for (var index = 0; index < ShaderCollection.Count; index++)
            {
                var shaderDefs = ShaderCollection[index];
                var t = File.GetLastWriteTime(shaderDefs.CompletePath);
                if (t > LastUpdate)
                {
                    ShaderChanged(shaderDefs, index);
                    LastUpdate = t;
                }
            }
#else
            return;
#endif
        }
        public void ShaderChanged(ShaderDefinition shaderDefinition, int index)
        {
            string name = shaderDefinition.Path;
            Process pProcess = new Process
            {
                StartInfo =
                {
                    FileName = mgcbPathExe,
                    Arguments = "/importer:EffectImporter /processor:EffectProcessor /processorParam:DebugMode=Auto /build:"+name+".fx",
                    CreateNoWindow = true,
                    WorkingDirectory = contentBuiltPath,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            //Get program output
            string stdError = null;
            StringBuilder stdOutput = new StringBuilder();
            pProcess.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);

            try
            {
                pProcess.Start();
                pProcess.BeginOutputReadLine();
                stdError = pProcess.StandardError.ReadToEnd();
                pProcess.WaitForExit();

                string builtPath = contentBuiltPath + "/" + name + ".xnb";
                string movePath = contentExecutablePath + name + ".xnb";
                File.Copy(builtPath, movePath, true);
                File.Delete(builtPath);

                ContentManager newTemp = new ContentManager(TempContent.ServiceProvider, TempContent.RootDirectory);
               
                shaderDefinition.Effect.Dispose();
                shaderDefinition.Effect = newTemp.Load<Effect>(shaderDefinition.Path);

                shaderDefinition.HasChanged = true;

                ShaderCollection[index] = shaderDefinition;
            }
            catch (Exception e)
            {
                //todo log
            }
            finally
            {

            }


        }
#endif

        public Effect GetShader(int index)
        {
            var shaderDefinition = ShaderCollection[index];
            shaderDefinition.HasChanged = false;
            return shaderDefinition.Effect;

        }

        public bool GetShaderHasChanged(int index)
        {
#if DEBUG
            return ShaderCollection[index].HasChanged;
#else
            return false;
#endif
        }

        /// <summary>
        /// Add a new shader to the collection
        /// </summary>
        /// <param name="path"></param>
        /// <returns>index in the array of shaders</returns>
        public int AddShader(string path)
        {
            bool match = ShaderCollection.Count(p => p.Path == path) > 0;

            if(!match)
            {
                ShaderCollection.Add(new ShaderDefinition(Content.Load<Effect>(path), path, contentBuiltPath + path.ToLower() + ".fx"));

                return ShaderCollection.Count - 1;
            }

            return ShaderCollection.IndexOf(ShaderCollection.First(p => p.Path == path));
        }

    }
}
