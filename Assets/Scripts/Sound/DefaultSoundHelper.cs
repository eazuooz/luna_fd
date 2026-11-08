//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


namespace UnityGameFramework.Runtime
{
    public class DefaultSoundHelper : SoundHelperBase
    {
        private ResourceComponent mResourceComponent = null;

        public override void ReleaseSoundAsset(object soundAsset)
        {
            mResourceComponent.UnloadAsset(soundAsset);
        }

        private void Start()
        {
            mResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (mResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}
