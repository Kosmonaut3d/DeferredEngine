using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;

namespace EngineTest.Recources.GUI
{
    /// <summary>
    /// Just a colored block
    /// </summary>
    public class GUIBlock : GUIElement
    {
        public Vector2 Position;
        public Vector2 Dimensions;
        public Color Color;

        public GUIBlock(Vector2 position, Vector2 dimensions, Color color, int layer = 0, GUICanvas.GUIAlignment alignment = GUICanvas.GUIAlignment.None, Vector2 ParentDimensions = default(Vector2))
        {
            Position = position;
            Dimensions = dimensions;
            Color = color;
            Layer = layer;
            Alignment = alignment;
            if (Alignment != GUICanvas.GUIAlignment.None)
            {
                ParentResized(ParentDimensions);
            }
        }

        public void Draw(GUIRenderer guiRenderer, Vector2 parentPosition)
        {
            guiRenderer.DrawQuad(parentPosition+Position, Dimensions, Color);
        }

        public void ParentResized(Vector2 dimensions)
        {
            Position = GUICanvas.UpdateAlignment(Alignment, dimensions, Dimensions, Position);
        }

        public int Layer { get; }
        public GUICanvas.GUIAlignment Alignment { get; }
    }
}