# KarmaHSK

Colony karma system tracking moral decisions (quests, wild men, travelers). Mood effects via Need_Mercy.

## Architecture

### Core System
- `GameComponent_QuestPressure` — central state, `List<QuestRecord>`, score range -30..+30, auto-purge after 1 year
- `Need_Mercy` — colony-wide need mapped from score (0.0-1.0), excludes guests/bloodlust/inhumanized
- `ThoughtWorker_Mercy` — 10-stage mood (-20 heartless to +8 saintly)

### Event Point Values
| Event | Points |
|-------|--------|
| Quest completed | +1 |
| Quest expired | -2 |
| Quest failed | -3 |
| Charity quest completed | +4 |
| Charity quest expired | -6 |
| Wild man killed | -4 |
| Wild man wounded | -1 |
| Wild man arrived | +1 |
| Traveler allowed | +1 |
| Traveler refused | -1 |
| Shop purchase | -5 |

### Quest Balancing (Settings)
- `threatMultiplier` (0.6) — quest difficulty scaling
- `rewardMultiplier` (0.25) — quest reward scaling
- `maxRefugees` (4), `maxHelpers` (2) — pawn count caps
- Wastepack nerfs with era-based multipliers (Neolithic 0.25x, Medieval 0.35x)

### UI
- `Dialog_MercyInfo` — history dashboard, colored bar, scrollable log
- `Dialog_MercyShop` — spend karma for supplies (pemmican, medicine via drop pod)

## Source Files

```
Source/QuestPressure/
  GameComponent_QuestPressure.cs  # Core state + scoring
  Need_Mercy.cs                   # Need display
  ThoughtWorker_Mercy.cs          # 10-stage mood
  Dialog_MercyInfo.cs             # History UI
  Dialog_MercyShop.cs             # Karma shop
  QuestPressureMod.cs             # Mod class + settings UI
  QuestPressureSettings.cs        # Settings model
  Patch_QuestExpired.cs           # Quest tracking + threat/reward scaling
  Patch_RefugeeCount.cs           # Refugee/helper caps
  Patch_TravelerAllowed.cs        # Traveler tracking (AskBeforeEnter)
  Patch_WastepackCount.cs         # Pollution dump nerfs
  Patch_WildMan.cs                # Wild man interaction (3 patches)
```

## Defs

- `Defs/NeedDefs/Need_Mercy.xml` — mercy need def
- `Defs/ThoughtDefs/Thoughts_QuestPressure.xml` — 10-stage thought

## RimWorld 1.5 Quests (QuestNode_Root_*)

### Беженцы / присоединение
| QuestNode | Описание |
|-----------|----------|
| Beggars | Попрошайки просят ресурсы, взамен предлагают рабочих на время. `LodgerCountFromPopulation` определяет кол-во |
| Hospitality_Refugee | Беженцы просят убежище (мод Hospitality), после срока могут присоединиться |
| WandererJoin | Странник хочет присоединиться к колонии |
| WandererJoin_WalkIn | Странник приходит пешком и присоединяется |
| WandererJoinAbasia | Странник с абазией (не ходит) просит помощи |
| RefugeePodCrash | Капсула с беженцем падает рядом с колонией |
| RefugeePodCrash_Baby | Капсула с ребёнком |
| RefugeePodCrash_Ghoul | Капсула с гулем (Anomaly) |
| RefugeeDelayedReward | Беженец уходит и обещает награду позже |
| RefugeeBetrayal | Беженцы предают колонию |

### Рабочие лагеря / миссии
| QuestNode | Описание |
|-----------|----------|
| WorkSite | Рабочий лагерь на карте мира (лесоповал/ферма/шахта/охота). Отправляем колонистов, зачищаем, забираем лут |
| Mission_BanditCamp | Миссия — зачистить лагерь бандитов за награду |
| Mission_AncientComplex | Миссия в древний комплекс |
| Loot_AncientComplex | Разграбление древнего комплекса (без заказчика) |
| Loot_AncientComplex_Mechanitor | Древний комплекс для механитора |
| Hack_AncientComplex | Взлом терминалов в древнем комплексе |
| Hack_Spacedrone | Перехват и взлом космодрона |
| Hack_WorshippedTerminal | Взлом почитаемого терминала (Anomaly) |

