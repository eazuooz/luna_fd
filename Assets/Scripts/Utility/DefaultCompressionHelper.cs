//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using Unity.SharpZipLib.GZip;
using System;
using System.IO;

namespace UnityGameFramework.Runtime
{
    public class DefaultCompressionHelper : Utility.Compression.ICompressionHelper
    {
        private const int CachedBytesLength = 0x1000;
        private readonly byte[] mCachedBytes = new byte[CachedBytesLength];

        public bool Compress(byte[] bytes, int offset, int length, Stream compressedStream)
        {
            if (bytes == null)
            {
                return false;
            }

            if (offset < 0 || length < 0 || offset + length > bytes.Length)
            {
                return false;
            }

            if (compressedStream == null)
            {
                return false;
            }

            try
            {
                GZipOutputStream gZipOutputStream = new GZipOutputStream(compressedStream);
                gZipOutputStream.Write(bytes, offset, length);
                gZipOutputStream.Finish();
                ProcessHeader(compressedStream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Compress(Stream stream, Stream compressedStream)
        {
            if (stream == null)
            {
                return false;
            }

            if (compressedStream == null)
            {
                return false;
            }

            try
            {
                GZipOutputStream gZipOutputStream = new GZipOutputStream(compressedStream);
                int bytesRead = 0;
                while ((bytesRead = stream.Read(mCachedBytes, 0, CachedBytesLength)) > 0)
                {
                    gZipOutputStream.Write(mCachedBytes, 0, bytesRead);
                }

                gZipOutputStream.Finish();
                ProcessHeader(compressedStream);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                Array.Clear(mCachedBytes, 0, CachedBytesLength);
            }
        }

        public bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream)
        {
            if (bytes == null)
            {
                return false;
            }

            if (offset < 0 || length < 0 || offset + length > bytes.Length)
            {
                return false;
            }

            if (decompressedStream == null)
            {
                return false;
            }

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes, offset, length, false);
                using (GZipInputStream gZipInputStream = new GZipInputStream(memoryStream))
                {
                    int bytesRead = 0;
                    while ((bytesRead = gZipInputStream.Read(mCachedBytes, 0, CachedBytesLength)) > 0)
                    {
                        decompressedStream.Write(mCachedBytes, 0, bytesRead);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }

                Array.Clear(mCachedBytes, 0, CachedBytesLength);
            }
        }

        public bool Decompress(Stream stream, Stream decompressedStream)
        {
            if (stream == null)
            {
                return false;
            }

            if (decompressedStream == null)
            {
                return false;
            }

            try
            {
                GZipInputStream gZipInputStream = new GZipInputStream(stream);
                int bytesRead = 0;
                while ((bytesRead = gZipInputStream.Read(mCachedBytes, 0, CachedBytesLength)) > 0)
                {
                    decompressedStream.Write(mCachedBytes, 0, bytesRead);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                Array.Clear(mCachedBytes, 0, CachedBytesLength);
            }
        }

        private static void ProcessHeader(Stream compressedStream)
        {
            if (compressedStream.Length >= 8L)
            {
                long current = compressedStream.Position;
                compressedStream.Position = 4L;
                compressedStream.WriteByte(25);
                compressedStream.WriteByte(134);
                compressedStream.WriteByte(2);
                compressedStream.WriteByte(32);
                compressedStream.Position = current;
            }
        }
    }
}
