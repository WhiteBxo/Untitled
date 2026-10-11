extends SceneTree

# Export saved scene cells without entering the tree or running _ready().
func _initialize() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() != 1:
		push_error("Usage: godot --headless --path <Godot project> --script <this script> -- <output.json>")
		quit(1)
		return
	var scene: PackedScene = load("res://src/levels/cave/cave_map.tscn")
	var map := scene.instantiate()
	var floor_layer: TileMapLayer = map.get_node("FloorLayer")
	var river_layer: TileMapLayer = map.get_node("RiverLayer")
	if map.transform != Transform2D.IDENTITY or floor_layer.transform != Transform2D.IDENTITY or river_layer.transform != Transform2D.IDENTITY:
		push_error("Transformed map/layers require an explicit coordinate mapping.")
		map.free()
		quit(1)
		return
	var spawn: Vector2i = map.get("spawn_cell")
	var floors := cells(floor_layer)
	var rivers := cells(river_layer)
	if not floor_layer.get_used_cells().has(spawn):
		push_error("Spawn must be on a saved floor cell.")
		map.free()
		quit(1)
		return
	var data := {
		"formatVersion": 1,
		"sourcePixelsPerCell": floor_layer.tile_set.tile_size.x,
		"sourceOriginPixels": {"x": 0, "y": 0},
		"cellSize": 1.0,
		"spawnCell": {"x": spawn.x, "y": spawn.y},
		"floorCells": floors,
		"riverCells": rivers,
	}
	var file := FileAccess.open(args[0], FileAccess.WRITE)
	if file == null:
		push_error("Cannot write output.")
		map.free()
		quit(1)
		return
	file.store_string(JSON.stringify(data, "\t") + "\n")
	print("Exported saved scene: ", floors.size(), " floor cells, ", rivers.size(), " river cells; spawn=", spawn, "; cell=", data.sourcePixelsPerCell, "px")
	map.free()
	quit()

func cells(layer: TileMapLayer) -> Array:
	var result: Array = []
	var used := layer.get_used_cells()
	used.sort_custom(func(a: Vector2i, b: Vector2i): return a.y < b.y or (a.y == b.y and a.x < b.x))
	for cell: Vector2i in used:
		result.append({"x": cell.x, "y": cell.y})
	return result
