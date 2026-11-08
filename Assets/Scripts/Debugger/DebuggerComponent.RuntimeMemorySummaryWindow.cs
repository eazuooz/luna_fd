//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed partial class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
        {
            private readonly List<Record> mRecords = new List<Record>();
            private readonly Comparison<Record> mRecordComparer = RecordComparer;
            private DateTime mSampleTime = DateTime.MinValue;
            private int mSampleCount = 0;
            private long mSampleSize = 0L;

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Runtime Memory Summary</b>");
                GUILayout.BeginVertical("box");
                {
                    if (GUILayout.Button("Take Sample", GUILayout.Height(30f)))
                    {
                        TakeSample();
                    }

                    if (mSampleTime <= DateTime.MinValue)
                    {
                        GUILayout.Label("<b>Please take sample first.</b>");
                    }
                    else
                    {
                        GUILayout.Label(Utility.Text.Format("<b>{0} Objects ({1}) obtained at {2:yyyy-MM-dd HH:mm:ss}.</b>", mSampleCount, GetByteLengthString(mSampleSize), mSampleTime.ToLocalTime()));

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("<b>Type</b>");
                            GUILayout.Label("<b>Count</b>", GUILayout.Width(120f));
                            GUILayout.Label("<b>Size</b>", GUILayout.Width(120f));
                        }
                        GUILayout.EndHorizontal();

                        for (int i = 0; i < mRecords.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(mRecords[i].Name);
                                GUILayout.Label(mRecords[i].Count.ToString(), GUILayout.Width(120f));
                                GUILayout.Label(GetByteLengthString(mRecords[i].Size), GUILayout.Width(120f));
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            private void TakeSample()
            {
                mRecords.Clear();
                mSampleTime = DateTime.UtcNow;
                mSampleCount = 0;
                mSampleSize = 0L;

                UnityEngine.Object[] samples = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
                for (int i = 0; i < samples.Length; i++)
                {
                    long sampleSize = 0L;
#if UNITY_5_6_OR_NEWER
                    sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
#else
                    sampleSize = Profiler.GetRuntimeMemorySize(samples[i]);
#endif
                    string name = samples[i].GetType().Name;
                    mSampleCount++;
                    mSampleSize += sampleSize;

                    Record record = null;
                    foreach (Record r in mRecords)
                    {
                        if (r.Name == name)
                        {
                            record = r;
                            break;
                        }
                    }

                    if (record == null)
                    {
                        record = new Record(name);
                        mRecords.Add(record);
                    }

                    record.Count++;
                    record.Size += sampleSize;
                }

                mRecords.Sort(mRecordComparer);
            }

            private static int RecordComparer(Record a, Record b)
            {
                int result = b.Size.CompareTo(a.Size);
                if (result != 0)
                {
                    return result;
                }

                result = a.Count.CompareTo(b.Count);
                if (result != 0)
                {
                    return result;
                }

                return a.Name.CompareTo(b.Name);
            }
        }
    }
}
