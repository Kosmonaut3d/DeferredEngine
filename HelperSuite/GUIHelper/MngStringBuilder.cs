using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;

namespace HelperSuite.GUIHelper //StringBuilderII
{
    /// <summary> 
    /// 
    /// http://community.monogame.net/t/no-garbage-text-and-numbers/8478
    /// By willmotil
    /// 
    /// About.
    /// Essentially a No Garbage StringBuilder Wrapper Safety device. 
    /// Thru this a stringbuilder will no longer generate garbage from numerical data.
    /// 
    /// If nothing else you can use this for numbers or directly access the internal stringbuilder for text.
    /// 
    /// About... this class.
    /// 
    /// 
    /// Die garbage die :) really its die collections but if there is nothing to collect its even better.
    /// Because you shouldn't have to take the garbage to the outside trashbin 15x a second, 
    /// for a few lines of information.
    /// 
    /// Had to do a lot of reading, a lot of brain bending a lot of testing.
    /// To figure out what was generating garbage (turns out its the numbers and this is c# wide in scope).
    /// 
    /// This doesn't handle globilization if you want that you will have to make alterations.
    /// 
    /// Additional note.
    /// Wrapping the stringbuilder it turns out forced the string builder to tostring,
    /// then to pull in a string builder as a object, hence boxing it arggg. fixed.
    /// Ideally i would have used operator overloading and extentions to upgrade the stringbuilder class itself. 
    /// Unfortunately c# has its limits and i found this was not technically possible, 
    /// though i got close it was also overly complex for something that couldn't meet the goal.
    /// 
    /// In conclusion :
    /// 
    /// It probably could be a little more polished. It can easily be perfected with a bit of effort.
    /// It works well enough for a simple game or editor or especially during testing as is.
    /// 
    /// </summary>
    public sealed class MngStringBuilder
    {
        private static char decimalseperator = '.';
        private static char minus = '-';
        private static char plus = '+';
        private static char space = ' ';

        private static StringBuilder last;
        private StringBuilder sb;

        public StringBuilder StringBuilder
        {
            get { return sb; }
            private set
            {
                sb = value;
                last = sb;
            }
        }

        public int Length
        {
            get { return StringBuilder.Length; }
            set { StringBuilder.Length = value; }
        }

        public int Capacity
        {
            get { return StringBuilder.Capacity; }
            set { StringBuilder.Capacity = value; }
        }

        // constructors
        public MngStringBuilder()
        {
            StringBuilder = StringBuilder;
        }

        public MngStringBuilder(int capacity)
        {
            StringBuilder = new StringBuilder(capacity);
        }

        public MngStringBuilder(StringBuilder sb)
        {
            StringBuilder = sb;
        }

        public MngStringBuilder(string s)
        {
            StringBuilder = new StringBuilder(s);
        }

        public static void CheckSeperator()
        {
            decimalseperator =
                Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        // operators
        public static implicit operator MngStringBuilder(StringBuilder sb)
        {
            return new MngStringBuilder(sb);
        }

        public static MngStringBuilder operator +(MngStringBuilder sbm, MngStringBuilder s)
        {
            sbm.StringBuilder.Append(s);
            return sbm;
        }

        public void AppendAt(int index, StringBuilder s)
        {
            int len = StringBuilder.Length;
            int reqcapacity = (index + s.Length + 1) - StringBuilder.Capacity;
            if (reqcapacity > 0)
                StringBuilder.Capacity += reqcapacity;

            int initialLength = StringBuilder.Length;
            //If we append near the end we can run out of space in the for loop. Make sure we are large enough
            if (StringBuilder.Length < index + s.Length)
            {
                StringBuilder.Length = index + s.Length;
            }

            //If our appendAt is outside the scope we need to add spaces until then
            if (index > initialLength - 1)
            {
                for (int j = initialLength - 1; j < index; j++)
                {
                    StringBuilder[j] = space;
                }
            }

            for (int i = 0; i < s.Length; i++)
            {
                StringBuilder[i + index] = s[i];
            }
        }


        public void Append(StringBuilder s)
        {
            int len = StringBuilder.Length;
            int reqcapacity = (s.Length + len) - StringBuilder.Capacity;
            //int reqcapacity = (s.Length + len +1) - this.StringBuilder.Capacity;
            if (reqcapacity > 0)
                StringBuilder.Capacity += reqcapacity;

            StringBuilder.Length = len + s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                StringBuilder[i + len] = s[i];
            }
        }

