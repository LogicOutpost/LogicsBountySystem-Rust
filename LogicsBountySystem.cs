using Oxide.Core;
using Oxide.Game.Rust.Cui;
using Oxide.Game.Rust.Libraries;
using Rust.Ai.Gen2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConsoleSystem;

namespace Oxide.Plugins;
[Info("LogicsBountySystem", "LogicOutpost", "1.0.0")]
public class LogicsBountySystem : RustPlugin
{
    List<BasePlayer> OnlinePlayer = new List<BasePlayer>();
    Dictionary<ulong, ulong> SelectedTargets = new Dictionary<ulong, ulong>();
    Dictionary<ulong, int> ActiveBountys = new Dictionary<ulong, int>();
    #region Server and players
    private void OnServerInitialized(bool initial)
    {
        foreach (BasePlayer onlinePlayer in BasePlayer.activePlayerList)
        {
            OnlinePlayer.Add(onlinePlayer);
        }
    }
    private void OnPlayerConnected(BasePlayer player)
    {
        OnlinePlayer.Add(player);
    }
    private void OnPlayerDisconnected(BasePlayer player)
    {
        OnlinePlayer.Remove(player);
    }
    #endregion
    #region Bounty controls
    [ChatCommand("bounty")]
    private void BountyCommand(BasePlayer player, string command, string[] args)
    {
        OpenBountyUI(player);
    }
    [ConsoleCommand("bounty.close")] 
    private void BountyClose(ConsoleSystem.Arg arg)
    {
        BasePlayer player = arg.Player();
        if (player == null)
            return;
        CuiHelper.DestroyUi(player, "BountyUI");
    }
    [ConsoleCommand("bounty.select")]
    private void SelectBounty(ConsoleSystem.Arg arg)
    {
        BasePlayer player = arg.Player();

        if (player == null || arg.Args == null || arg.Args.Length == 0)
        {
            return;
        }

        var targetID = arg.Args[0].ToString();
        BasePlayer target = BasePlayer.FindByID(ulong.Parse(targetID));
        if (target == null)
        {
            return;
        } 
        if (player.userID == target.userID)
        {
            SendReply(player, "You can not place a bounty on yourself");
            return;
        }
        SelectedTargets[player.userID] = target.userID;
        SendReply(player, $"You selected player: {target.displayName}");
        OpenBountyAmountUI(player, target);
        CuiHelper.DestroyUi(player, "BountyUI");

    }
    [ConsoleCommand("pay.bounty")]
    private void PayBounty(ConsoleSystem.Arg arg)
    {
        BasePlayer player = arg.Player();
        if (player == null)
        {
            return;
        }
        if(!SelectedTargets.TryGetValue(player.userID, out ulong targetID))
        {
            SendReply(player, "You have not selected a target!");
            return;
        }
        BasePlayer target = BasePlayer.FindByID(targetID);
        if (target == null)
        {
            SendReply(player, "That player is not online");
            return;
        }
        int playerScrap = player.inventory.GetAmount(ItemManager.FindItemDefinition("scrap").itemid);
        if (playerScrap < bountyAmount)
        {
            SendReply(player, "You do not have enough scrap");
            return;
        }
        else
        {
            player.inventory.Take(null, ItemManager.FindItemDefinition("scrap").itemid,bountyAmount);
        }
        if (ActiveBountys.ContainsKey(target.userID))
        {
            ActiveBountys[target.userID] += bountyAmount;
        }
        else
        {
            ActiveBountys.Add(target.userID, bountyAmount);
        }
        SendReply(player, $"You placed a {bountyAmount} scrap bounty on {target.displayName}");
        SendReply(target, $"A {bountyAmount} scrap bounty has been placed on you");
        PrintToChat($"{player.displayName} has placed a {bountyAmount} scrap bounty on {target.displayName}");


            CuiHelper.DestroyUi(player, "BountyPay");
    }

