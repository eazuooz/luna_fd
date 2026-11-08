using GameFramework.Procedure;
using UnityEngine.SceneManagement;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace EverydayDevup.Framework
{
    public sealed class LunaLoadingProcedure : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            Game.Instance.lunaSceneManager.EnterCurrentScene();
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "Lobby") ChangeState<LunaLobbyProcedure>(owner);
            else if (scene == "Game" || scene == "Dungeon") ChangeState<LunaBattleProcedure>(owner);
        }
    }
    public sealed class LunaLobbyProcedure : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            Game.Instance.lunaSceneManager.EnterCurrentScene();
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "Game" || scene == "Dungeon") ChangeState<LunaBattleProcedure>(owner);
            else if (scene == "Loading") ChangeState<LunaLoadingProcedure>(owner);
        }
    }
    public sealed class LunaBattleProcedure : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            Game.Instance.lunaSceneManager.EnterCurrentScene();
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "Lobby") ChangeState<LunaLobbyProcedure>(owner);
            else if (scene == "Loading") ChangeState<LunaLoadingProcedure>(owner);
        }
    }
}
