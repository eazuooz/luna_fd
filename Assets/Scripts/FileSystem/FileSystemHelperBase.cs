//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.FileSystem;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class FileSystemHelperBase : MonoBehaviour, IFileSystemHelper
    {
        public abstract FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew);
    }
}
