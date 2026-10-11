# 美术资源

## 目录

| 目录 | 内容 |
| --- | --- |
| `Sprites/Player.gif` | 玩家图像 |
| `Sprites/Terrain/Cave` | 洞穴地面、墙面与地形图集 |
| `Sprites/Props/Kitchen` | 铁锅、柴火、食材、水槽、工作台、晾肉架 |
| `Sprites/Props/Camp` | 帐篷、草床、木刺、训练假人 |
| `Sprites/Props/TotemsBones` | 旗帜、兽骨、仪式木架、木盾 |
| `Sprites/Props/Lighting` | 支架火把、火盆、壁挂火把 |
| `References/Kitchen` | 厨房道具参考图 |
| `Materials` / `Models` / `Textures` / `Audio` | 材质、模型、纹理与音频 |

## 命名

使用小写英文 `snake_case`，文件名描述物件内容。

- `_sheet`：多个物件或视角的图集；`_8view_sheet`：八视角图集。
- `_tileset`：地形组合图。
- `_crop`：裁剪素材，可能包含其他视图或边缘残片。
- `_reference`：参考图。
- `_01`、`_02`：同类物件的不同变体。

## 使用注意

- PNG Sprite 当前为 `Single`、`Point` 过滤、关闭 mipmaps、无压缩，PPU 为 `150`，pivot 为底部居中，mesh 为 `Full Rect`。
- 图集尚未切割。需要独立物件或视角时，使用 Sprite Editor 切割，并按实际内容确认裁切范围与方向顺序。
- 部分小图带有边缘残片；放入场景前检查显示范围，按物件需要调整 PPU 与 pivot。
- `References` 中的 JPG 保留白底，作为参考图使用。
