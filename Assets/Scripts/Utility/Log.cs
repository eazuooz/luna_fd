//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using System;
using System.Diagnostics;
using System.Linq;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 로그 유틸리티 클래스 - 가변인자 지원
    /// </summary>
    public static class Log
    {
        #region Debug 로그

        /// <summary>
        /// Debug 레벨 로그 (오브젝트)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(object message)
        {
            GameFrameworkLog.Debug(message);
        }

        /// <summary>
        /// Debug 레벨 로그 (문자열)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(string message)
        {
            GameFrameworkLog.Debug(message);
        }

        /// <summary>
        /// Debug 레벨 로그 (포맷 문자열 + 가변인자)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(string format, params object[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    GameFrameworkLog.Debug(format);
                }
                else
                {
                    GameFrameworkLog.Debug(string.Format(format, args));
                }
            }
            catch (FormatException ex)
            {
                string fallbackMessage = $"[FORMAT_ERROR] {format} [Args: {string.Join(", ", args?.Select(a => a?.ToString() ?? "null") ?? new string[0])}] Error: {ex.Message}";
                GameFrameworkLog.Debug(fallbackMessage);
            }
        }

        #endregion

        #region Info 로그

        /// <summary>
        /// Info 레벨 로그 (오브젝트)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(object message)
        {
            GameFrameworkLog.Info(message);
        }

        /// <summary>
        /// Info 레벨 로그 (문자열)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(string message)
        {
            GameFrameworkLog.Info(message);
        }

        /// <summary>
        /// Info 레벨 로그 (포맷 문자열 + 가변인자)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(string format, params object[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    GameFrameworkLog.Info(format);
                }
                else
                {
                    GameFrameworkLog.Info(string.Format(format, args));
                }
            }
            catch (FormatException ex)
            {
                string fallbackMessage = $"[FORMAT_ERROR] {format} [Args: {string.Join(", ", args?.Select(a => a?.ToString() ?? "null") ?? new string[0])}] Error: {ex.Message}";
                GameFrameworkLog.Info(fallbackMessage);
            }
        }

        #endregion

        #region Warning 로그

        /// <summary>
        /// Warning 레벨 로그 (오브젝트)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(object message)
        {
            GameFrameworkLog.Warning(message);
        }

        /// <summary>
        /// Warning 레벨 로그 (문자열)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(string message)
        {
            GameFrameworkLog.Warning(message);
        }

        /// <summary>
        /// Warning 레벨 로그 (포맷 문자열 + 가변인자)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(string format, params object[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    GameFrameworkLog.Warning(format);
                }
                else
                {
                    GameFrameworkLog.Warning(string.Format(format, args));
                }
            }
            catch (FormatException ex)
            {
                string fallbackMessage = $"[FORMAT_ERROR] {format} [Args: {string.Join(", ", args?.Select(a => a?.ToString() ?? "null") ?? new string[0])}] Error: {ex.Message}";
                GameFrameworkLog.Warning(fallbackMessage);
            }
        }

        #endregion

        #region Error 로그

        /// <summary>
        /// Error 레벨 로그 (오브젝트)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(object message)
        {
            GameFrameworkLog.Error(message);
        }

        /// <summary>
        /// Error 레벨 로그 (문자열)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(string message)
        {
            GameFrameworkLog.Error(message);
        }

        /// <summary>
        /// Error 레벨 로그 (포맷 문자열 + 가변인자)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(string format, params object[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    GameFrameworkLog.Error(format);
                }
                else
                {
                    GameFrameworkLog.Error(string.Format(format, args));
                }
            }
            catch (FormatException ex)
            {
                string fallbackMessage = $"[FORMAT_ERROR] {format} [Args: {string.Join(", ", args?.Select(a => a?.ToString() ?? "null") ?? new string[0])}] Error: {ex.Message}";
                GameFrameworkLog.Error(fallbackMessage);
            }
        }

        #endregion

        #region Fatal 로그

        /// <summary>
        /// Fatal 레벨 로그 (오브젝트)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_FATAL_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        [Conditional("ENABLE_FATAL_AND_ABOVE_LOG")]
        public static void Fatal(object message)
        {
            GameFrameworkLog.Fatal(message);
        }

        /// <summary>
        /// Fatal 레벨 로그 (문자열)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_FATAL_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        [Conditional("ENABLE_FATAL_AND_ABOVE_LOG")]
        public static void Fatal(string message)
        {
            GameFrameworkLog.Fatal(message);
        }

        /// <summary>
        /// Fatal 레벨 로그 (포맷 문자열 + 가변인자)
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_FATAL_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        [Conditional("ENABLE_FATAL_AND_ABOVE_LOG")]
        public static void Fatal(string format, params object[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    GameFrameworkLog.Fatal(format);
                }
                else
                {
                    GameFrameworkLog.Fatal(string.Format(format, args));
                }
            }
            catch (FormatException ex)
            {
                string fallbackMessage = $"[FORMAT_ERROR] {format} [Args: {string.Join(", ", args?.Select(a => a?.ToString() ?? "null") ?? new string[0])}] Error: {ex.Message}";
                GameFrameworkLog.Fatal(fallbackMessage);
            }
        }

        #endregion

        #region 편의 메서드들 (선택사항)

        /// <summary>
        /// 조건부 Debug 로그 - 조건이 true일 때만 출력
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void DebugIf(bool condition, string format, params object[] args)
        {
            if (condition)
            {
                Debug(format, args);
            }
        }

        /// <summary>
        /// 조건부 Warning 로그 - 조건이 true일 때만 출력
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void WarningIf(bool condition, string format, params object[] args)
        {
            if (condition)
            {
                Warning(format, args);
            }
        }

        /// <summary>
        /// 조건부 Error 로그 - 조건이 true일 때만 출력
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void ErrorIf(bool condition, string format, params object[] args)
        {
            if (condition)
            {
                Error(format, args);
            }
        }

        /// <summary>
        /// Assert 스타일 로그 - 조건이 false일 때 Error 출력
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Assert(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                Error($"[ASSERTION_FAILED] {format}", args);
            }
        }

        /// <summary>
        /// 예외와 함께 Error 로그 출력
        /// </summary>
        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Exception(Exception exception, string format = null, params object[] args)
        {
            string message = format != null ? string.Format(format, args) : exception.Message;
            Error($"{message}\n{exception}");
        }

        #endregion
    }
}

/* 
사용 예시:

public class Example
{
    public void TestLogs()
    {
        // 기존 방식과 동일하게 모두 사용 가능
        Log.Debug("Simple debug message");
        Log.Debug("Player position: {0}", playerPos);
        Log.Debug("Player {0} at position {1} with health {2}", playerName, playerPos, health);
        Log.Debug("Complex: {0}, {1}, {2}, {3}, {4}", a, b, c, d, e); // 5개 이상도 가능!
        
        // 새로운 편의 메서드들
        Log.DebugIf(isDebugMode, "Debug mode enabled for {0}", playerName);
        Log.Assert(health > 0, "Player health must be positive, got {0}", health);
        
        try
        {
            // 위험한 작업
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "Failed to load player data for {0}", playerName);
        }
    }
}

// 코드 크기 비교:
// Before: 500+ 줄 (끔찍한 제네릭 오버로드들)
// After:  200 줄 (가변인자 + 편의 메서드 포함)
//
// 기능 비교:
// Before: 최대 16개 인자까지만 지원
// After:  무제한 인자 지원
//
// 유지보수성: 훨씬 개선됨!
*/