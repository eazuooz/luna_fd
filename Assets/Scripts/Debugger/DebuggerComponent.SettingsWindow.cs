//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class SettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerComponent mDebuggerComponent = null;
            private SettingComponent mSettingComponent = null;
            private float mLastIconX = 0f;
            private float mLastIconY = 0f;
            private float mLastWindowX = 0f;
            private float mLastWindowY = 0f;
            private float mLastWindowWidth = 0f;
            private float mLastWindowHeight = 0f;
            private float mLastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                mDebuggerComponent = GameEntry.GetComponent<DebuggerComponent>();
                if (mDebuggerComponent == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                mSettingComponent = GameEntry.GetComponent<SettingComponent>();
                if (mSettingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                mLastIconX = mSettingComponent.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                mLastIconY = mSettingComponent.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                mLastWindowX = mSettingComponent.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                mLastWindowY = mSettingComponent.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                mLastWindowWidth = mSettingComponent.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                mLastWindowHeight = mSettingComponent.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                mDebuggerComponent.WindowScale = mLastWindowScale = mSettingComponent.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                mDebuggerComponent.IconRect = new Rect(mLastIconX, mLastIconY, DefaultIconRect.width, DefaultIconRect.height);
                mDebuggerComponent.WindowRect = new Rect(mLastWindowX, mLastWindowY, mLastWindowWidth, mLastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (mLastIconX != mDebuggerComponent.IconRect.x)
                {
                    mLastIconX = mDebuggerComponent.IconRect.x;
                    mSettingComponent.SetFloat("Debugger.Icon.X", mDebuggerComponent.IconRect.x);
                }

                if (mLastIconY != mDebuggerComponent.IconRect.y)
                {
                    mLastIconY = mDebuggerComponent.IconRect.y;
                    mSettingComponent.SetFloat("Debugger.Icon.Y", mDebuggerComponent.IconRect.y);
                }

                if (mLastWindowX != mDebuggerComponent.WindowRect.x)
                {
                    mLastWindowX = mDebuggerComponent.WindowRect.x;
                    mSettingComponent.SetFloat("Debugger.Window.X", mDebuggerComponent.WindowRect.x);
                }

                if (mLastWindowY != mDebuggerComponent.WindowRect.y)
                {
                    mLastWindowY = mDebuggerComponent.WindowRect.y;
                    mSettingComponent.SetFloat("Debugger.Window.Y", mDebuggerComponent.WindowRect.y);
                }

                if (mLastWindowWidth != mDebuggerComponent.WindowRect.width)
                {
                    mLastWindowWidth = mDebuggerComponent.WindowRect.width;
                    mSettingComponent.SetFloat("Debugger.Window.Width", mDebuggerComponent.WindowRect.width);
                }

                if (mLastWindowHeight != mDebuggerComponent.WindowRect.height)
                {
                    mLastWindowHeight = mDebuggerComponent.WindowRect.height;
                    mSettingComponent.SetFloat("Debugger.Window.Height", mDebuggerComponent.WindowRect.height);
                }

                if (mLastWindowScale != mDebuggerComponent.WindowScale)
                {
                    mLastWindowScale = mDebuggerComponent.WindowScale;
                    mSettingComponent.SetFloat("Debugger.Window.Scale", mDebuggerComponent.WindowScale);
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Window Settings</b>");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Position:", GUILayout.Width(60f));
                        GUILayout.Label("Drag window caption to move position.");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float width = mDebuggerComponent.WindowRect.width;
                        GUILayout.Label("Width:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            width--;
                        }
                        width = GUILayout.HorizontalSlider(width, 100f, Screen.width - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            width++;
                        }
                        width = Mathf.Clamp(width, 100f, Screen.width - 20f);
                        if (width != mDebuggerComponent.WindowRect.width)
                        {
                            mDebuggerComponent.WindowRect = new Rect(mDebuggerComponent.WindowRect.x, mDebuggerComponent.WindowRect.y, width, mDebuggerComponent.WindowRect.height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float height = mDebuggerComponent.WindowRect.height;
                        GUILayout.Label("Height:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            height--;
                        }
                        height = GUILayout.HorizontalSlider(height, 100f, Screen.height - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            height++;
                        }
                        height = Mathf.Clamp(height, 100f, Screen.height - 20f);
                        if (height != mDebuggerComponent.WindowRect.height)
                        {
                            mDebuggerComponent.WindowRect = new Rect(mDebuggerComponent.WindowRect.x, mDebuggerComponent.WindowRect.y, mDebuggerComponent.WindowRect.width, height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float scale = mDebuggerComponent.WindowScale;
                        GUILayout.Label("Scale:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            scale -= 0.01f;
                        }
                        scale = GUILayout.HorizontalSlider(scale, 0.5f, 4f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            scale += 0.01f;
                        }
                        scale = Mathf.Clamp(scale, 0.5f, 4f);
                        if (scale != mDebuggerComponent.WindowScale)
                        {
                            mDebuggerComponent.WindowScale = scale;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("0.5x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 0.5f;
                        }
                        if (GUILayout.Button("1.0x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 1f;
                        }
                        if (GUILayout.Button("1.5x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 1.5f;
                        }
                        if (GUILayout.Button("2.0x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 2f;
                        }
                        if (GUILayout.Button("2.5x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 2.5f;
                        }
                        if (GUILayout.Button("3.0x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 3f;
                        }
                        if (GUILayout.Button("3.5x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 3.5f;
                        }
                        if (GUILayout.Button("4.0x", GUILayout.Height(60f)))
                        {
                            mDebuggerComponent.WindowScale = 4f;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Layout", GUILayout.Height(30f)))
                    {
                        mDebuggerComponent.ResetLayout();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