        public void Append(string s)
        {
            StringBuilder.Append(s);
        }

        public void Append(char c)
        {
            StringBuilder.Append(c);
        }

        public void Append(byte value)
        {
            // basics
            int num = value;
            if (num == 0)
            {
                sb.Append('0');
                return;
            }
            int place = 100;
            if (num >= place*10)
            {
                // just append it
                sb.Append(num);
                return;
            }
            // part 1 pull integer digits
            bool addzeros = false;
            while (place > 0)
            {
                if (num >= place)
                {
                    addzeros = true;
                    int modulator = place*10;
                    int val = num%modulator;
                    int dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (int) (place*.1);
            }
        }

        public void Append(short value)
        {
            int num = value;
            // basics
            if (num < 0)
            {
                // Negative.
                sb.Append(minus);
                num = -num;
            }
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            int place = 10000;
            if (num >= place*10)
            {
                // just append it, if its this big, this isn't a science calculator, its a edge case.
                sb.Append(num);
                return;
            }
            // part 1 pull integer digits
            bool addzeros = false;
            while (place > 0)
            {
                if (num >= place)
                {
                    addzeros = true;
                    int modulator = place*10;
                    int val = num%modulator;
                    int dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (int) (place*.1);
            }
        }

        public void Append(int value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                sb.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            int place = 1000000000;
            if (value >= place*10)
            {
                // just append it
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = value;
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place*10;
                    int val = n%modulator;
                    int dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (int) (place*.1);
            }
        }

        public void Append(long value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                sb.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            long place = 10000000000000000L;
            if (value >= place*10)
            {
                // just append it,
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            long n = value;
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place*10L;
                    long val = n%modulator;
                    long dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (long) (place*.1);
            }
        }

        public void Append(float value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                sb.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            int place = 100000000;
            if (value >= place*10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = (int) (value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place*10;
                    int val = n%modulator;
                    int dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (int) (place*.1);
            }

            // ok lets try again
            float nd = value - n;
            if (nd > 0 && nd < 1)
            {
                sb.Append(decimalseperator);
            }
            //addzeros = true;
            //nd = value;
            float placed = .1f;
            while (placed > 0.00000001)
            {
                if (nd > placed)
                {
                    float modulator = placed*10;
                    float val = nd%modulator;
                    float dc = val/placed;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                   sb.Append('0');
                }
                placed = placed*.1f;
            }
        }

        public void Append(double number)
        {

            // basics
            if (number < 0)
            {
                // Negative.
                sb.Append(minus);
                number = -number;
            }
            if (number == 0)
            {
                sb.Append('0');
                return;
            }

            long place = 10000000000000000L;
            if (number >= place*10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(number);
                return;
            }
            // part 1 pull integer digits
            long n = (long) (number);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place*10L;
                    long val = n%modulator;
                    long dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (long) (place*.1);
            }

            // the decimal part
            double nd = number - n;
            if (nd > 0 && nd < 1)
            {
                sb.Append(decimalseperator);
            }
            addzeros = true;
            //nd = number;
            double placed = .1;
            while (placed > 0.0000000000001)
            {
                if (nd > placed)
                {
                    double modulator = placed*10;
                    double val = nd%modulator;
                    double dc = val/placed;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                placed = placed*.1;
            }
        }

        public void AppendTrim(float value)
        {
            // basics
            if (value < 0)
            {
                // Negative.
                sb.Append(minus);
                value = -value;
            }
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            int place = 100000000;
            if (value >= place*10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = (int) (value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place*10;
                    int val = n%modulator;
                    int dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (int) (place*.1);
            }

            // ok lets try again
            float nd = value - n;
            sb.Append(decimalseperator);
            addzeros = true;
            //nd = value;
            float placed = .1f;
            while (placed > 0.001)
            {
                if (nd > placed)
                {
                    float modulator = placed*10;
                    float val = nd%modulator;
                    float dc = val/placed;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                placed = placed*.1f;
            }
        }

        public void AppendTrim(double number)
        {
            // basics
            if (number < 0)
            {
                // Negative.
                sb.Append(minus);
                number = -number;
            }
            if (number == 0)
            {
                sb.Append('0');
                return;
            }
            long place = 10000000000000000L;
            if (number >= place*10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(number);
                return;
            }
            // part 1 pull integer digits
            long n = (long) (number);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place*10L;
                    long val = n%modulator;
                    long dc = val/place;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                place = (long) (place*.1);
            }

            // ok lets try again
            double nd = number - n;
            sb.Append(decimalseperator);
            addzeros = true;
            //nd = number;
            double placed = .1;
            while (placed > 0.001)
            {
                if (nd > placed)
                {
                    double modulator = placed*10;
                    double val = nd%modulator;
                    double dc = val/placed;
                    sb.Append((char) (dc + 48));
                }
                else
                {
                    if (addzeros)
                    {
                        sb.Append('0');
                    }
                }
                placed = placed*.1;
            }
        }

        public void AppendLine(StringBuilder s)
        {
            Append(s);
            sb.AppendLine();
        }

        public void AppendLine()
        {
            sb.AppendLine();
        }

        public void Insert(int index, StringBuilder s)
        {
            StringBuilder.Insert(index, s);
        }

        public void Remove(int index, int length)
        {
            StringBuilder.Remove(index, length);
        }

        public char[] ToCharArray()
        {
            char[] a = new char[sb.Length];
            sb.CopyTo(0, a, 0, sb.Length);
            return a;
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }

    //http://www.gavpugh.com* 0.5f010/04/01/xnac-avoiding-garbage-when-working-with-stringbuilder/
    public static class StringBuilderExtensions
        {
            // These digits are here in a static array to support hex with simple, easily-understandable code. 
            // Since A-Z don't sit next to 0-9 in the ascii table.
            private static readonly char[] ms_digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            private static readonly uint ms_default_decimal_places = 5; //< Matches standard .NET formatting dp's
            private static readonly char ms_default_pad_char = '0';

            //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
            public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char, uint base_val)
            {
                Debug.Assert(pad_amount >= 0);
                Debug.Assert(base_val > 0 && base_val <= 16);

                // Calculate length of integer when written out
                uint length = 0;
                uint length_calc = uint_val;

                do
                {
                    length_calc /= base_val;
                    length++;
                }
                while (length_calc > 0);

                // Pad out space for writing.
                string_builder.Append(pad_char, (int)Math.Max(pad_amount, length));

                int strpos = string_builder.Length;

                // We're writing backwards, one character at a time.
                while (length > 0)
                {
                    strpos--;

                    // Lookup from static char array, to cover hex values too
                    string_builder[strpos] = ms_digits[uint_val % base_val];

                    uint_val /= base_val;
                    length--;
                }

                return string_builder;
            }


        public static StringBuilder AppendColor(this StringBuilder string_builder, Color color)
        {
            const string r = "r:";
            const string g = " g:";
            const string b = " b:";

            string_builder.Append(r);
            string_builder.Concat(color.R);
            string_builder.Append(g);
            string_builder.Concat(color.G);
            string_builder.Append(b);
            string_builder.Concat(color.B);
            return string_builder;
        }

        public static StringBuilder AppendVector3(this StringBuilder string_builder, Vector3 v3)
        {
            const string r = "x:";
            const string g = " y:";
            const string b = " z:";

            string_builder.Append(r);
            string_builder.Concat(v3.X);
            string_builder.Append(g);
            string_builder.Concat(v3.Y);
            string_builder.Append(b);
            string_builder.Concat(v3.Z);
            return string_builder;
        }

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume no padding and base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val)
            {
                string_builder.Concat(uint_val, 0, ms_default_pad_char, 10);
                return string_builder;
            }

            //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
            public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount)
            {
                string_builder.Concat(uint_val, pad_amount, ms_default_pad_char, 10);
                return string_builder;
            }

            //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
            public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char)
            {
                string_builder.Concat(uint_val, pad_amount, pad_char, 10);
                return string_builder;
            }

            //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
            public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char, uint base_val)
            {
                Debug.Assert(pad_amount >= 0);
                Debug.Assert(base_val > 0 && base_val <= 16);

                // Deal with negative numbers
                if (int_val < 0)
                {
                    string_builder.Append('-');
                    uint uint_val = uint.MaxValue - ((uint)int_val) + 1; //< This is to deal with Int32.MinValue
                    string_builder.Concat(uint_val, pad_amount, pad_char, base_val);
                }
                else
                {
                    string_builder.Concat((uint)int_val, pad_amount, pad_char, base_val);
                }

                return string_builder;
            }

