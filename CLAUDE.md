# KarmaHSK

Colony karma system tracking moral decisions (quests, wild men, travelers, refugees). Mood effects via Need_Mercy.

## Architecture

### Core System
- `GameComponent_QuestPressure` — central state, `List<QuestRecord>`, score range -40..+40 (`ScoreMax` constant), auto-purge after 1 year
- `QuestRecord` has `customWeight` field for variable-cost events
- `Need_Mercy` — colony-wide need mapped from score (0.0-1.0), excludes guests/bloodlust/inhumanized
- `ThoughtWorker_Mercy` — 10-stage mood (-20 heartless to +8 saintly)

### Event Point Values
| Event | Points | Type |
|-------|--------|------|
| Quest completed | +2 | Completed |
| Quest expired | -3 | Expired |
| Quest failed | -4 | Failed |
| Charity quest completed | +3 | CharityCompleted |
| Charity quest expired | -4 | CharityExpired |
| Charity quest failed | -5 | CharityFailed |
| Refugee accepted (HSK) | +3 | CharityCompleted |
| Refugee rejected (HSK) | -4 | CharityExpired |
| Wild man killed | -4 | MajorPenalty |
| Wild man wounded | -1 | MinorPenalty |
| Wild man arrived | +1 | MinorBonus |
| Traveler allowed | +1 | TinyBonus |
| Traveler refused | -1 | TinyPenalty |
| Colonist killed by own | -5 | ColonistKilled |
| Shop purchase | customWeight | ShopPurchase |

### Quest Balancing (Settings)
- `threatMultiplier` (0.6) — quest difficulty scaling
- `rewardMultiplier` (0.25) — quest reward scaling
- `maxRefugees` (4), `maxHelpers` (2) — pawn count caps
- Wastepack nerfs with era-based multipliers (Neolithic 0.25x, Medieval 0.35x)

### Quest Blocking (`Patch_NoDoubleGuests`)
- `QuestScriptDef.CanRun` — blocks XML quests: Hospitality_Joiners, Hospitality_Prisoners
- `QuestNode_Root_Beggars.TestRunInt` — blocks beggars
- `QuestNode_Root_Hospitality_Refugee.TestRunInt` — blocks refugees
- Shows message when blocked

### Bugged Quest Detection
- Skips karma penalty for quests with 0 parts or name starting with "ERR:"
- Shows message to player

### RefugeeChased (HSK event)
- Patched via `AccessTools.TypeByName("SK.IncidentWorker_RefugeeChased")`
- Wraps dialog accept/reject actions
- Raid points scaled by `threatMultiplier`

### Traveler Refusal (AskBeforeEnter)
- Patches `AskBeforeEnter.Main.AskDialog` via AccessTools
- Wraps "Send away" dialog option with -1 karma
- Optional — skipped if ABE not present

### UI
- `Dialog_MercyInfo` — history dashboard, colored bar, scrollable log, debug button (dev mode)
- `Dialog_MercyShop` — spend karma for supplies (pemmican 2pts, medicine 5pts via drop pod)
- `Dialog_KarmaDebug` — opens right of mercy window, shows lodgers, score, quest settings, threat points, active quests with bugged detection

## Source Files

```
Source/QuestPressure/
  GameComponent_QuestPressure.cs  # Core state + scoring + ScoreMax constant
  Need_Mercy.cs                   # Need display
  ThoughtWorker_Mercy.cs          # 10-stage mood
  Dialog_MercyInfo.cs             # History UI + debug button
  Dialog_MercyShop.cs             # Karma shop with customWeight
  Dialog_KarmaDebug.cs            # Debug window (threat points, quests, lodgers)
  QuestPressureMod.cs             # Mod class + settings UI
  QuestPressureSettings.cs        # Settings model
  Patch_QuestExpired.cs           # Quest tracking + threat/reward scaling + bugged quest detection
  Patch_RefugeeCount.cs           # Refugee/helper caps
  Patch_TravelerAllowed.cs        # Traveler tracking + ABE refusal patch
  Patch_WastepackCount.cs         # Pollution dump nerfs
  Patch_WildMan.cs                # Wild man interaction (3 patches)
  Patch_RefugeeChased.cs          # HSK refugee event (AccessTools)
  Patch_ColonistKilled.cs         # Colonist killed by own (-10)
  Patch_NoDoubleGuests.cs         # Block guest quests when lodgers present
```

## Build

```bash
# 1.5 only (default)
dotnet build Source/QuestPressure/QuestPressure.v15.csproj -c Release
```
