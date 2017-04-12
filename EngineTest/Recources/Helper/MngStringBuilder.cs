using System;
using System.Text;

namespace DeferredEngine.Recources.Helper //StringBuilderII
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
        private static readonly char minus = '-';
        private static char plus = '+';
        private static readonly char space = ' ';

        private static StringBuilder last;
        private StringBuilder sb;
        public StringBuilder StringBuilder
        {
            get { return sb; }
            private set { sb = value; last = sb; }
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
            decimalseperator = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
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
            int len = this.StringBuilder.Length;
            int reqcapacity = (index + s.Length + 1) - this.StringBuilder.Capacity;
            if (reqcapacity > 0)
                this.StringBuilder.Capacity += reqcapacity;

            int initialLength = StringBuilder.Length;
            //If we append near the end we can run out of space in the for loop. Make sure we are large enough
            if (StringBuilder.Length < index + s.Length)
            {
                StringBuilder.Length = index + s.Length;
            }

            //If our appendAt is outside the scope we need to add spaces until then
            if (index > initialLength-1)
            {
                for (int j = initialLength - 1; j < index; j++)
                {
                    StringBuilder[j] = space;
                }
            }

            for (int i = 0; i < s.Length; i++)
            {
                this.StringBuilder[i + index] = (char)(s[i]);
            }
        }


        public void Append(StringBuilder s)
        {
            int len = this.StringBuilder.Length;
            int reqcapacity = (s.Length + len) - this.StringBuilder.Capacity;
            //int reqcapacity = (s.Length + len +1) - this.StringBuilder.Capacity;
            if (reqcapacity > 0)
                this.StringBuilder.Capacity += reqcapacity;

            this.StringBuilder.Length = len + s.Length;
            for(int i = 0;i< s.Length;i++)
            {
                this.StringBuilder[i + len] = (char)(s[i]);
            }
        }
        public void Append(string s)
        {
            this.StringBuilder.Append(s);
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
            if (num >= place * 10)
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
                    int modulator = place * 10;
                    int val = num % modulator;
                    int dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (int)(place * .1);
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
            if (num >= place * 10)
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
                    int modulator = place * 10;
                    int val = num % modulator;
                    int dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (int)(place * .1);
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
            if (value >= place * 10)
            {
                // just append it
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = (int)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = n % modulator;
                    int dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (int)(place * .1);
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
            if (value >= place * 10)
            {
                // just append it,
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            long n = (long)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place * 10L;
                    long val = n % modulator;
                    long dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (long)(place * .1);
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
            if (Math.Abs(value) < 0.0001f)
            {
                sb.Append('0');
                return;
            }

            int place = 100000000;
            if (value >= place * 10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = (int)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = n % modulator;
                    int dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (int)(place * .1);
            }

            // ok lets try again
            float nd = value - (float)(n);
            if (nd > 0 && nd < 1)
            {
                sb.Append(decimalseperator);
            }
            addzeros = true;
            //nd = value;
            float placed = .1f;
            while (placed > 0.00000001)
            {
                if (nd > placed)
                {
                    float modulator = placed * 10;
                    float val = nd % modulator;
                    float dc = val / placed;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                placed = placed * .1f;
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
            if (Math.Abs(number) < 0.0001f)
            {
                sb.Append('0');
                return;
            }

            long place = 10000000000000000L;
            if (number >= place * 10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(number);
                return;
            }
            // part 1 pull integer digits
            long n = (long)(number);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place * 10L;
                    long val = n % modulator;
                    long dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (long)(place * .1);
            }

            // the decimal part
            double nd = number - (double)(n);
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
                    double modulator = placed * 10;
                    double val = nd % modulator;
                    double dc = val / placed;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                placed = placed * .1;
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
            if (value >= place * 10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(value);
                return;
            }
            // part 1 pull integer digits
            int n = (int)(value);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    int modulator = place * 10;
                    int val = n % modulator;
                    int dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (int)(place * .1);
            }

            // ok lets try again
            float nd = value - (float)(n);
            sb.Append(decimalseperator);
            addzeros = true;
            //nd = value;
            float placed = .1f;
            while (placed > 0.001)
            {
                if (nd > placed)
                {
                    float modulator = placed * 10;
                    float val = nd % modulator;
                    float dc = val / placed;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                placed = placed * .1f;
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
            if (number >= place * 10)
            {
                // just append it, if its this big its a edge case.
                sb.Append(number);
                return;
            }
            // part 1 pull integer digits
            long n = (long)(number);
            bool addzeros = false;
            while (place > 0)
            {
                if (n >= place)
                {
                    addzeros = true;
                    long modulator = place * 10L;
                    long val = n % modulator;
                    long dc = val / place;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                place = (long)(place * .1);
            }

            // ok lets try again
            double nd = number - (double)(n);
            sb.Append(decimalseperator);
            addzeros = true;
            //nd = number;
            double placed = .1;
            while (placed > 0.001)
            {
                if (nd > placed)
                {
                    double modulator = placed * 10;
                    double val = nd % modulator;
                    double dc = val / placed;
                    sb.Append((char)(dc + 48));
                }
                else
                {
                    if (addzeros) { sb.Append('0'); }
                }
                placed = placed * .1;
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
            this.StringBuilder.Insert(index, s);
        }
        public void Remove(int index, int length)
        {
            this.StringBuilder.Remove(index, length);
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
}
