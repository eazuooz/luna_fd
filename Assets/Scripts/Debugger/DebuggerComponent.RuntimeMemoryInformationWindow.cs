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
        private sealed partial class RuntimeMemoryInformationWindow<T> : ScrollableDebuggerWindowBase where T : UnityEngine.Object
        {
            private const int ShowSampleCount = 300;

            private readonly List<Sample> mSamples = new List<Sample>();
            private readonly Comparison<Sample> mSampleComparer = SampleComparer;
            private DateTime mSampleTime = DateTime.MinValue;
            private long mSampleSize = 0L;
            private long mDuplicateSampleSize = 0L;
            private int mDuplicateSimpleCount = 0;

            protected override void OnDrawScrollableWindow()
            {
                string typeName = typeof(T).Name;
                GUILayout.Label(Utility.Text.Format("<b>{0} Runtime Memory Information</b>", typeName));
                GUILayout.BeginVertical("box");
                {
                    if (GUILayout.Button(Utility.Text.Format("Take Sample for {0}", typeName), GUILayout.Height(30f)))
                    {
                        TakeSample();
                    }

                    if (mSampleTime <= DateTime.MinValue)
                    {
                        GUILayout.Label(Utility.Text.Format("<b>Please take sample for {0} first.</b>", typeName));
                    }
                    else
                    {
                        if (mDuplicateSimpleCount > 0)
                        {
                            GUILayout.Label(Utility.Text.Format("<b>{0} {1}s ({2}) obtained at {3:yyyy-MM-dd HH:mm:ss}, while {4} {1}s ({5}) might be duplicated.</b>", mSamples.Count, typeName, GetByteLengthString(mSampleSize), mSampleTime.ToLocalTime(), mDuplicateSimpleCount, GetByteLengthString(mDuplicateSampleSize)));
                        }
                        else
                        {
                            GUILayout.Label(Utility.Text.Format("<b>{0} {1}s ({2}) obtained at {3:yyyy-MM-dd HH:mm:ss}.</b>", mSamples.Count, typeName, GetByteLengthString(mSampleSize), mSampleTime.ToLocalTime()));
                        }

                        if (mSamples.Count > 0)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(Utility.Text.Format("<b>{0} Name</b>", typeName));
                                GUILayout.Label("<b>Type</b>", GUILayout.Width(240f));
                                GUILayout.Label("<b>Size</b>", GUILayout.Width(80f));
                            }
                            GUILayout.EndHorizontal();
                        }

                        int count = 0;
                        for (int i = 0; i < mSamples.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(mSamples[i].Highlight ? Utility.Text.Format("<color=yellow>{0}</color>", mSamples[i].Name) : mSamples[i].Name);
                                GUILayout.Label(mSamples[i].Highlight ? Utility.Text.Format("<color=yellow>{0}</color>", mSamples[i].Type) : mSamples[i].Type, GUILayout.Width(240f));
                                GUILayout.Label(mSamples[i].Highlight ? Utility.Text.Format("<color=yellow>{0}</color>", GetByteLengthString(mSamples[i].Size)) : GetByteLengthString(mSamples[i].Size), GUILayout.Width(80f));
                            }
                            GUILayout.EndHorizontal();

                            count++;
                            if (count >= ShowSampleCount)
                            {
                                break;
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            private void TakeSample()
            {
                mSampleTime = DateTime.UtcNow;
                mSampleSize = 0L;
                mDuplicateSampleSize = 0L;
                mDuplicateSimpleCount = 0;
                mSamples.Clear();

                T[] samples = Resources.FindObjectsOfTypeAll<T>();
                for (int i = 0; i < samples.Length; i++)
                {
                    long sampleSize = 0L;
#if UNITY_5_6_OR_NEWER
                    sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
#else
                    sampleSize = Profiler.GetRuntimeMemorySize(samples[i]);
#endif
                    mSampleSize += sampleSize;
                    mSamples.Add(new Sample(samples[i].name, samples[i].GetType().Name, sampleSize));
                }

                mSamples.Sort(mSampleComparer);

                for (int i = 1; i < mSamples.Count; i++)
                {
                    if (mSamples[i].Name == mSamples[i - 1].Name && mSamples[i].Type == mSamples[i - 1].Type && mSamples[i].Size == mSamples[i - 1].Size)
                    {
                        mSamples[i].Highlight = true;
                        mDuplicateSampleSize += mSamples[i].Size;
                        mDuplicateSimpleCount++;
                    }
                }
            }

            private static int SampleComparer(Sample a, Sample b)
            {
                int result = b.Size.CompareTo(a.Size);
                if (result != 0)
                {
                    return result;
                }

                result = a.Type.CompareTo(b.Type);
                if (result != 0)
                {
                    return result;
                }

                return a.Name.CompareTo(b.Name);
            }
        }
    }
}
