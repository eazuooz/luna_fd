//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed partial class RuntimeMemoryInformationWindow<T> : ScrollableDebuggerWindowBase where T : UnityEngine.Object
        {
            private sealed class Sample
            {
                private readonly string mName;
                private readonly string mType;
                private readonly long mSize;
                private bool mHighlight;

                public Sample(string name, string type, long size)
                {
                    mName = name;
                    mType = type;
                    mSize = size;
                    mHighlight = false;
                }

                public string Name
                {
                    get
                    {
                        return mName;
                    }
                }

                public string Type
                {
                    get
                    {
                        return mType;
                    }
                }

                public long Size
                {
                    get
                    {
                        return mSize;
                    }
                }

                public bool Highlight
                {
                    get
                    {
                        return mHighlight;
                    }
                    set
                    {
                        mHighlight = value;
                    }
                }
            }
        }
    }
}