### Церемонии / титулы (Royalty)
| QuestNode | Описание |
|-----------|----------|
| BestowingCeremony | Империя присылает делегацию для дарования титула |
| ShuttleCrash_Rescue | Шаттл терпит крушение, нужно защитить выживших |

### Загрязнение (Biotech)
| QuestNode | Описание |
|-----------|----------|
| PollutionDump | Фракция предлагает заплатить за приём отходов |
| PollutionRaid | Рейд из-за загрязнения |
| PollutionRetaliation | Ответный удар за загрязнение территории |

### Механиторы (Biotech)
| QuestNode | Описание |
|-----------|----------|
| MechanitorShip | Корабль с механоидами, можно захватить |
| MechanitorStartingMech | Стартовый мех для механитора |

### Сангвофаги (Biotech)
| QuestNode | Описание |
|-----------|----------|
| SanguophageShip | Корабль сангвофагов |
| SanguophageMeetingHost | Встреча с сангвофагами |

### Идеология (Ideology)
| QuestNode | Описание |
|-----------|----------|
| RelicHunt | Охота за реликвией идеологии |
| ReliquaryPilgrims | Паломники приходят к реликварию |

### Anomaly DLC
| QuestNode | Описание |
|-----------|----------|
| Creepjoiner_Arrival | Крипджойнер просится в колонию (скрытая угроза) |
| VoidMonolith | Монолит пустоты появляется на карте |
| VoidAwakening | Пробуждение пустоты — финальное событие |
| UnnaturalDarkness | Неестественная тьма накрывает карту |
| SightstealerArrival | Зрительный вор прибывает |
| Bossgroup | Босс-группа — вызов мощного врага за награду |
| DistressCall | Сигнал бедствия — отправляем колонистов на помощь |
| MysteriousCargo (+ UnnaturalCube, UnnaturalCorpse, RevenantSpine) | Таинственный груз — подозрительная посылка |
| MonolithMigration | Монолит перемещается |

### Прочие
| QuestNode | Описание |
|-----------|----------|
| DelayedRewardDropPods | Отложенная награда — ресурсы падают подами позже |
| AncientSignalActivation | Активация древнего сигнала |
| ArchonexusVictory (3 цикла) | Победа через архонексус — финальный квест |

## RimWorld 1.5 Events (IncidentWorker_*)

### Нейтральные / позитивные
| IncidentWorker | Описание |
|----------------|----------|
| TravelerGroup | Группа путников проходит мимо колонии |
| VisitorGroup | Гости посещают колонию |
| TraderCaravanArrival | Караван торговца прибывает для торговли |
| CaravanArrivalTributeCollector | Сборщик дани от Империи |
| OrbitalTraderArrival | Орбитальный торговец выходит на связь |
| WandererJoin | Одинокий странник присоединяется |
| WildManWandersIn | Дикарь забрёл на территорию |
| SelfTame | Дикое животное самоприручилось |
| FarmAnimalsWanderIn | Домашние животные забрели на карту |
| ThrumboPasses | Тромбо проходят через территорию |
| HerdMigration | Стадо мигрирует через карту |
| ResourcePodCrash | Капсула с ресурсами падает с орбиты |
| MeteoriteImpact | Метеорит падает (сталь/компоненты) |
| ShipChunkDrop | Обломки корабля падают с неба |
| AmbrosiaSprout | Росток амброзии вырастает на карте |
| InsectJelly | Желе насекомых найдено |
| Aurora | Северное сияние — бонус к настроению |
| WanderersSkylanterns | Небесные фонари — красивое событие |
| CaravanMeeting | Встреча караванов на мировой карте |
| GameEndedWanderersJoin | Странники присоединяются после проигрыша |

