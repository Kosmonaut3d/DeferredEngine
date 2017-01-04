using Microsoft.Xna.Framework;

namespace DeferredEngine.Recources.Helper
{
    static class IdGenerator
    {
        //start at 3, we need 123 for gizmos
        static int _currentId = 103;

        public static int GetNewId()
        {
            _currentId++;

            //Color test = GetColorFromId(2342);
            //int test2 = GetIdFromColor(test);

            return _currentId;
        }

        public static Color GetColorFromId(int id)
        {
            int b = id/(255*255);
            int g = (id - b*255*255)/255;
            int r = id - g*255 - b*255*255;

            return new Color(r, g, b);
        }

        public static int GetIdFromColor(Color color)
        {
            if (color.R == 255 && color.G == 255 && color.B == 255)
                return 0;
            int id = color.R + color.G*255 + color.B*255*255;
            return id;
        }
    }
}
