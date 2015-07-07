using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;

using SharpDX;

using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace SDKSlutty_Ryze
{

    internal class Ryze
    {
        public const string ChampName = "Ryze";
        public const string Menuname = "Slutty Ryze";
        public static Menu Config;
        public static Spell Q, W, E, R, Qn;
        // private static SpellSlot Ignite;

        public static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static int[] abilitySequence;
        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

        public static void OnLoad(object sender, EventArgs e)
        {
            if (Player.ChampionName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 865);
            Qn = new Spell(SpellSlot.Q, 865);
            W = new Spell(SpellSlot.W, 585);
            E = new Spell(SpellSlot.E, 585);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.26f, 50f, 1700f, true, SkillshotType.SkillshotLine);
            Qn.SetSkillshot(0.26f, 50f, 1700f, false, SkillshotType.SkillshotLine);

            abilitySequence = new int[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 3, 2, 2, 3, 4, 3, 3};

            Config = new Menu("Slutty Ryze", "Slutty Ryze", true);
            Bootstrap.Init(new string[] { });

            var comboMenu = new Menu("combo", "Combos");
            {
                comboMenu.Add(new MenuBool("useQ", "Use Q", true));
                comboMenu.Add(new MenuBool("useW", "Use W", true));
                comboMenu.Add(new MenuBool("useE", "Use E", true));
                comboMenu.Add(new MenuBool("useR", "Use R"));
                comboMenu.Add(new MenuBool("useRww", "Use R Only when Rooted", true));
                comboMenu.Add(new MenuList<string>("combooptions", "Combo Mode", new[] { "Stable", "Beta Combo" }));

                Config.Add(comboMenu);
            }

            var laneMenu = new Menu("laneclear", "Lane Clear");
            {
                laneMenu.Add(new MenuBool("useQ", "Use Q", true));
                laneMenu.Add(new MenuBool("useQlc", "Use Q Last hit", true));
                laneMenu.Add(new MenuBool("useWlc", "Use W Last hit", true));
                laneMenu.Add(new MenuBool("useElc", "Use E Last Hit", true));
                laneMenu.Add(new MenuBool("useRl", "Use R When X Minions", true));
                laneMenu.Add(new MenuBool("spellblock", "Don't Use Spells when to pop passive"));

                Config.Add(laneMenu);
            }

            Config.Attach();

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    Combo();
                    break;

                case OrbwalkerMode.LastHit:
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.Hybrid:
                    break;
            }
        }

        private static int GetPassiveBuff
        {
            get
            {
                var data = ObjectManager.Player.Buffs.FirstOrDefault(b => b.DisplayName == "RyzePassiveStack");
                return data != null ? data.Count : 0;
            }
        }

        private static void Combo()
        {

            // Ignite = Player.GetSpellSlot("summonerdot");
            var qSpell = Config["combo"]["useQ"].GetValue<MenuBool>().Value;
            var eSpell = Config["combo"]["useE"].GetValue<MenuBool>().Value;
            var wSpell = Config["combo"]["useW"].GetValue<MenuBool>().Value;
            var rSpell = Config["combo"]["useR"].GetValue<MenuBool>().Value;
            var rwwSpell = Config["combo"]["useRww"].GetValue<MenuBool>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range);
            var combooptions = Config["combo"]["combooptions"].GetValue<MenuList>();

            if (!target.IsValidTarget(Q.Range))
            {
                return;
            }


            switch (combooptions.Index)
            {
                case 1:
                    if (R.IsReady())
                    {
                        if (GetPassiveBuff == 1
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if ((Player.Distance(target) < W.Range)  && (wSpell) && 
                                W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }


                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 2)
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                   )
                                {
                                    if (target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                     /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/ )
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW")
                                        && (Q.IsReady() || W.IsReady() || E.IsReady()))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell
                                        && (Q.IsReady() || W.IsReady() || E.IsReady()))
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 4)
                        {
                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (target.IsValidTarget(Qn.Range)
                                && Q.IsReady()
                                && qSpell)
                            {
                                Qn.Cast(target);
                            }
                            if (target.IsValidTarget(E.Range)
                                && E.IsReady()
                                && eSpell)
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!Q.IsReady() && !W.IsReady() && !E.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!E.IsReady() && !Q.IsReady() && !W.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }
                    }

                    if (!R.IsReady())
                    {
                        if (GetPassiveBuff == 1
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }

                        if (GetPassiveBuff == 2)
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                        }

                        if (GetPassiveBuff == 4)
                        {
                            if (target.IsValidTarget(E.Range)
                                && E.IsReady()
                                && eSpell)
                            {
                                E.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (target.IsValidTarget(Qn.Range)
                                && Q.IsReady()
                                && qSpell)
                            {
                                Qn.Cast(target);
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }
                        }
                    }
                    break;



                case 0:

                    if (target.IsValidTarget(Q.Range))
                    {
                        if (GetPassiveBuff <= 2
                            || !Player.HasBuff("RyzePassiveStack"))
                        {
                            if (target.IsValidTarget(Q.Range)
                                && qSpell
                                && Q.IsReady())
                            {
                                Q.Cast(target);
                            }

                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }

                            if (target.IsValidTarget(E.Range)
                                && eSpell
                                && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }


                        if (GetPassiveBuff == 3)
                        {
                            if (Q.IsReady()
                                && target.IsValidTarget(Q.Range))
                            {
                                {
                                    Qn.Cast(target);
                                }
                            }
                            if (E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                {
                                    W.CastOnUnit(target);
                                }
                            }
                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }

                        }

                        if (GetPassiveBuff == 4)
                        {
                            if (target.IsValidTarget(W.Range)
                                && wSpell
                                && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (target.IsValidTarget(Qn.Range)
                                && Q.IsReady()
                                && qSpell)
                            {
                                Qn.Cast(target);
                            }
                            if (target.IsValidTarget(E.Range)
                                && E.IsReady()
                                && eSpell)
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                        if (Player.HasBuff("ryzepassivecharged"))
                        {
                            if (wSpell
                                && W.IsReady()
                                && target.IsValidTarget(W.Range))
                            {
                                W.CastOnUnit(target);
                            }

                            if (qSpell
                                && Qn.IsReady()
                                && target.IsValidTarget(Qn.Range))
                            {
                                Qn.Cast(target);
                            }

                            if (eSpell
                                && E.IsReady()
                                && target.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(target);
                            }

                            if (R.IsReady()
                                && rSpell)
                            {
                                if (target.IsValidTarget(W.Range)
                                    /*&& target.Health > (Q.GetDamage(target) + E.GetDamage(target)*/)
                                {
                                    if (rwwSpell && target.HasBuff("RyzeW"))
                                    {
                                        R.Cast();
                                    }
                                    if (!rwwSpell)
                                    {
                                        R.Cast();
                                    }
                                    if (!E.IsReady() && !Q.IsReady() && !W.IsReady())
                                    {
                                        R.Cast();
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (wSpell
                            && W.IsReady()
                            && target.IsValidTarget(W.Range))
                        {
                            W.CastOnUnit(target);
                        }

                        if (qSpell
                            && Qn.IsReady()
                            && target.IsValidTarget(Qn.Range))
                        {
                            Qn.Cast(target);
                        }

                        if (eSpell
                            && E.IsReady()
                            && target.IsValidTarget(E.Range))
                        {
                            E.CastOnUnit(target);
                        }

                    }
                    break;
            }

            if (R.IsReady()
            && GetPassiveBuff == 4
            && rSpell)
            {
                if (!Q.IsReady()
                && !W.IsReady()
                && !E.IsReady())
                {
                    R.Cast();
                }
            }
        }

        private static void LaneClear()
        {

            if (GetPassiveBuff == 4
                && !Player.HasBuff("RyzeR")
                && Config["laneclear"]["spellsblock"].GetValue<MenuBool>().Value)
                return;


            var qlchSpell = Config["laneclear"]["useQlc"].GetValue<MenuBool>().Value;
            var elchSpell = Config["laneclear"]["useElc"].GetValue<MenuBool>().Value;
            var wlchSpell = Config["laneclear"]["useWlc"].GetValue<MenuBool>().Value;
            var rSpell = Config["laneclear"]["useRll"].GetValue<MenuBool>().Value;
            var minMana = Config["laneclear"]["useRll"].GetValue<MenuSlider>().Value;

            var minions =
                GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Q.Range).ToList();

            if (Player.ManaPercent <= minMana)
                return;
            foreach (var minion in minions)
            {
                if (qlchSpell
                    && Q.IsReady()
                    && Player.Distance(minions[0]) < Q.Range)
                {
                    Q.Cast(minion);
                }

                if (wlchSpell
                    && W.IsReady()
                    && Player.Distance(minions[0]) < W.Range)
                {
                    W.CastOnUnit(minion);
                }

                if (elchSpell
                    && E.IsReady()
                    && Player.Distance(minions[0]) < E.Range)
                {
                    E.CastOnUnit(minion);
                }

                if (rSpell
                    && R.IsReady()
                    && Player.Distance(minions[0]) < Q.Range
                    && minions.Count < 4)
                {
                    R.Cast();
                }
            }
        }

    }
}
