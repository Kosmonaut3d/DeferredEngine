using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Renderer.Helper;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources.GUI
{
    public interface GUIElement
    {
        void Draw(GUIRenderer guiRenderer, Microsoft.Xna.Framework.Vector2 parentPosition);
        void ParentResized(Vector2 dimensions);
        int Layer { get; }
        GUICanvas.GUIAlignment Alignment { get; }
    }

    public class GUICanvas : GUIElement
    {
        public Vector2 Dimensions;
        public Vector2 Position;
        private List<GUIElement> _children = new List<GUIElement>();

        public enum GUIAlignment
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        };

        public GUICanvas(Vector2 position, Vector2 dimensions,  int layer = 0, GUIAlignment alignment = GUIAlignment.None)
        {
            Dimensions = dimensions;
            Alignment = alignment;
            Position = position;
            Layer = layer;
            Alignment = alignment;
        }

        //Draw the GUI, cycle through the children
        public void Draw(GUIRenderer guiRenderer, Microsoft.Xna.Framework.Vector2 parentPosition)
        {
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                child.Draw(guiRenderer, parentPosition + Position);
            }
        }

        public void Resize(float width, float height)
        {
            Dimensions = new Vector2(width, height);
            ParentResized(Dimensions);
        }

        //Adjust things when resized
        public void ParentResized(Vector2 parentDimensions)
        {
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                child.ParentResized(Dimensions);
            }

            Position = UpdateAlignment(Alignment, parentDimensions, Dimensions, Position);
        }

        //If the parent resized then our alignemnt may have changed and we need new position coordinates
        public static Vector2 UpdateAlignment(GUIAlignment alignment, Vector2 parentDimensions, Vector2 dimensions, Vector2 position)
        {
            switch (alignment)
            {
                case GUIAlignment.None:
                    break;
                case GUIAlignment.TopLeft:
                    break;
                case GUIAlignment.TopRight:
                    position.X = parentDimensions.X - dimensions.X;
                    break;
                case GUIAlignment.BottomLeft:
                    position.Y = parentDimensions.Y - dimensions.Y;
                    break;
                case GUIAlignment.BottomRight:
                    position = parentDimensions - dimensions;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return position;
        }

        public void AddElement(GUIElement element)
        {
            //In Order
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Layer > element.Layer)
                {
                    _children.Insert(i, element);
                    return;
                }
            }

            _children.Add(element);
        }

        public int Layer { get; }
        public GUIAlignment Alignment { get; }
    }
}
