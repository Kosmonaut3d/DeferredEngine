using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelperSuite.ContentLoader;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HelperSuite.GUIHelper
{
    public class GUIContentLoader : IDisposable
    {

        private ContentManager _contentManager;

        public readonly List<object> ContentArray = new List<object>();
        
        enum buildMode
        { defaultMode, retryWithoutAnimation, retryWithoutTangents, finished, failed };


        public void Load(ContentManager contentManager)
        {
            _contentManager = new ThreadSafeContentManager(contentManager.ServiceProvider) {RootDirectory = "Content"};
        }

        public void LoadContentFile<T>(out Task loadTaskOut, ref int pointerPositionInOut, out string filenameOut/*, string path*/)
        {
            GUIControl.LastMouseState = Mouse.GetState();
            GUIControl.CurrentMouseState = Mouse.GetState();
            
            string dialogFilter = "All files(*.*) | *.*";
            string pipeLineFile = "runtime.txt";
            //Switch the content pipeline parameters depending on the content type
            
            if (typeof(T) == typeof(Texture2D))
            {
                dialogFilter =
                    "image files (*.png, .jpg, .jpeg, .bmp, .dds, .gif)|*.png;*.jpg;*.bmp;*.jpeg;*.gif;*.dds|All files (*.*)|*.*";
                pipeLineFile = "runtimetexture.txt";
            }
            //else if (typeof(T) == typeof(AnimatedModel))
            //{
            //    dialogFilter =
            //        "model file (*.fbx, *.obj)|*.fbx;*.obj|All files (*.*)|*.*";
            //    pipeLineFile = "runtimeanimatedmodel.txt";
            //}
            else
            {
                throw new Exception("Content type not supported!");
            }

            filenameOut = "...";

            string completeFilePath = null;

            string fileName = null;
            string copiedFilePath = null;
            string shortFileName = null;
            
            string fileEnding = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                //InitialDirectory = Application.StartupPath,
                Filter = dialogFilter,
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = false
            };

            //"c:\\";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                completeFilePath = openFileDialog1.FileName;
                if (openFileDialog1.SafeFileName != null)
                    fileName = openFileDialog1.SafeFileName;

                //Make it test instead of test.jpg;
                if (fileName != null)
                {
                    string[] split = fileName.Split('.');

                    shortFileName = split[0];
                    fileEnding = split[1];
                }

                copiedFilePath = Application.StartupPath + "/" + fileName;

                filenameOut = fileName;
            }
            else
            {
                loadTaskOut = null;
                if(pointerPositionInOut!=-1)
                    ContentArray[pointerPositionInOut] = null;

                filenameOut = "...";

                return;
            }


            if (pointerPositionInOut == -1)
            {
                pointerPositionInOut = ContentArray.Count;
                ContentArray.Add(null);
            }
            else
            {
                if (pointerPositionInOut >= ContentArray.Count)
                    throw new NotImplementedException("Shouldn't be possible, pointer is greater than list.count");
            }

            int position = pointerPositionInOut;

            loadTaskOut = Task.Factory.StartNew(() =>
            {
                //Copy file to directory
                {
                    try
                    {
                        if (copiedFilePath != null)
                            File.Copy(completeFilePath, copiedFilePath);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    if (!File.Exists(copiedFilePath)) return;
                }

                //string MGCBpathDirectory = Application.StartupPath + "/Content/MGCB/";
                string mgcbPathExe = Application.StartupPath + "/Content/MGCB/mgcb.exe";

                completeFilePath = completeFilePath.Replace("\\", "/");

                buildMode mode = buildMode.defaultMode;

                while (mode != buildMode.finished)
                {
                    if (mode == buildMode.retryWithoutAnimation)
                    {
                        pipeLineFile = "runtimemodel.txt";
                    }
                    else if (mode == buildMode.retryWithoutTangents)
                    {
                        pipeLineFile = "runtimemodelnotangent.txt";
                    }

                    //Create pProcess
                    Process pProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = mgcbPathExe,
                            Arguments = "/@:\"Content/mgcb/" + pipeLineFile + "\""
                            +" /build:\"" + fileName /*completeFilePath*/+ "\"",
                            CreateNoWindow = true,
                            WorkingDirectory = Application.StartupPath,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
                    };

                    //Get program output
                    string stdError = null;
                    StringBuilder stdOutput = new StringBuilder();
                    pProcess.OutputDataReceived += (sender, args) => stdOutput.Append( args.Data );

                    try
                    {
                        pProcess.Start();
                        pProcess.BeginOutputReadLine();
                        stdError = pProcess.StandardError.ReadToEnd();
                        pProcess.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("OS error while executing : " + e.Message, e);
                    }

                    if (pProcess.ExitCode == 0)
                    {

                       mode = buildMode.finished;
                    }
                    else
                    {
                        var message = new StringBuilder();

                        if (!string.IsNullOrEmpty(stdError))
                        {
                            message.AppendLine(stdError);
                        }

                        bool fail = true;

                        if (stdOutput.Length != 0)
                        {
                            string stout = stdOutput.ToString();
                            //A bit senseless, eh?
                            message.AppendLine("Std output:");
                            message.AppendLine(stout);

                            if (stout.Contains("tangent") && mode == buildMode.defaultMode)
                            {
                                mode = buildMode.retryWithoutTangents;
                                fail = false;
                            }
                        }

                        if (fail)
                        {
                            //Debug.WriteLine(message);

                            //if (GameSettings.d_log)
                            //{
                            //    StreamWriter steam =
                            //        new StreamWriter(new FileStream("log.txt", FileMode.OpenOrCreate, FileAccess.Write));
                            //    steam.WriteLine("mgcb finished with exit code = " + pProcess.ExitCode + ": " + message);
                            //    steam.Close();
                            //}

                            throw new Exception("mgcb finished with exit code = " + pProcess.ExitCode + ": " + message);
                        }
                    }
                }

                //if (typeof(T) == typeof(AnimatedModel))
                //{
                //    //Have to load normal model
                //    if (mode == buildMode.retryWithoutAnimation)
                //    {
                //         ContentArray[position] = _contentManager.Load<Model>("Runtime/Textures/" + shortFileName);
                //    }
                //    else
                //    {
                //        ContentArray[position] = new AnimatedModel("Runtime/Textures/" + shortFileName);
                //        ((AnimatedModel)ContentArray[position]).LoadContent(_contentManager);
                //    }
                //}
                //else
                {
                    ContentArray[position] = _contentManager.Load<T>("Runtime/Textures/" + shortFileName);
                }
                string path = Application.StartupPath + "\\Content\\Runtime\\Textures\\" + shortFileName;
                File.Delete(path + ".xnb");

                //We should delete the generated .xnb file in the directory now

                if (copiedFilePath != null)
                    File.Delete(copiedFilePath);


            });

        }

        public void Dispose()
        {
            _contentManager?.Dispose();
        }
    }
}