### Рейды / атаки
| IncidentWorker | Описание |
|----------------|----------|
| RaidEnemy | Рейд враждебной фракции |
| RaidFriendly | Дружественная фракция помогает в бою |
| Infestation | Инфестация насекомых из-под земли |
| DeepDrillInfestation | Инфестация вызванная глубинным буром |
| MechCluster | Кластер механоидов падает с орбиты |
| CrashedShipPart | Обломок корабля (психический/ядовитый) |
| Ambush_EnemyFaction | Засада враждебной фракции (на мировой карте) |
| Ambush_ManhunterPack | Засада людоловов |
| CaravanDemand | Вражеская фракция требует ресурсы у каравана |
| RansomDemand | Требование выкупа за пленника |
| PsychicRitualSiege | Психическая осада (Anomaly) |
| ShamblerAssault | Атака шамблеров (Anomaly) |

### Животные
| IncidentWorker | Описание |
|----------------|----------|
| AggressiveAnimals | Стая агрессивных животных нападает |
| AnimalInsanitySingle | Одно животное впадает в бешенство |
| AnimalInsanityMass | Массовое бешенство животных |
| Alphabeavers | Альфабобры — уничтожают деревья |

### Погода / условия
| IncidentWorker | Описание |
|----------------|----------|
| ColdSnap | Резкие заморозки |
| HeatWave | Аномальная жара |
| Flashstorm | Точечная молниевая буря |
| CropBlight | Болезнь поражает посевы |
| ShortCircuit | Короткое замыкание — взрыв батареи |
| PsychicDrone | Психический дрон — давит настроение |
| PsychicSoothe | Психическое успокоение — поднимает настроение |
| DeathPall | Пелена смерти (Anomaly) |

### Болезни
| IncidentWorker | Описание |
|----------------|----------|
| DiseaseHuman | Вспышка болезни среди людей (чума, грипп и т.д.) |
| DiseaseAnimal | Вспышка болезни среди животных |

### Anomaly DLC
| IncidentWorker | Описание |
|----------------|----------|
| Revenant | Ревенант появляется и охотится на колонистов |
| RevenantEmergence | Ревенант вылезает из трупа |
| FleshbeastAttack | Атака мясных тварей |
| FleshmassHeart | Сердце плотемассы вырастает на карте |
| GhoulAttack | Атака гулей |
| GorehulkAssault | Атака горехалка |
| ChimeraAssault | Атака химеры |
| DevourerAssault | Пожиратель нападает с суши |
| DevourerWaterAssault | Пожиратель нападает из воды |
| ShamblerSwarm / Small / Animals | Рой шамблеров разных размеров |
| SightstealerArrival | Зрительный вор прибывает |
| SightstealerSwarm | Рой зрительных воров |
| HateChanters | Певцы ненависти окружают колонию |
| MetalhorrorImplantation | Металхоррор имплантируется в колониста |
| VoidCuriosity | Колонист проявляет любопытство к пустоте |
| UnnaturalCorpseArrival | Неестественный труп появляется |
| PitGate | Врата бездны открываются |
| Nociosphere | Ноцисфера — психическая аномалия |
| WastepackInfestation | Инфестация от отходов |
| Obelisk (Mutator/Duplicator/Abductor) | Обелиски с разными эффектами |
| GoldenCubeArrival | Золотой куб падает с неба |

### Деревья
| IncidentWorker | Описание |
|----------------|----------|
| AnimaTreeSpawn | Дерево анимы вырастает (Royalty) |
| GauranlenPodSpawn | Капсула гауранлена появляется (Ideology) |
| PoluxTreeSpawn | Дерево полукс вырастает |
| HarbingerTreeSpawn | Дерево предвестника вырастает (Anomaly) |
| HarbingerTreeProvoked | Дерево предвестника спровоцировано |

## Build

```bash
# 1.5 only (default)
dotnet build Source/QuestPressure/QuestPressure.v15.csproj -c Release

# 1.6 (only when asked)
dotnet build Source/QuestPressure/QuestPressure.v16.csproj -c Release
```
