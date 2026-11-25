# LogicsBountySystem v1.0.0

## Overview

LogicsBountySystem is a Rust server plugin that introduces a player-driven bounty system. Players can place scrap bounties on other online players, allowing the community to put rewards on targets and earn scrap by eliminating them.

Multiple players can contribute to the same bounty, increasing its total reward.

## Features

- **Player Selection:** Browse online players through an in-game UI with a scrollable player list.
- **Bounty Placement:** Players can select a target and place a scrap bounty.
- **Self-Bounty Protection:** Players cannot place bounties on themselves.
- **Online Player Validation:** Bounties cannot be placed on players who are offline.
- **Minimum Bounty:** Bounties cannot be lower than 10 scrap.
- **Maximum Bounty:** Bounties cannot exceed 1,000 scrap per contribution.
- **Adjustable Amount:** Players can increase or decrease their selected bounty amount.
- **Stacking Bounties:** Multiple players can place bounties on the same target, increasing the total reward.
- **Automatic Rewards:** When a player with an active bounty is killed by another player, the killer receives the entire accumulated bounty in scrap.
- **Automatic Bounty Removal:** Once claimed, the bounty is removed so it cannot be claimed again.
- **Server Announcements:** The player placing the bounty, the target, and the rest of the server can be notified when a bounty is placed.
- **In-Game Interface:** A custom UI for browsing targets and managing bounty amounts.

## How It Works

1. A player opens the bounty menu.
2. They select an online player.
3. They choose their desired bounty amount.
4. The required scrap is deducted from their inventory.
5. The bounty is added to the target's existing total, if one exists.
6. When the target is killed by another player, the killer receives the full accumulated bounty.
7. The claimed bounty is removed from the system.

## Bounty Rules

| Rule | Limit |
|---|---|
| Minimum contribution | 10 Scrap |
| Maximum contribution | 1,000 Scrap |
| Self-targeting | Not allowed |
| Offline targets | Not allowed |
| Multiple contributions | Supported |
| Reward | Full accumulated bounty |

## Version Information

**Version:** 1.0.0  
**Status:** Initial Release  
**Developer:** LogicOutpost

## What's Next?

Future updates may introduce additional features, improvements to the interface, username searching, and expanded bounty management options.
