# Remove old QuestPressure junctions
foreach ($old in @('D:\RimWorld_HSK\Mods\QuestPressure', 'D:\RimWorld_HSK_1.5\Mods\QuestPressure')) {
    if (Test-Path $old) { cmd /c rmdir "$old" }
}
# Create new HSKQuestPressure junctions
$targets = @('D:\RimWorld_HSK\Mods\HSKQuestPressure', 'D:\RimWorld_HSK_1.5\Mods\HSKQuestPressure')
$source = 'D:\Mods\HSKQuestPressure'
foreach ($t in $targets) {
    if (Test-Path $t) { cmd /c rmdir "$t" }
    cmd /c mklink /J "$t" "$source"
}
