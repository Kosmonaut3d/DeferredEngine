using System;
using System.Collections.Generic;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;

namespace EngineTest.Recources.GUI
{
    public class GUIList : GUIElement
    {
        public bool IsEnabled = true;
        public Vector2 ElementDimensions;

        private List<GUIElement> _children = new List<GUIElement>();
        
        /// <summary>
        /// A list has a unified width/height of the elements. Each element is rendered below the other one
        /// </summary>
        /// <param name="position"></param>
        /// <param name="elementDimensions"></param>
        /// <param name="layer"></param>
        /// <param name="alignment"></param>
        public GUIList(Vector2 position, Vector2 elementDimensions, int layer = 0, GUICanvas.GUIAlignment alignment = GUICanvas.GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            ElementDimensions = elementDimensions;
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
        
        //Adjust things when resized
        public override void ParentResized(Vector2 parentDimensions)
        {
            //for (int index = 0; index < _children.Count; index++)
            //{
            //    GUIElement child = _children[index];
            //    child.ParentResized(ElementDimensions);
            //}

            Position = GUICanvas.UpdateAlignment(Alignment, parentDimensions, ElementDimensions, Position);
        }
        

        public void AddElement(GUIElement element)
        {
            element.Position = new Vector2(0, _children.Count*ElementDimensions.Y);
            element.Dimensions = ElementDimensions;
            
            //In Order
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

        public override GUICanvas.GUIAlignment Alignment { get; set; }
    }
}