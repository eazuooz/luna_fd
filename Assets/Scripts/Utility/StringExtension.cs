//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


public static class StringExtension
{
    public static string ReadLine(this string rawString, ref int position)
    {
        if (position < 0)
        {
            return null;
        }

        int length = rawString.Length;
        int offset = position;
        while (offset < length)
        {
            char ch = rawString[offset];
            switch (ch)
            {
                case '\r':
                case '\n':
                    if (offset > position)
                    {
                        string line = rawString.Substring(position, offset - position);
                        position = offset + 1;
                        if ((ch == '\r') && (position < length) && (rawString[position] == '\n'))
                        {
                            position++;
                        }

                        return line;
                    }

                    offset++;
                    position++;
                    break;

                default:
                    offset++;
                    break;
            }
        }

        if (offset > position)
        {
            string line = rawString.Substring(position, offset - position);
            position = offset;
            return line;
        }

        return null;
    }
}
