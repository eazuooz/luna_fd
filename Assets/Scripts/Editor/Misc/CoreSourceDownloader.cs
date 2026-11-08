//------------------------------------------------------------
// Game Framework (YamYamCoding Edition) - MIT License
// Copyright © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unity.SharpZipLib.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityGameFramework.Editor
{
    // 코어(Game Framework)는 Unity 프로젝트 밖의 별도 C# 프로젝트이고, 이 프로젝트에는 빌드 결과인
    // Assets/Plugins/GameFramework.dll 만 들어 있다. DLL 만으로도 프레임워크는 그대로 동작한다.
    //
    // 코어를 디버깅하거나 고치고 싶을 때 이 메뉴로 소스를 받는다. <프로젝트>/External/GameFramework 에
    // 받으며, 그 프로젝트를 빌드하면 빌드 후 이벤트가 DLL 을 Assets/Plugins 로 복사한다.
    // Assets 밖이라 Project 창에는 보이지 않는다(Assets 안에 두면 DLL 과 같은 이름의 어셈블리가 둘이 된다).
    //
    // git 으로 클론하고, git 이 없거나 실패하면 GitHub 의 ZIP 아카이브를 받아 푼다.
    // 사용자가 메뉴를 고르고 확인 창에서 동의했을 때만 받는다. 임포트만으로는 아무것도 받지 않는다.
    public static class CoreSourceDownloader
    {
        private const string RepositoryUrl = "https://github.com/eazuooz/GameFramework.git";
        private const string ArchiveUrl = "https://github.com/eazuooz/GameFramework/archive/refs/heads/master.zip";

        private const string ExternalFolderName = "External";
        private const string CoreFolderName = "GameFramework";
        private const string SolutionFileName = "GameFramework.sln";

        /// <summary>
        /// ZIP 을 받아 푸는 임시 폴더의 접두어. 옮길 때 같은 드라이브여야 해서 External 아래에 만든다.
        /// 이름을 짧게 둔다 — 아카이브 안의 경로가 길어서, 프로젝트 경로가 깊으면 Windows 의 260자 제한에 걸린다.
        /// </summary>
        private const string DownloadFolderPrefix = ".dl-";

        private const string DialogTitle = "Game Framework Core Source";

        [MenuItem("Game Framework/Download Core Source", false, 85)]
        public static void DownloadCoreSource()
        {
            string targetDirectory = GetTargetDirectory();

            if (Directory.Exists(targetDirectory))
            {
                if (IsCoreSource(targetDirectory))
                {
                    ShowAlreadyDownloaded(targetDirectory);
                    return;
                }

                if (!IsEmptyDirectory(targetDirectory))
                {
                    UnityEngine.Debug.LogWarning("Download core source skipped: the folder exists but has no " + SolutionFileName + " → " + targetDirectory);
                    EditorUtility.DisplayDialog(DialogTitle,
                        "This folder already exists but is not the core source (" + SolutionFileName + " is missing):\n" + targetDirectory
                        + "\n\nMove or delete it, then choose Download Core Source again.",
                        "Close");
                    return;
                }

                // 이전 시도가 남긴 빈 폴더다. 지우고 새로 받는다.
                Directory.Delete(targetDirectory);
            }

            bool agreed = EditorUtility.DisplayDialog(DialogTitle,
                "This will download the Game Framework core source:\n\n  git clone " + RepositoryUrl + "\n\ninto:\n" + targetDirectory
                + "\n\nIf git is not installed, the source is downloaded as a ZIP archive instead. It needs an internet connection."
                + " The framework already works without it (Assets/Plugins/GameFramework.dll); download it only if you want to debug or modify the core.",
                "Download", "Cancel");
            if (!agreed)
            {
                return;
            }

            string method = null;
            string message = null;
            bool succeeded = false;
            try
            {
                succeeded = Download(targetDirectory, out method, out message);
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!succeeded)
            {
                UnityEngine.Debug.LogError("Download core source failure: " + message);
                EditorUtility.DisplayDialog(DialogTitle,
                    "Download failed.\n\n" + message + "\n\nYou can download it yourself:\n  git clone " + RepositoryUrl + " \"" + targetDirectory + "\"\nor\n  " + ArchiveUrl,
                    "Close");
                return;
            }

            UnityEngine.Debug.Log("Download core source complete (" + method + "): " + targetDirectory);

            string note = method == "git"
                ? string.Empty
                : "\n\ngit was not available, so the source was downloaded as a ZIP archive (no git history). To update it later, delete the folder and download again.";
            bool show = EditorUtility.DisplayDialog(DialogTitle,
                "Downloaded to:\n" + targetDirectory + note
                + "\n\nThe folder is outside Assets, so it does not appear in the Project window. Open " + SolutionFileName
                + " and build it. The build copies GameFramework.dll to Assets/Plugins, and Unity picks it up.",
                "Show Folder", "Close");
            if (show)
            {
                EditorUtility.RevealInFinder(Path.Combine(targetDirectory, SolutionFileName));
            }
        }

        internal static string GetTargetDirectory()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            return Path.Combine(projectRoot, ExternalFolderName, CoreFolderName);
        }

        /// <summary>
        /// 창을 띄우지 않고 받는다. git 으로 먼저 클론하고, 안 되면 ZIP 을 받는다.
        /// method 는 "git" 또는 "zip". 실패하면 false 와 함께 두 방법의 실패 이유를 돌려준다.
        /// </summary>
        internal static bool Download(string targetDirectory, out string method, out string message)
        {
            EditorUtility.DisplayProgressBar(DialogTitle, "Cloning " + RepositoryUrl + " ...", 0.3f);
            if (Clone(targetDirectory, out string gitMessage))
            {
                method = "git";
                message = null;
                return true;
            }

            UnityEngine.Debug.LogWarning("Download core source: git clone failed, trying the ZIP archive. " + gitMessage);

            // 실패한 클론이 폴더를 남겼을 수 있다. 받기 전에는 없던 폴더이므로 지워도 된다.
            DeleteDirectory(targetDirectory);

            if (DownloadArchive(targetDirectory, out string archiveMessage))
            {
                method = "zip";
                message = null;
                return true;
            }

            method = null;
            message = "git: " + gitMessage + "\nZIP: " + archiveMessage;
            return false;
        }

        // 창을 띄우지 않고 클론만 한다. 실패하면 false 와 함께 이유를 돌려준다.
        internal static bool Clone(string targetDirectory, out string message)
        {
            if (Directory.Exists(targetDirectory))
            {
                message = "The target directory already exists: " + targetDirectory;
                return false;
            }

            if (!RunGit("clone " + RepositoryUrl + " \"" + targetDirectory + "\"", out message))
            {
                return false;
            }

            if (!IsCoreSource(targetDirectory))
            {
                message = "The clone finished but " + SolutionFileName + " is missing: " + targetDirectory;
                return false;
            }

            return true;
        }

        /// <summary>
        /// GitHub 의 ZIP 아카이브를 받아 푼다. git 이 없는 PC 를 위한 길이다.
        /// 아카이브는 "GameFramework-master" 폴더 하나로 감싸져 있어서, 그 폴더를 대상 경로로 옮긴다.
        /// </summary>
        internal static bool DownloadArchive(string targetDirectory, out string message)
        {
            string externalDirectory = Path.GetDirectoryName(targetDirectory);
            string workDirectory = Path.Combine(externalDirectory, DownloadFolderPrefix + Guid.NewGuid().ToString("N").Substring(0, 8));
            string archivePath = Path.Combine(workDirectory, "s.zip");
            string extractDirectory = Path.Combine(workDirectory, "x");

            try
            {
                Directory.CreateDirectory(workDirectory);

                using (UnityWebRequest request = UnityWebRequest.Get(ArchiveUrl))
                {
                    request.downloadHandler = new DownloadHandlerFile(archivePath);
                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(DialogTitle, "Downloading " + ArchiveUrl + " ...", request.downloadProgress))
                        {
                            request.Abort();
                            message = "Canceled.";
                            return false;
                        }

                        Thread.Sleep(50);
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        message = request.error;
                        return false;
                    }
                }

                EditorUtility.DisplayProgressBar(DialogTitle, "Extracting ...", 0.9f);
                ZipUtility.UncompressFromZip(archivePath, null, extractDirectory);

                string[] roots = Directory.GetDirectories(extractDirectory);
                if (roots.Length != 1 || !IsCoreSource(roots[0]))
                {
                    message = "The archive does not contain " + SolutionFileName + " at the expected place.";
                    return false;
                }

                Directory.Move(roots[0], targetDirectory);
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }
            finally
            {
                DeleteDirectory(workDirectory);
            }

            message = null;
            return true;
        }

        private static void ShowAlreadyDownloaded(string targetDirectory)
        {
            UnityEngine.Debug.Log("Download core source skipped: already downloaded → " + targetDirectory);

            string text = "The core source is already downloaded:\n" + targetDirectory
                + "\n\nThe folder is outside Assets, so it does not appear in the Project window.";

            if (!Directory.Exists(Path.Combine(targetDirectory, ".git")))
            {
                if (EditorUtility.DisplayDialog(DialogTitle, text + "\n\nTo update it, delete the folder and download again.", "Show Folder", "Close"))
                {
                    EditorUtility.RevealInFinder(Path.Combine(targetDirectory, SolutionFileName));
                }

                return;
            }

            // 0: Show Folder, 1: Close, 2: Update
            int choice = EditorUtility.DisplayDialogComplex(DialogTitle, text, "Show Folder", "Close", "Update (git pull)");
            if (choice == 0)
            {
                EditorUtility.RevealInFinder(Path.Combine(targetDirectory, SolutionFileName));
                return;
            }

            if (choice != 2)
            {
                return;
            }

            bool updated;
            string message;
            try
            {
                EditorUtility.DisplayProgressBar(DialogTitle, "git pull ...", 0.5f);
                updated = RunGit("-C \"" + targetDirectory + "\" pull --ff-only", out message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!updated)
            {
                UnityEngine.Debug.LogError("Update core source failure: " + message);
                EditorUtility.DisplayDialog(DialogTitle, "Update failed.\n\n" + message, "Close");
                return;
            }

            UnityEngine.Debug.Log("Update core source complete: " + targetDirectory);
            EditorUtility.DisplayDialog(DialogTitle, "Updated. Build " + SolutionFileName + " again to refresh Assets/Plugins/GameFramework.dll.", "Close");
        }

        /// <summary>
        /// 폴더를 통째로 지운다. git 의 pack 파일은 읽기 전용이라 그대로는 Directory.Delete 가 실패한다.
        /// </summary>
        internal static void DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(directory, true);
        }

        private static bool IsCoreSource(string directory)
        {
            return File.Exists(Path.Combine(directory, SolutionFileName));
        }

        private static bool IsEmptyDirectory(string directory)
        {
            return Directory.GetFileSystemEntries(directory).Length == 0;
        }

        private static bool RunGit(string arguments, out string message)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("git", arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // 자격 증명 창이나 터미널 입력을 기다리며 에디터가 멈추지 않게 한다. 공개 저장소라 필요 없다.
            startInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    // git 은 진행 상황과 오류를 모두 표준 오류로 쓴다.
                    string output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        message = "git exited with code " + process.ExitCode + ". " + output.Trim();
                        return false;
                    }
                }
            }
            catch (Win32Exception)
            {
                message = "Can not run git. It is not installed or not on the PATH.";
                return false;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }

            message = null;
            return true;
        }
    }
}
