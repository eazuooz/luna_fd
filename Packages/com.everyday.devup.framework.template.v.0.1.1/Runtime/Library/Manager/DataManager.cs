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
            for (int i = 0; i < AllStagePrefab.Count; ++i)
            {
                if (stageName == AllStagePrefab[i].name)
                {
                    Game.Instance.dataManager.SelectStagePrefab = AllStagePrefab[i];
                }
            }
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
            
            if(returnStage == "Stage03-10")
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

            returnStage = ((stageFront - 1) * 10) + stageBack.ToString() + "Ãþ";

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

            returnStage = ( ((stageFront - 1) * 10) + stageBack ).ToString() + "Ãþ";

            return input + returnStage;
        }
        private string currentStageString = "";
        public string CurrentStageName
        {
            set 
            {
                currentStageString = value;
                //°üÂûÀÚµé¿¡°Ô ¾Ë¸²
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
        //¹Ì´Ï¸Ê¿ë Áß°£º¸½º Ã¼Å©
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
        public Dictionary<int, int> pickUpData
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