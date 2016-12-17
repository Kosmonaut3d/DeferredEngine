using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Main;
using EngineTest.Renderer.Helper;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources.GUI
{
    public abstract class GUIElement
    {
        public Vector2 Position;
        public virtual Vector2 Dimensions { get; set; }
        public abstract void Draw(GUIRenderer guiRenderer, Vector2 parentPosition);
        public abstract void ParentResized(Vector2 dimensions);
        public abstract int Layer { get; set; }
        public abstract void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition);
        public abstract GUICanvas.GUIAlignment Alignment { get; set; }
    }

    public class GUICanvas : GUIElement
    {
        public bool IsEnabled = true;

        private List<GUIElement> _children = new List<GUIElement>();

        public enum GUIAlignment
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        };

        public GUICanvas(Vector2 position, Vector2 dimensions,  int layer = 0, GUIAlignment alignment = GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            Dimensions = dimensions;
            Alignment = alignment;
            Position = position;
            Layer = layer;
            if (Alignment != GUICanvas.GUIAlignment.None)
            {
                ParentResized(ParentDimensions);
            }
        }

        //Draw the GUI, cycle through the children
        public override void Draw(GUIRenderer guiRenderer, Microsoft.Xna.Framework.Vector2 parentPosition)
        {
            if (!IsEnabled) return;
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
        public override void ParentResized(Vector2 parentDimensions)
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
            if (parentDimensions == Vector2.Zero) throw new NotImplementedException();

            switch (alignment)
            {
                case GUIAlignment.None:
                    break;
                case GUIAlignment.TopLeft:
                    position.X = 0;
                    position.Y = 0;
                    break;
                case GUIAlignment.TopRight:
                    position.X = parentDimensions.X - dimensions.X;
                    position.Y = 0;
                    break;
                case GUIAlignment.BottomLeft:
                    position.Y = parentDimensions.Y - dimensions.Y;
                    position.X = 0;
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

        public override int Layer { get; set; }

        /// <summary>
        /// Update our logic
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="mousePosition"></param>
        /// <param name="parentPosition"></param>
        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            if (!IsEnabled) return;
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                child.Update(gameTime, mousePosition, parentPosition + Position);
            }
        }
        
        public override GUIAlignment Alignment { get; set; }
    }
}
