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
        private sealed partial class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
        {
            private sealed class Record
            {
                private readonly string mName;
                private int mCount;
                private long mSize;

                public Record(string name)
                {
                    mName = name;
                    mCount = 0;
                    mSize = 0L;
                }

                public string Name
                {
                    get
                    {
                        return mName;
                    }
                }

                public int Count
                {
                    get
                    {
                        return mCount;
                    }
                    set
                    {
                        mCount = value;
                    }
                }

                public long Size
                {
                    get
                    {
                        return mSize;
                    }
                    set
                    {
                        mSize = value;
                    }
                }
            }
        }
    }
}
