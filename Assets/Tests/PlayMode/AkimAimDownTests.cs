#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VLCNP.Tests
{
    /// <summary>
    /// パーティーの Akim は下入力で照準を下げず、Orochi は下に撃てることを検証する。
    ///
    /// 背景: #682 で Akim の下撃ちを廃止し、下方向の攻撃をオロチの役割にした。
    /// Akim と Orochi はどちらも Player.prefab 由来で Fighter.canVerticalShot が有効なため、
    /// PlayerController は BaseStats の StatClass.Player(Akim だけが使う)で判定している。
    /// Prefab の StatClass が変わると判定が崩れるので、Party.prefab の実構成で確かめる。
    ///
    /// ゲーム側のコードは Assembly-CSharp にあり asmdef から参照できないため、
    /// 型名とリフレクションで呼び出す。
    /// </summary>
    public class AkimAimDownTests
    {
        const string PartyPrefabPath = "Assets/Game/Characters/Party.prefab";

        [TestCase("Akim", false)]
        [TestCase("Orochi", true)]
        public void CanAimDown_DependsOnPartyMember(string memberName, bool expected)
        {
            GameObject party = AssetDatabase.LoadAssetAtPath<GameObject>(PartyPrefabPath);
            Assert.IsNotNull(party, $"{PartyPrefabPath} が見つかりません");

            Transform member = party.transform.Find(memberName);
            Assert.IsNotNull(member, $"Party に {memberName} が見つかりません");

            Component baseStats = member.GetComponent("BaseStats");
            Assert.IsNotNull(baseStats, $"{memberName} に BaseStats がありません");

            Type controllerType = Type.GetType("VLCNP.Control.PlayerController, Assembly-CSharp");
            Assert.IsNotNull(controllerType, "PlayerController の型が見つかりません");
            MethodInfo canAimDown = controllerType.GetMethod(
                "CanAimDown",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            Assert.IsNotNull(canAimDown, "PlayerController.CanAimDown が見つかりません");

            bool actual = (bool)canAimDown.Invoke(null, new object[] { baseStats });
            Assert.AreEqual(expected, actual, $"{memberName} の下照準可否");
        }
    }
}
#endif
