//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.FileSystem;
using System;
using System.IO;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class AndroidFileSystemStream : FileSystemStream
    {
        private static readonly string SplitFlag = "!/assets/";
        private static readonly int SplitFlagLength = SplitFlag.Length;
        private static readonly AndroidJavaObject s_AssetManager = null;
        private static readonly IntPtr s_InternalReadMethodId = IntPtr.Zero;
        private static readonly jvalue[] s_InternalReadArgs = null;

        private readonly AndroidJavaObject mFileStream;
        private readonly IntPtr mFileStreamRawObject;

        static AndroidFileSystemStream()
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayer == null)
            {
                throw new GameFrameworkException("Unity player is invalid.");
            }

            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (currentActivity == null)
            {
                throw new GameFrameworkException("Current activity is invalid.");
            }

            AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");
            if (assetManager == null)
            {
                throw new GameFrameworkException("Asset manager is invalid.");
            }

            s_AssetManager = assetManager;

            IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
            s_InternalReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
            s_InternalReadArgs = new jvalue[3];

            AndroidJNI.DeleteLocalRef(inputStreamClassPtr);
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }

        public AndroidFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access != FileSystemAccess.Read)
            {
                throw new GameFrameworkException(Utility.Text.Format("'{0}' is not supported in AndroidFileSystemStream.", access));
            }

            if (createNew)
            {
                throw new GameFrameworkException("Create new is not supported in AndroidFileSystemStream.");
            }

            int position = fullPath.LastIndexOf(SplitFlag, StringComparison.Ordinal);
            if (position < 0)
            {
                throw new GameFrameworkException("Can not find split flag in full path.");
            }

            string fileName = fullPath.Substring(position + SplitFlagLength);
            mFileStream = InternalOpen(fileName);
            if (mFileStream == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Open file '{0}' from Android asset manager failure.", fullPath));
            }

            mFileStreamRawObject = mFileStream.GetRawObject();
        }

        protected override long Position
        {
            get
            {
                throw new GameFrameworkException("Get position is not supported in AndroidFileSystemStream.");
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        protected override long Length
        {
            get
            {
                return InternalAvailable();
            }
        }

        protected override void SetLength(long length)
        {
            throw new GameFrameworkException("SetLength is not supported in AndroidFileSystemStream.");
        }

        protected override void Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                Seek(Length + offset, SeekOrigin.Begin);
                return;
            }

            if (origin == SeekOrigin.Begin)
            {
                InternalReset();
            }

            while (offset > 0)
            {
                long skip = InternalSkip(offset);
                if (skip < 0)
                {
                    return;
                }

                offset -= skip;
            }
        }

        protected override int ReadByte()
        {
            return InternalRead();
        }

        protected override int Read(byte[] buffer, int startIndex, int length)
        {
            byte[] result = null;
            int bytesRead = InternalRead(length, out result);
            Array.Copy(result, 0, buffer, startIndex, bytesRead);
            return bytesRead;
        }

        protected override void WriteByte(byte value)
        {
            throw new GameFrameworkException("WriteByte is not supported in AndroidFileSystemStream.");
        }

        protected override void Write(byte[] buffer, int startIndex, int length)
        {
            throw new GameFrameworkException("Write is not supported in AndroidFileSystemStream.");
        }

        protected override void Flush()
        {
            throw new GameFrameworkException("Flush is not supported in AndroidFileSystemStream.");
        }

        protected override void Close()
        {
            InternalClose();
            mFileStream.Dispose();
        }

        private AndroidJavaObject InternalOpen(string fileName)
        {
            return s_AssetManager.Call<AndroidJavaObject>("open", fileName);
        }

        private int InternalAvailable()
        {
            return mFileStream.Call<int>("available");
        }

        private void InternalClose()
        {
            mFileStream.Call("close");
        }

        private int InternalRead()
        {
            return mFileStream.Call<int>("read");
        }

        private int InternalRead(int length, out byte[] result)
        {
#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            IntPtr resultPtr = AndroidJNI.NewByteArray(length);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            int offset = 0;
            int bytesLeft = length;
            while (bytesLeft > 0)
            {
                s_InternalReadArgs[0] = new jvalue() { l = resultPtr };
                s_InternalReadArgs[1] = new jvalue() { i = offset };
                s_InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                int bytesRead = AndroidJNI.CallIntMethod(mFileStreamRawObject, s_InternalReadMethodId, s_InternalReadArgs);
                if (bytesRead <= 0)
                {
                    break;
                }

                offset += bytesRead;
                bytesLeft -= bytesRead;
            }

#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            result = AndroidJNI.FromByteArray(resultPtr);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            AndroidJNI.DeleteLocalRef(resultPtr);
            return offset;
        }

        private void InternalReset()
        {
            mFileStream.Call("reset");
        }

        private long InternalSkip(long offset)
        {
            return mFileStream.Call<long>("skip", offset);
        }
    }
}