    private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
    {
        
        if (entity == null)
        {
            return;
        }
        
        if (entity.IsNonNpcPlayer())
        {
            BasePlayer victim = entity as BasePlayer;
            ulong steamID = victim.userID;
            if (ActiveBountys.TryGetValue(steamID, out int bountyPayout))
            {
                BasePlayer killer = info?.InitiatorPlayer;
                if (killer == null)
                {
                    return;
                }
                else
                {
                    Item scrap = ItemManager.CreateByName("scrap", bountyPayout);
                    ActiveBountys.Remove(steamID);
                    SendReply(killer, $"You have claimed the {bountyAmount} scrap bounty on {victim}");
                    PrintToChat($"{killer} has claimed the {bountyAmount} on {victim}");
                }
            }
        }
    }
    [ConsoleCommand("bounty.back")]
    private void BountyBackPage(ConsoleSystem.Arg args)
    {
        BasePlayer player = args.Player();
        OpenBountyUI(player);
        CuiHelper.DestroyUi(player, "BountyPay");
    }
    [ConsoleCommand("raise.bounty")]
    private void RaiseBounty(ConsoleSystem.Arg args)
    {
        BasePlayer player = args.Player();
        if (player == null)
        {
            return;
        }
        if (!SelectedTargets.TryGetValue(player.userID, out ulong targetID))
            return;
        BasePlayer target = BasePlayer.FindByID(targetID);

        if (target == null)
        {
            return;
        }
        bountyAmount += 10;
        if (bountyAmount >= 1010)
        {
            return;
        }
        CuiHelper.DestroyUi(player, "BountyPay");
        OpenBountyAmountUI(player, target);
    }
    [ConsoleCommand("lower.bounty")]
    private void LowerBounty(ConsoleSystem.Arg args)
    {
        BasePlayer player = args.Player();
        if (player == null)
        {
            return;
        }
        if (!SelectedTargets.TryGetValue(player.userID, out ulong targetID))
            return;
        BasePlayer target = BasePlayer.FindByID(targetID);

        if (target == null)
        {
            return;
        }
        bountyAmount -= 10;
        if (bountyAmount <= 0)
        {
            return;
        }
        CuiHelper.DestroyUi(player, "BountyPay");
        OpenBountyAmountUI(player, target);
    }
    private void OpenBountyUI(BasePlayer player)
    {
        CuiElementContainer container = new CuiElementContainer();
        container.Add(new CuiPanel
        {
            CursorEnabled = true,
            Image = { Color = "0.1 0.1 0.1 0.9" },
            RectTransform = { AnchorMin = "0.3 0.3", AnchorMax = "0.7 0.7" }
        }, "Overlay", "BountyUI");
        container.Add(new CuiLabel
        {
            Text =
            {
                Text = "Logics Bounty System",
                FontSize = 24,
                Color = "1 1 1 1"
            }
        }, "BountyUI");

        container.Add(new CuiButton
        {
            Button =
            {
                Color = "0.2 0.2 0.2 1",
                Command = $"bounty.close"
            },
            RectTransform =
            {
                AnchorMin = "0.05 0.05",
                AnchorMax = "0.35 0.15"
            },
            Text =
            {
                Text = "Close",
                FontSize = 16,
            }
        }, "BountyUI");

        int buttonNumber = 0;

        foreach (BasePlayer onlinePlayer in OnlinePlayer)
        {
            float buttonY = 0.8f - (buttonNumber * 0.12f);

            container.Add(new CuiButton
            {

                Button =
                {
                    Color = "0.2 0.5 0.2 1",
                    Command = $"bounty.select {onlinePlayer.UserIDString}"
                },
                RectTransform =
                {
                    AnchorMin = $"0.1 {buttonY - 0.08f}",
                    AnchorMax = $"0.9 {buttonY}"
                },
                Text =
                {
                    Text = onlinePlayer.displayName,
                    FontSize = 16
                }
                
            }, "BountyUI");

           
        }

        float height = OnlinePlayer.Count * 50f;
        container.Add(new CuiElement
        {
            Name = "PlayerScroll",
            Parent = "BountyUI",
            Components =
            {
                new CuiScrollViewComponent
                {
                    ContentTransform = new CuiRectTransform
                    {
                        AnchorMin = "0 1",
                        AnchorMax = "1 1",
                        OffsetMin = $"0 {-height}",
                        OffsetMax = "0 0",
                        Pivot = "0.5 1"
                    },
                    Vertical = true,
                    Horizontal = false,
                    MovementType = ScrollRect.MovementType.Clamped,
                    Inertia = true,
                    DecelerationRate = 0.3f,
                    ScrollSensitivity = 24f,
                    VerticalScrollbar = new CuiScrollbar
                    {
                        AutoHide = true,
                        Size = 20
                    }
                },
                 new CuiRectTransformComponent
                 {
                     AnchorMin = "0.08 0.12",
                     AnchorMax = "0.92 0.78",
                     Pivot = "0.5 1"
                 }

            },


        });

        CuiHelper.AddUi(player, container);
    }

