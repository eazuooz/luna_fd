//------------------------------------------------------------
// Game Framework (YamYamCoding Edition) - MIT License
// Copyright © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using GameFramework.Procedure;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 프레임워크를 처음 넣었을 때 바로 돌려 볼 수 있는 시작 절차.
    ///
    /// GameFramework.prefab 의 Procedure 컴포넌트가 기본으로 이 절차로 시작한다. 같은 폴더의
    /// GameFrameworkStarter 씬을 열고 Play 하면 콘솔에 "Game Framework is running" 이 찍힌다.
    ///
    /// 자기 게임을 만들 때는 <see cref="ProcedureBase"/> 를 상속한 절차를 만들고, Procedure 인스펙터에서
    /// 그 절차에 체크한 뒤 Entrance Procedure 로 고른다. 이 절차는 체크를 풀면 된다.
    /// 게임의 절차 설정은 프리팹이 아니라 게임 씬에 둔다(프리팹은 어느 프로젝트에서나 쓰는 공통 설정이다).
    /// </summary>
    public sealed class ProcedureStarter : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // Log.Info 는 로그 심볼(ENABLE_LOG)이 없으면 컴파일에서 빠진다. 처음 넣은 프로젝트에도
            // 보이도록 Unity 로그로 남긴다.
            string otherProcedures = FindOtherProcedures();
            if (otherProcedures.Length == 0)
            {
                Debug.Log("Game Framework is running (ProcedureStarter).\n"
                          + "To start your own game, create a class that derives from GameFramework.Procedure.ProcedureBase, "
                          + "check it in the Procedure inspector (GameFramework > Procedure) and select it as Entrance Procedure.");
                return;
            }

            Debug.Log("Game Framework is running (ProcedureStarter).\n"
                      + "This project has other procedures: " + otherProcedures + ". "
                      + "This scene does not start them. Open the scene that uses them, or select one as Entrance Procedure in the Procedure inspector.");
        }

        /// <summary>프로젝트에 있는 다른 절차의 이름. 없으면 빈 문자열.</summary>
        private static string FindOtherProcedures()
        {
            List<string> names = new List<string>();
            foreach (Type type in Utility.Assembly.GetTypes())
            {
                if (type.IsAbstract || type == typeof(ProcedureStarter)) continue;
                if (!typeof(ProcedureBase).IsAssignableFrom(type)) continue;

                names.Add(type.FullName);
            }

            names.Sort(StringComparer.Ordinal);

            return string.Join(", ", names.ToArray());
        }
    }
}
