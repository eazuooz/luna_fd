//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;
using System.IO;

public static class BinaryExtension
{
    private static readonly byte[] s_CachedBytes = new byte[byte.MaxValue + 1];

    public static int Read7BitEncodedInt32(this BinaryReader binaryReader)
    {
        int value = 0;
        int shift = 0;
        byte b;
        do
        {
            if (shift >= 35)
            {
                throw new GameFrameworkException("7 bit encoded int is invalid.");
            }

            b = binaryReader.ReadByte();
            value |= (b & 0x7f) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    public static void Write7BitEncodedInt32(this BinaryWriter binaryWriter, int value)
    {
        uint num = (uint)value;
        while (num >= 0x80)
        {
            binaryWriter.Write((byte)(num | 0x80));
            num >>= 7;
        }

        binaryWriter.Write((byte)num);
    }

    public static uint Read7BitEncodedUInt32(this BinaryReader binaryReader)
    {
        return (uint)Read7BitEncodedInt32(binaryReader);
    }

    public static void Write7BitEncodedUInt32(this BinaryWriter binaryWriter, uint value)
    {
        Write7BitEncodedInt32(binaryWriter, (int)value);
    }

    public static long Read7BitEncodedInt64(this BinaryReader binaryReader)
    {
        long value = 0L;
        int shift = 0;
        byte b;
        do
        {
            if (shift >= 70)
            {
                throw new GameFrameworkException("7 bit encoded int is invalid.");
            }

            b = binaryReader.ReadByte();
            value |= (b & 0x7fL) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    public static void Write7BitEncodedInt64(this BinaryWriter binaryWriter, long value)
    {
        ulong num = (ulong)value;
        while (num >= 0x80)
        {
            binaryWriter.Write((byte)(num | 0x80));
            num >>= 7;
        }

        binaryWriter.Write((byte)num);
    }

    public static ulong Read7BitEncodedUInt64(this BinaryReader binaryReader)
    {
        return (ulong)Read7BitEncodedInt64(binaryReader);
    }

    public static void Write7BitEncodedUInt64(this BinaryWriter binaryWriter, ulong value)
    {
        Write7BitEncodedInt64(binaryWriter, (long)value);
    }

    public static string ReadEncryptedString(this BinaryReader binaryReader, byte[] encryptBytes)
    {
        byte length = binaryReader.ReadByte();
        if (length <= 0)
        {
            return null;
        }

        if (length > byte.MaxValue)
        {
            throw new GameFrameworkException("String is too long.");
        }

        for (byte i = 0; i < length; i++)
        {
            s_CachedBytes[i] = binaryReader.ReadByte();
        }

        Utility.Encryption.GetSelfXorBytes(s_CachedBytes, 0, length, encryptBytes);
        string value = Utility.Converter.GetString(s_CachedBytes, 0, length);
        Array.Clear(s_CachedBytes, 0, length);
        return value;
    }

    public static void WriteEncryptedString(this BinaryWriter binaryWriter, string value, byte[] encryptBytes)
    {
        if (string.IsNullOrEmpty(value))
        {
            binaryWriter.Write((byte)0);
            return;
        }

        int length = Utility.Converter.GetBytes(value, s_CachedBytes);
        if (length > byte.MaxValue)
        {
            throw new GameFrameworkException(Utility.Text.Format("String '{0}' is too long.", value));
        }

        Utility.Encryption.GetSelfXorBytes(s_CachedBytes, encryptBytes);
        binaryWriter.Write((byte)length);
        binaryWriter.Write(s_CachedBytes, 0, length);
    }
}
