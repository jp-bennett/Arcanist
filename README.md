# Arcanist
Source code of my mod ArcaneTide.

## Credits
Thanks to Elmindra for her code of [Eldritch Arcana](https://github.com/jennyem/pathfinder-mods),which shows a good example of how to add a new class.
Most of the codes of Helpers.cs of this project also comes from her.

Thanks to Spacehamster for his/her patience when I asked him/her some stupid questions.

Thanks to 白猫,Falengel,北爹,炽炎烈龙,狐狸 and all other contributors to the Chinese translation of Pathfinder rules.

## Differences from Tabletop PF
Because of the limitation PF:KM's game mechanics, some abilities are not implemented to be 100% same as what they are in tabletop PF.
### Arcane Exploits
**Dimension Slide:** This costs a swift action now.

## 致谢
感谢Elmindra大佬，Elmindra大佬的[Eldritch Arcana](https://github.com/jennyem/pathfinder-mods)是我学习拥王者mod制作的入门教材。
这个mod的Helpers.cs中的大部分代码来自她的mod。

感谢Spacehamster大佬，在我提一些傻逼问题的时候能够细心解答。

感谢白猫,Falengel,北爹,炽炎烈龙,狐狸（排名不分先后）还有其他参与制作PF规则中文翻译的大佬们。

## 和桌面PF的不同
因为拥王者游戏机制的限制，部分能力的实现上和桌面规则有一定差异。
### 职业能力
**转耗法术：** 被P子削弱过的转耗法术太菜了。现将转耗法术的每日使用次数提高到3+魅力调整值。
### 奥能技艺
**次元滑步：** 次元滑步现在消耗迅捷动作。
**高等超魔学识：** (new in 0.92)因为拥王者的游戏机能限制，几乎无法实现原版的高等超魔学识。现将高等超魔学识改为：

选择一个不具备的超魔专长，奥能师学会此专长，并可以在施法时以一个移动动作消耗等同于超魔专长费用的奥能点数，为法术附着此专长且不需要更高级别的法术位、也不需要延长施法时间。这样的行动每日最多三次。奥能师选择超魔专长时必须满足前置条件。


I've made some changes to the release branch and pushed it into master on my fork of this project. Since I'm building on Linux using Mono, there are a few other things I've fixed up.

To build, I'm running:
xbuild ArcaneTide.csproj /p:KingmakerPath=/home/jbennett/.steam/steam/steamapps/common /p:Configuration=Release

This automatically copies the release DLL to the ArcaneTide directory, which is the directory that gets copied into Kingmaker's mod directory. That directory can alternatively be zipped and used with the UnityModManager.