    private int bountyAmount = 10;
    private void OpenBountyAmountUI(BasePlayer player, BasePlayer Target)
    {
        CuiElementContainer container = new CuiElementContainer();
        container.Add(new CuiPanel {
            CursorEnabled = true,
            Image = { Color = "0.1 0.1 0.1 0.9" },
            RectTransform = { AnchorMin = "0.3 0.3", AnchorMax = "0.7 0.7" }
        }, "Overlay", "BountyPay");
        container.Add(new CuiLabel
        {
            Text =
            {
                Text = $"{bountyAmount} Scrap",
                FontSize = 20,
                Color = "1 1 1 1",
            },
            RectTransform =
            {
                AnchorMin = "0.40 0.64",
                AnchorMax = "0.68 0.73",

            }

        },"BountyPay");
        container.Add(new CuiButton
        {
            Text =
            {
                Text = "Back",
                FontSize = 18,
                Color = "1 1 1 1",
            },
            Button =
            {
                Color = "0.27 0.29 0.31 0.95",
                Command = $"bounty.back",
            },
            RectTransform =
            {
                AnchorMin = "0.04 0.04",
                AnchorMax = "0.28 0.14"
            }
        },"BountyPay");
        container.Add(new CuiLabel
        {
            Text =
            {
                Text = $"Target: {Target.displayName}",
                FontSize = 18,
                Color = "0.48 0.00 0.00 1.00"
            },
            RectTransform =
            {
                AnchorMin = "0.37 0.86",
                AnchorMax = "0.65 0.95"
            }
        },"BountyPay");
        container.Add(new CuiButton
        {
            Button =
            {
                Color = "0.27 0.29 0.31 0.95",
                Command = "pay.bounty"
            },
            Text =
            {
                Text = "Place Bounty",
                FontSize = 18,
                Color = "0.81 0.80 0.81 1.00"
            },
            RectTransform =
            {
                AnchorMin = "0.36 0.12",
                AnchorMax = "0.64 0.21"
            }
        }, "BountyPay");
        container.Add(new CuiButton
        {
            Button =
            {
                Color = "0.27 0.29 0.31 0.95",
                Command = $"raise.bounty"
            },
            Text =
            {
                Text = "+",
                FontSize = 18,
                Color = "0.81 0.80 0.81 1.00"
            },
            RectTransform =
            {
                AnchorMin = "0.41 0.56",
                AnchorMax = "0.48 0.65"
            }
        },"BountyPay");
        container.Add(new CuiButton
        {
            Button =
            {
                Color = "0.27 0.29 0.31 0.95",
                Command = $"lower.bounty"
            },
            Text =
            {
                Text = "-",
                FontSize = 18,
                Color = "0.81 0.80 0.81 1.00"
            },
            RectTransform =
            {
                AnchorMin = "0.53 0.56",
                AnchorMax = "0.59 0.65"
            }
        }, "BountyPay");
        CuiHelper.AddUi(player, container);
    }
    #endregion
}