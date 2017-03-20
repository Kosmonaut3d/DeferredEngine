using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HelperSuite.GUI
{
    //todo:
    //Sort by layer to see which UIClick is the active one (only on top!)

    public class GUICanvas : GUIElement
    {
        public bool IsEnabled = true;

        private List<GUIElement> _children = new List<GUIElement>();

        public GUICanvas(Vector2 position, Vector2 dimensions,  int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            Dimensions = dimensions;
            Alignment = alignment;
            Position = position;
            OffsetPosition = position;
            Layer = layer;
            if (Alignment != GUIStyle.GUIAlignment.None)
            {
                ParentResized(ParentDimensions);
            }
        }

        //Draw the GUI, cycle through the children
        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            if (!IsEnabled) return;
            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                if (child.IsHidden) continue;
                child.Draw(guiRenderer, parentPosition + Position, mousePosition);
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
            Position = UpdateAlignment(Alignment, parentDimensions, Dimensions, Position, OffsetPosition);

            for (int index = 0; index < _children.Count; index++)
            {
                GUIElement child = _children[index];
                if (child.IsHidden) continue;
                child.ParentResized(Dimensions);
            }

        }

        //If the parent resized then our alignemnt may have changed and we need new position coordinates
        public static Vector2 UpdateAlignment(GUIStyle.GUIAlignment alignment, Vector2 parentDimensions, Vector2 dimensions, Vector2 position, Vector2 offsetPosition)
        {
            if (parentDimensions == Vector2.Zero) throw new NotImplementedException();

            switch (alignment)
            {
                case GUIStyle.GUIAlignment.None:
                    break;
                case GUIStyle.GUIAlignment.TopLeft:
                    position.X = 0;
                    position.Y = 0;
                    break;
                case GUIStyle.GUIAlignment.TopRight:
                    position.X = parentDimensions.X - dimensions.X;
                    position.Y = 0;
                    break;
                case GUIStyle.GUIAlignment.BottomLeft:
                    position.Y = parentDimensions.Y - dimensions.Y;
                    position.X = 0;
                    break;
                case GUIStyle.GUIAlignment.BottomRight:
                    position = parentDimensions - dimensions;
                    break;
                case GUIStyle.GUIAlignment.Center:
                    position = parentDimensions/2 - dimensions/2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return position + offsetPosition;
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
        
        public override GUIStyle.GUIAlignment Alignment { get; set; }
    }
}
