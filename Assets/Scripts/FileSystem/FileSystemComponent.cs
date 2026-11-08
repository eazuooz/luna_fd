//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.FileSystem;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/File System")]
    public sealed class FileSystemComponent : GameFrameworkComponent
    {
        private IFileSystemManager mFileSystemManager = null;

        [SerializeField]
        private string mFileSystemHelperTypeName = "UnityGameFramework.Runtime.DefaultFileSystemHelper";

        [SerializeField]
        private FileSystemHelperBase mCustomFileSystemHelper = null;

        public int Count
        {
            get
            {
                return mFileSystemManager.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mFileSystemManager = GameFrameworkEntry.GetModule<IFileSystemManager>();
            if (mFileSystemManager == null)
            {
                Log.Fatal("File system manager is invalid.");
                return;
            }

            FileSystemHelperBase fileSystemHelper = Helper.CreateHelper(mFileSystemHelperTypeName, mCustomFileSystemHelper);
            if (fileSystemHelper == null)
            {
                Log.Error("Can not create fileSystem helper.");
                return;
            }

            fileSystemHelper.name = "FileSystem Helper";
            Transform transform = fileSystemHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mFileSystemManager.SetFileSystemHelper(fileSystemHelper);
        }

        private void Start()
        {
        }

        public bool HasFileSystem(string fullPath)
        {
            return mFileSystemManager.HasFileSystem(fullPath);
        }

        public IFileSystem GetFileSystem(string fullPath)
        {
            return mFileSystemManager.GetFileSystem(fullPath);
        }

        public IFileSystem CreateFileSystem(string fullPath, FileSystemAccess access, int maxFileCount, int maxBlockCount)
        {
            return mFileSystemManager.CreateFileSystem(fullPath, access, maxFileCount, maxBlockCount);
        }

        public IFileSystem LoadFileSystem(string fullPath, FileSystemAccess access)
        {
            return mFileSystemManager.LoadFileSystem(fullPath, access);
        }

        public void DestroyFileSystem(IFileSystem fileSystem, bool deletePhysicalFile)
        {
            mFileSystemManager.DestroyFileSystem(fileSystem, deletePhysicalFile);
        }

        public IFileSystem[] GetAllFileSystems()
        {
            return mFileSystemManager.GetAllFileSystems();
        }

        public void GetAllFileSystems(List<IFileSystem> results)
        {
            mFileSystemManager.GetAllFileSystems(results);
        }
    }
}
