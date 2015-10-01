﻿using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace KonohaSwain
{
    internal class Program
    {
        public static Spell.Targeted Q, E;
        public static Spell.Skillshot W;
        public static Spell.Active R;
        public static AIHeroClient selected;
        public static Menu menu,
            ComboMenu,
            HarrassMenu,
            LaneclearMenu,
            JungleclearMenu,
            MiscMenu,
            DrawingsMenu,   
            SkinHackMenu;
        private static Dictionary<AIHeroClient, Slider> _SkinVals = new Dictionary<AIHeroClient, Slider>();
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Swain") return;
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, 1250, 275);
            E = new Spell.Targeted(SpellSlot.E, 625);
            R = new Spell.Active(SpellSlot.R);
            Loadmenu();
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += OnProc;
            _SkinVals[ObjectManager.Player].OnValueChange += Program_OnValueChange;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        static void JungleClear()
        {
            var jungleQ = LaneclearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var jungleW = LaneclearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var jungleE = LaneclearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var jungleR = LaneclearMenu["LR"].Cast<CheckBox>().CurrentValue;
            Obj_AI_Base minion =
        EntityManager.GetJungleMonsters(

            ObjectManager.Player.Position.To2D(),
            600,
            true).FirstOrDefault();
            if (minion != null)
            {

                if (jungleQ)
                    Q.Cast(minion);
                if (jungleW)
                    W.Cast(minion);
                if (jungleE)
                    E.Cast(minion);
                if (jungleR && R.Handle.ToggleState == 1)
                {
                    R.Cast();
                    lanet = true;
                }


            }
            else
            {
                if (R.Handle.ToggleState == 2)
                {
                    R.Cast();
                    lanet = false;
                }
            }
        }
        private static void Laneclear()
        {

            var laneQ = LaneclearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var laneW = LaneclearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var laneE = LaneclearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var laneR = LaneclearMenu["LR"].Cast<CheckBox>().CurrentValue;
            Obj_AI_Base minion =
         EntityManager.GetLaneMinions(
             EntityManager.UnitTeam.Enemy,
             ObjectManager.Player.Position.To2D(),
             600,
             true).FirstOrDefault();
            if (minion != null)
            {

                if (laneQ)
                    Q.Cast(minion);
                if (laneW)
                    W.Cast(minion);
                if (laneE)
                    E.Cast(minion);
                if (laneR && R.Handle.ToggleState == 1)
                {
                    R.Cast();
                    lanet = true;
                }


            }
            else
            {
                if (R.Handle.ToggleState == 2)
                {
                    R.Cast();
                    lanet = false;
                }
            }

        }

        public static bool lanet , julet;
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = DrawingsMenu["Draw Q"].Cast<CheckBox>().CurrentValue;
            var drawW = DrawingsMenu["Draw W"].Cast<CheckBox>().CurrentValue;
            var drawE = DrawingsMenu["Draw E"].Cast<CheckBox>().CurrentValue;
            var drawR = DrawingsMenu["Draw R"].Cast<CheckBox>().CurrentValue;
            if(drawQ)
                Circle.Draw(Color.SlateBlue, Q.Range, Player.Instance.Position);
            if(drawW)
                Circle.Draw(Color.SlateBlue, W.Range, Player.Instance.Position);
            if(drawE)
                Circle.Draw(Color.SlateBlue, E.Range, Player.Instance.Position);
            if(drawR)
                Circle.Draw(Color.SlateBlue,700, Player.Instance.Position);
            if (selected != null && selected.IsVisible)
            {
                Circle.Draw(Color.Red, 100, selected.Position);
            }
        }

        public static bool Rac;
        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            else
            {
                if (Rac == true && R.Handle.ToggleState == 2)
                {
                    R.Cast();
                    Rac = false;
                }


            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Laneclear();
                JungleClear();
                if (lanet == true && R.Handle.ToggleState == 2)
                {
                    R.Cast();
                    Rac = false;
                }
            }
        }

        private static void Combo()
        {
            var comboQ = ComboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var comboW = ComboMenu["CW"].Cast<CheckBox>().CurrentValue;
            var comboE = ComboMenu["CE"].Cast<CheckBox>().CurrentValue;
            var comboR = ComboMenu["CR"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(700, DamageType.Magical);
            if ( selected != null && selected.IsVisible && selected.Position.Distance(ObjectManager.Player) <= 570) target = selected;
            if (target != null)
            {

                var Wpred = W.GetPrediction(target);
                if(comboE)
                E.Cast(target);
                if (comboQ&&ObjectManager.Player.Distance(target) <= 550)
                    Q.Cast(target);
                if(target.HasBuffOfType(BuffType.Slow))
                W.Cast(Wpred.UnitPosition);
        
                if (comboR&&R.Handle.ToggleState == 1)
                {
                    R.Cast();
                    Rac = true;
                }
            }
            if (target == null && R.Handle.ToggleState == 2)
            {
                R.Cast();
                Rac = false;
            }

        }

        private static void OnProc(WndEventArgs args)
        {

            if (args.Msg != (uint)WindowMessages.LeftButtonDown)
            {
                return;
            }
            var trys = HeroManager.Enemies
              .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
              .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
            if (trys != null)
            {
                selected = HeroManager.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
            }

        }
        private static void Loadmenu()
        {
            menu = MainMenu.AddMenu("Swain", "Swain");
            ComboMenu = menu.AddSubMenu("Combo", "Combomenu");
            HarrassMenu = menu.AddSubMenu("Harrass", "Harrassmenu");
            LaneclearMenu = menu.AddSubMenu("Laneclear", "Laneclearmenu");
            JungleclearMenu = menu.AddSubMenu("Jungleclear", "Jungleclearmenu");
            MiscMenu = menu.AddSubMenu("Misc", "Miscmenu");
            DrawingsMenu = menu.AddSubMenu("Drawings", "Drawingsmenu");
            ComboMenu.Add("CQ", new CheckBox("Use Q"));
            ComboMenu.Add("CW", new CheckBox("Use W"));
            ComboMenu.Add("CE", new CheckBox("Use E"));
            ComboMenu.Add("CR", new CheckBox("Use R"));
            ComboMenu.Add("StopRMana%", new Slider("Stop R when ur MP %", 1, 0, 100));
            ComboMenu.Add("ManualR", new CheckBox("Manual R"));
            HarrassMenu.Add("HQ", new CheckBox("Use Q"));
            HarrassMenu.Add("HE", new CheckBox("Use E"));
            HarrassMenu.Add("HR", new CheckBox("Use R"));
            LaneclearMenu.Add("LQ", new CheckBox("Use Q"));
            LaneclearMenu.Add("Lw", new CheckBox("Use W"));
            LaneclearMenu.Add("LE", new CheckBox("Use E"));
            LaneclearMenu.Add("LR", new CheckBox("Use R"));
            JungleclearMenu.Add("JQ", new CheckBox("Use Q"));
            JungleclearMenu.Add("JW", new CheckBox("Use W"));
            JungleclearMenu.Add("JE", new CheckBox("Use E"));
            JungleclearMenu.Add("JR", new CheckBox("Use R"));
            MiscMenu.Add("Antigapclosers", new CheckBox("Use W Antigapclosers"));
            MiscMenu.Add("RecoverHp", new Slider("Use R when ur HP % ", 1, 0, 100));
            DrawingsMenu.Add("Draw Q", new CheckBox("Draw Q"));
            DrawingsMenu.Add("Draw W", new CheckBox("Draw W"));
            DrawingsMenu.Add("Draw E", new CheckBox("Draw E"));
            DrawingsMenu.Add("Draw R", new CheckBox("Draw R"));
            SkinHackMenu = menu.AddSubMenu("SkinHack", "SkinHack");
            var slid = SkinHackMenu.Add("Skin", new Slider("SkinHack", 0, 0, 3));
            Player.SetSkinId(slid.CurrentValue);
            _SkinVals.Add(ObjectManager.Player, slid);
        }
         private static void Program_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            var hero = ObjectManager.Get<AIHeroClient>().Where(x => x.BaseSkinName == sender.DisplayName.Replace("Skin ID ", "")).FirstOrDefault();
            if (hero == null)
                return;
            hero.SetSkinId(args.NewValue);
        }
    }
}
