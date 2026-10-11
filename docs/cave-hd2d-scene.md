# CaveHD2D 场景

打开 `Assets/Scenes/CaveHD2D.unity`，点击 Play，用原有移动输入探索。

场景保留 `Cave` 当前保存的 6309 个地板格、232 个河道格、出生点和房间连接。根据平面图布置入口、大厅、宴会区、居住区、酒馆、工作区、种植区、仓储、厨房、训练区和长老房间，使用项目已有 Props Prefab；未修改这些 Prefab 的源资产。

使用 [PrefabUtility.InstantiatePrefab](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.InstantiatePrefab.html) 保留 Prefab 关联；格子编辑、派生显示和镜头构图依据见 [HD-2D 场景开发经验](hd2d-scene-development.md)。

- `CaveMap/LayoutGrid`：正式布局，使用现有 Tile Palette 编辑。
- `CaveMap/Geometry`：按 16 格区块保存的派生地面、连续岩体、河床、水面和碰撞。
- `CaveMap/Architecture`：可手工移动的台座、台阶、立柱和地面分区。
- `LevelRoot/PlacedProps`：按区域分组的关联 Prefab 实例，可单独增删和摆放。
- `CaveTerrainStyle`：地面材质、岩层高度、起伏和外侧厚度；材质和 Volume 独立于原 Cave 场景。

像素物件实例使用 `Art/Materials/CaveHD2D/Prop_*` 材质和 `CavePixelProp` Shader，保留原贴图与八方向 frame，颜色、Depth、Normals 和 Shadow pass 使用相同的透明裁切。源 Prefab 与其材质保持原样。地形使用世界坐标采样选定贴图块，大台座和岩壁不随物体缩放放大像素。

地图保存时同步派生地形。`Untitled → Cave → Build HD2D Scene` 在已搭建场景中只重建地形，保留手工摆放；不会重新导入布局或重新摆放物件。`Apply HD2D Scene Lighting` 会重设该场景的补光、环境光及地形 Shader，手工调光后不必重复执行。

台座最高 1.08 单位，每级台阶 0.18 单位；需要玩家开启现有重力与 CharacterController。河面低于地面 0.10 单位，河床低于地面 0.42 单位，沿用原布局中河道可通行的规则。岩壁保持完整，遮挡玩家时由现有 CaveWallReveal 局部透视，碰撞不随显示消失。

人工试玩重点：从入口走过居住区和厨房检查通道；靠近前景岩壁检查透视与恢复；进入大厅检查台阶上下、遮挡和亮度。当前是完整地图的场景初稿，各区摆放和光照仍可根据试玩调整；未新增 NPC、生产或经营逻辑。

已完成 Unity 编译、Editor 生成/保存/重载、Tilemap 格子一致性检查和一次 Play Mode 初始化检查。新的像素 Shader 已在 Editor 中确认物件颜色与火焰显示；全图通行、台阶及动态遮挡需人工试玩。该场景依赖现有 Props 资源；与地图一同推送这些资源及其 `.meta`。