            //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume no padding and base ten.
            public static StringBuilder Concat(this StringBuilder string_builder, int int_val)
            {
                string_builder.Concat(int_val, 0, ms_default_pad_char, 10);
                return string_builder;
            }

            //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
            public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount)
            {
                string_builder.Concat(int_val, pad_amount, ms_default_pad_char, 10);
                return string_builder;
            }

            //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
            public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char)
            {
                string_builder.Concat(int_val, pad_amount, pad_char, 10);
                return string_builder;
            }

            //! Convert a given float value to a string and concatenate onto the stringbuilder
            public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount, char pad_char)
            {
                Debug.Assert(pad_amount >= 0);

                if (float_val < 0)
                    string_builder.Append('-');

                if (decimal_places == 0)
                {
                    // No decimal places, just round up and print it as an int

                    // Agh, Math.Floor() just works on doubles/decimals. Don't want to cast! Let's do this the old-fashioned way.
                    int int_val;
                    if (float_val >= 0.0f)
                    {
                        // Round up
                        int_val = (int)(float_val + 0.5f);
                    }
                    else
                    {
                        // Round down for negative numbers
                        int_val = (int)(float_val - 0.5f);
                    }

                    string_builder.Concat(int_val, pad_amount, pad_char, 10);
                }
                else
                {
                    int int_part = (int)Math.Abs(float_val);

                    // First part is easy, just cast to an integer
                    string_builder.Concat(int_part, pad_amount, pad_char, 10);

                    // Decimal point
                    string_builder.Append('.');

                    // Work out remainder we need to print after the d.p.
                    float remainder = Math.Abs(float_val - int_part);

                    // Multiply up to become an int that we can print
                    do
                    {
                        remainder *= 10;
                        decimal_places--;

                        if(remainder<1)
                        string_builder.Concat((uint)0, 0, '0', 10);

                }
                    while (decimal_places > 0);

                    // Round up. It's guaranteed to be a positive number, so no extra work required here.
                    remainder += 0.5f;

                    // All done, print that as an int!
                    string_builder.Concat((uint)remainder, 0, '0', 10);
                }
                return string_builder;
            }

            //! Convert a given float value to a string and concatenate onto the stringbuilder. Assumes five decimal places, and no padding.
            public static StringBuilder Concat(this StringBuilder string_builder, float float_val)
            {
                string_builder.Concat(float_val, ms_default_decimal_places, 0, ms_default_pad_char);
                return string_builder;
            }

            //! Convert a given float value to a string and concatenate onto the stringbuilder. Assumes no padding.
            public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places)
            {
                string_builder.Concat(float_val, decimal_places, 0, ms_default_pad_char);
                return string_builder;
            }

            //! Convert a given float value to a string and concatenate onto the stringbuilder.
            public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount)
            {
                string_builder.Concat(float_val, decimal_places, pad_amount, ms_default_pad_char);
                return string_builder;
            }
        
    }
}
