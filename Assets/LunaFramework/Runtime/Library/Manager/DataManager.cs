using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace EverydayDevup.Framework
{
    public class DataManager : MonoBehaviour, IManager
    {
        #region STAGE_DATA

        Subject _selectSubject;
        public Subject currentStageString_Subject
        {
            get
            {
                if (_selectSubject == null)
                {
                    _selectSubject = new Subject();
                }

                return _selectSubject;
            }
        }

        public GameObject[] lobbyUnits = new GameObject[7];
        public List<GameObject> AllStagePrefab;
        private GameObject selectStage;
        public GameObject SelectStagePrefab
        {
            set { selectStage = value; }
            get { return selectStage; }
        }
        public void SetStagePrefab(string stageName)
        {
            GameObject stage = AllStagePrefab?.Find(candidate => candidate != null && candidate.name == stageName);
            if (stage == null)
                throw new System.ArgumentException("Unknown stage: " + stageName, nameof(stageName));
            SelectStagePrefab = stage;
        }
        public string NextStageString()
        {
            string returnStage = currentStageString;

            returnStage 
                = returnStage.Replace("Stage", "");

            string[] strStage 
                = returnStage.Split(new char[] {'-'});
            
            int stageFront = int.Parse(strStage[0]);
            int stageBack = int.Parse(strStage[1]);

            stageBack++;
            if(stageBack == 11)
            {
                stageBack = 1;
                stageFront++;
            }

            returnStage 
                = "Stage" 
                + stageFront.ToString("D2") 
                + "-" 
                + stageBack.ToString("D2");
            
            if(stageFront > 3)
            {
                returnStage = "Stage01-01";
            }

            return returnStage;
        }
        public string NextStageLevelString()
        {
            string returnStage = currentStageString;

            returnStage
                = returnStage.Replace("Stage", "");

            string[] strStage
                = returnStage.Split(new char[] { '-' });

            int stageFront = int.Parse(strStage[0]);
            int stageBack = int.Parse(strStage[1]);

            stageBack++;
            if (stageBack == 11)
            {
                stageBack = 1;
                stageFront++;
            }

            returnStage = (((stageFront - 1) * 10) + stageBack).ToString() + "층";

            return returnStage;
        }
        public string CurrentStageLevelString(string input = "")
        {
            string returnStage = currentStageString;

            returnStage
                = returnStage.Replace("Stage", "");

            string[] strStage
                = returnStage.Split(new char[] { '-' });

            int stageFront = int.Parse(strStage[0]);
            int stageBack = int.Parse(strStage[1]);

            returnStage = ( ((stageFront - 1) * 10) + stageBack ).ToString() + "층";

            return input + returnStage;
        }
        private string currentStageString = "Stage01-01";
        public string CurrentStageName
        {
            set 
            {
                currentStageString = value;
                //관찰자들에게 알림
                currentStageString_Subject.OnNotify();
            }
            get 
            {
                return currentStageString; 
            }
        }
        private string selectDungeonString = "";
        public string SelectDungeonString
        {
            set { selectDungeonString = value; }
            get { return selectDungeonString; }
        }
        //미니맵용 중간보스 체크
        //private List<GameObject> middleBoss
        //    = new List<GameObject>();
        //public List<GameObject> MiddleBoss
        //{
        //    set { middleBoss = value; }
        //    get { return middleBoss; }
        //}

        //private List<GameObject> instHeroes
        //    = new List<GameObject>();
        //public List<GameObject> InstHeroes
        //{
        //    set { instHeroes = value; }
        //    get { return instHeroes; }
        //}
        #endregion
        #region DECK_DATA
        private List<GameObject> heroePrefabs = new List<GameObject>();
        private List<GameObject> soldierPrefabs = new List<GameObject>();
        public List<GameObject> HeroePrefabs
        {
            set { heroePrefabs = value; }
            get { return heroePrefabs; }
        }
        public List<GameObject> SoldierPrefabs
        {
            set { soldierPrefabs = value; }
            get { return soldierPrefabs; }
        }
        #endregion

        #region PLAYER DATA
        private GameObject lunaPlayer;
        public GameObject LunaPlayer
        {
            set { lunaPlayer = value; }
            get { return lunaPlayer; }
        }

        public GameObject[] Players;
        #endregion
        #region EQUIPDESIGN
        private int equipDesignItemNumber;
        public int EquipDesignNumber
        {
            set { equipDesignItemNumber = value; }
            get { return equipDesignItemNumber; }
        }
        #endregion
        #region PICKUP ITEM
        [System.NonSerialized] public Dictionary<int, int> pickUpData
            = new Dictionary<int, int>();

        public int pickItemNumber;
        #endregion

        public List<AudioSource> StageSounds { get; set; } = new List<AudioSource>();
        public List<AudioSource> ObjectSounds { get; set; } = new List<AudioSource>();
        private GameObject pickingGameObject;
        public GameObject PickingGameObject
        {
            get { return pickingGameObject; }
            set { pickingGameObject = value; }                   
        }
        public GameObject selectDungeon;
        public bool doNotSummonMode = false;

        public GameObject chatManager;
        public string chatStr;
        public string OnClickSkillName;
        public void ManagerInitialize()
        {
            // Next-stage and direct battle entry must work before the stage chooser is opened.
            AllStagePrefab = new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/10Stage"));
        }
        public void ManagerClear()
        {
            //throw new System.NotImplementedException();
        }
        public void ManagerLoop()
        {
            //throw new System.NotImplementedException();
        }
        public void ManagerUpdate()
        {
            //throw new System.NotImplementedException();
        }
    }
}