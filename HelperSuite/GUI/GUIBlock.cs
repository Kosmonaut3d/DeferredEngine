using Microsoft.Xna.Framework;

namespace HelperSuite.GUI
{
    /// <summary>
    /// Just a colored block
    /// </summary>
    public class GUIBlock : GUIElement
    {
        public Color BlockColor;

        public GUIBlock(GUIStyle style) : this(
            position: Vector2.Zero,
            dimensions: style.DimensionsStyle,
            blockColor: style.BlockColorStyle,
            layer: 0,
            alignment: style.GuiAlignmentStyle,
            ParentDimensions: style.ParentDimensionsStyle)
        {
            //Filled by GuiStyle
        }

        public GUIBlock(Vector2 position, Vector2 dimensions, Color blockColor, int layer = 0, GUIStyle.GUIAlignment alignment = GUIStyle.GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            Position = position;
            Dimensions = dimensions;
            BlockColor = blockColor;
            Layer = layer;
            Alignment = alignment;
            if (Alignment != GUIStyle.GUIAlignment.None)
            {
                ParentResized(ParentDimensions);
            }
            
        }

        private GUIBlock()
        {
        }

        public override void Draw(GUIRenderer.GUIRenderer guiRenderer, Vector2 parentPosition, Vector2 mousePosition)
        {
            guiRenderer.DrawQuad(parentPosition+Position, Dimensions, BlockColor);
        }

        public override void ParentResized(Vector2 dimensions)
        {
            Position = GUICanvas.UpdateAlignment(Alignment, dimensions, Dimensions, Position, OffsetPosition);
        }

        public override int Layer { get; set; }
        public override void Update(GameTime gameTime, Vector2 mousePosition, Vector2 parentPosition)
        {
            //;
        }
        

        public override GUIStyle.GUIAlignment Alignment { get; set; }

        public bool IsVisible
        {
            get
            {
                return !IsHidden;
            }

            set
            {
                IsHidden = !value;
            }
        }
        
    }
}