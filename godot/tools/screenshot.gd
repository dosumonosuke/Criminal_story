extends Node
## Test helper (not part of the game): loads a scene given as the
## first user argument, waits, saves a viewport screenshot to the
## path given as the second user argument, then quits.
##   godot --path . res://tools/screenshot.tscn -- res://scenes/town.tscn /tmp/shot.png [wait_seconds]

func _ready() -> void:
    var args: PackedStringArray = OS.get_cmdline_user_args()
    var scene_path: String = args[0] if args.size() > 0 else "res://scenes/town.tscn"
    var out_path: String = args[1] if args.size() > 1 else "/tmp/shot.png"
    var wait_sec: float = float(args[2]) if args.size() > 2 else 3.0
    var packed: PackedScene = load(scene_path)
    var inst: Node = packed.instantiate()
    add_child(inst)
    await get_tree().create_timer(wait_sec).timeout
    await RenderingServer.frame_post_draw
    var img: Image = get_viewport().get_texture().get_image()
    img.save_png(out_path)
    print("SHOT_SAVED: ", out_path, " size=", img.get_size())
    get_tree().quit()
