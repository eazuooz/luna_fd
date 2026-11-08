using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    /// <summary>Game-specific form ordering; Game Framework owns asynchronous loading and closing.</summary>
    public class UIManager : MonoBehaviour, IManager
    {
        public UIAddressable uIAddressable;
        public AssetBundle uiAssetBundle;
        public Canvas rootCanvas;
        private readonly Queue<UI_TYPE> requests = new Queue<UI_TYPE>();
        private readonly List<UIBase> forms = new List<UIBase>();
        private UIComponent frameworkUI;
        private EventComponent events;
        private UI_TYPE loadingType;
        private int loadingSerial;
        private bool ready;
        private const string Group = "Luna";

        public void Push(UI_TYPE type)
        {
            if (type == UI_TYPE.NONE || loadingType == type || requests.Contains(type)) return;
            foreach (var form in forms) if (form != null && form.type == type) return;
            requests.Enqueue(type);
        }

        public void ManagerInitialize()
        {
            frameworkUI = GameEntry.GetComponent<UIComponent>();
            events = GameEntry.GetComponent<EventComponent>();
            frameworkUI.AddUIGroup(Group, 0);
            // Existing Luna forms create dynamic children in OnInit. Retire them on close.
            frameworkUI.InstanceCapacity = 0;
            frameworkUI.InstanceExpireTime = 0f;
            events.Subscribe(OpenUIFormFailureEventArgs.EventId, OnFailure);
            ready = true;
        }

        public void ManagerUpdate()
        {
            if (!ready || loadingType != UI_TYPE.NONE || requests.Count == 0) return;
            foreach (var pool in GameEntry.GetComponent<ObjectPoolComponent>().GetAllObjectPools())
                if (pool.Name == "UI Instance Pool") pool.ReleaseAllUnused();
            var type = requests.Dequeue();
            var data = uIAddressable != null ? uIAddressable.GetUIAddressableDataByUIType(type) : null;
            if (data == null || data.reference == null || !data.reference.RuntimeKeyIsValid())
            {
                Debug.LogError("Missing Luna UI address: " + type);
                return;
            }
            loadingType = type;
            loadingSerial = frameworkUI.OpenUIForm(data.reference.RuntimeKey.ToString(), Group, false, data);
        }

        internal void RegisterOpened(UIBase form)
        {
            forms.Add(form);
            loadingType = UI_TYPE.NONE;
            loadingSerial = 0;
        }

        public void Pop(UI_TYPE type)
        {
            var keep = new Queue<UI_TYPE>();
            while (requests.Count > 0) { var item = requests.Dequeue(); if (item != type) keep.Enqueue(item); }
            while (keep.Count > 0) requests.Enqueue(keep.Dequeue());
            if (!ready) return;
            if (loadingType == type)
            {
                frameworkUI.CloseUIForm(loadingSerial);
                loadingType = UI_TYPE.NONE;
                loadingSerial = 0;
            }
            for (int i = forms.Count - 1; i >= 0; --i)
            {
                var form = forms[i];
                if (form == null) { forms.RemoveAt(i); continue; }
                if (form.type != type) continue;
                forms.RemoveAt(i);
                frameworkUI.CloseUIForm(form.UIForm);
            }
            RefreshVisibility();
        }

        public void ClearSceneForms()
        {
            requests.Clear();
            loadingType = UI_TYPE.NONE;
            loadingSerial = 0;
            // Clear our bookkeeping before callbacks: UIPlayer.OnClear also closes UISkill.
            forms.Clear();
            if (!ready) return;
            frameworkUI.CloseAllLoadingUIForms();
            frameworkUI.CloseAllLoadedUIForms();
            requests.Clear();
        }

        public bool CompareTop(UI_TYPE type)
        {
            return forms.Count > 0 && forms[forms.Count - 1] != null && forms[forms.Count - 1].type == type;
        }

        internal void RefreshVisibility()
        {
            bool covered = false;
            for (int i = forms.Count - 1; i >= 0; --i)
            {
                var form = forms[i];
                if (form == null) continue;
                form.SetDepth(i);
                if (form.IsFullScreen && covered) form.UIInActive();
                else form.UIActive();
                if (form.IsFullScreen) covered = true;
            }
        }

        private void OnFailure(object sender, GameEventArgs args)
        {
            var failure = (OpenUIFormFailureEventArgs)args;
            if (failure.SerialId != loadingSerial) return;
            Debug.LogError("Luna UI load failed: " + loadingType + ": " + failure.ErrorMessage);
            loadingType = UI_TYPE.NONE;
            loadingSerial = 0;
        }

        public void ManagerLoop() { }
        public void ManagerClear()
        {
            ready = false;
            requests.Clear();
            if (events != null) events.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnFailure);
            forms.Clear();
        }
    }
}
