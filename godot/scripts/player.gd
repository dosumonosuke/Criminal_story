extends CharacterBody3D
## First-person player for the town prototype.
## Story-driven design: free walking exists only inside this small
## block, and there is exactly ONE objective at a time. When the
## player reaches the marked person and presses E, a dialogue scene
## plays, and when it ends the prototype's story beat is complete.

const SPEED: float = 4.2
const GRAVITY: float = 18.0

# Dialogue with the grandmother (Chapter 1-2 facts, docs/RESEARCH.md:
# taken in by the maternal grandparents before age four and adopted
# in March 1960; her older brother is Keith). Lines are gentle and
# contain no new factual claims beyond that.
const DIALOGUE: Array = [
    {"who": "祖母", "text": "おかえり、アイリーン。ここが、あなたの家よ。"},
    {"who": "", "text": "1960年、4歳になる直前。母は家を出て、兄キースとアイリーンは母方の祖父母に引き取られた。同年3月、2人は正式に養子となった。"},
    {"who": "祖母", "text": "キースも待っているわ。さあ、中に入りましょう。"},
    {"who": "", "text": "穏やかとは言えない日々が、この家で始まろうとしていた――。"},
]

var _head: Node3D
var _cam: Camera3D
var _yaw: float = 0.0
var _pitch: float = 0.0
var _target: Node3D
var _dialogue_idx: int = -1
var _in_dialogue: bool = false
var _story_done: bool = false

var _objective_label: Label
var _prompt_label: Label
var _dialogue_panel: PanelContainer
var _speaker_label: Label
var _line_label: Label
var _end_panel: PanelContainer

func _ready() -> void:
    _head = get_node("Head") as Node3D
    _cam = get_node("Head/Camera3D") as Camera3D
    _yaw = rotation.y
    var targets: Array[Node] = get_tree().get_nodes_in_group("interactable")
    if targets.size() > 0:
        _target = targets[0] as Node3D
    _build_ui()
    Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _build_ui() -> void:
    var layer := CanvasLayer.new()
    add_child(layer)

    var top := Label.new()
    top.add_theme_font_size_override("font_size", 24)
    top.position = Vector2(32, 22)
    top.text = "第1〜2章　ミシガン州の住宅街（1956–1960）"
    top.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.85))
    top.add_theme_constant_override("shadow_offset_x", 2)
    top.add_theme_constant_override("shadow_offset_y", 2)
    layer.add_child(top)

    _objective_label = Label.new()
    _objective_label.add_theme_font_size_override("font_size", 20)
    _objective_label.add_theme_color_override("font_color", Color(1.0, 0.87, 0.5))
    _objective_label.position = Vector2(32, 58)
    _objective_label.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.85))
    _objective_label.add_theme_constant_override("shadow_offset_x", 2)
    _objective_label.add_theme_constant_override("shadow_offset_y", 2)
    layer.add_child(_objective_label)

    var controls := Label.new()
    controls.add_theme_font_size_override("font_size", 15)
    controls.position = Vector2(32, 668)
    controls.text = "WASD：歩く　マウス：見回す　E：話しかける　Esc：マウスを離す"
    controls.modulate = Color(1, 1, 1, 0.7)
    layer.add_child(controls)

    _prompt_label = Label.new()
    _prompt_label.add_theme_font_size_override("font_size", 24)
    _prompt_label.add_theme_color_override("font_color", Color(1.0, 0.9, 0.55))
    _prompt_label.set_anchors_preset(Control.PRESET_CENTER_BOTTOM)
    _prompt_label.offset_left = -260
    _prompt_label.offset_right = 260
    _prompt_label.offset_top = -120
    _prompt_label.offset_bottom = -84
    _prompt_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    _prompt_label.text = ""
    layer.add_child(_prompt_label)

    _dialogue_panel = PanelContainer.new()
    _dialogue_panel.set_anchors_preset(Control.PRESET_BOTTOM_WIDE)
    _dialogue_panel.offset_left = 60
    _dialogue_panel.offset_right = -60
    _dialogue_panel.offset_top = -190
    _dialogue_panel.offset_bottom = -36
    var vbox := VBoxContainer.new()
    _dialogue_panel.add_child(vbox)
    _speaker_label = Label.new()
    _speaker_label.add_theme_font_size_override("font_size", 22)
    _speaker_label.add_theme_color_override("font_color", Color(1.0, 0.85, 0.45))
    vbox.add_child(_speaker_label)
    _line_label = Label.new()
    _line_label.add_theme_font_size_override("font_size", 25)
    _line_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    vbox.add_child(_line_label)
    var hint := Label.new()
    hint.add_theme_font_size_override("font_size", 15)
    hint.text = "クリック／スペース／E：次へ"
    hint.modulate = Color(1, 1, 1, 0.6)
    vbox.add_child(hint)
    _dialogue_panel.visible = false
    layer.add_child(_dialogue_panel)

    _end_panel = PanelContainer.new()
    _end_panel.set_anchors_preset(Control.PRESET_CENTER)
    _end_panel.offset_left = -340
    _end_panel.offset_right = 340
    _end_panel.offset_top = -150
    _end_panel.offset_bottom = 150
    var ev := VBoxContainer.new()
    _end_panel.add_child(ev)
    var et := Label.new()
    et.add_theme_font_size_override("font_size", 24)
    et.text = "試作版はここまでです"
    ev.add_child(et)
    var eb := Label.new()
    eb.add_theme_font_size_override("font_size", 19)
    eb.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    eb.text = "第0章（生まれる場面）と、第1〜2章の舞台であるミシガンの住宅街を歩けるようにした段階です。今後はこの町を広げ、章ごとの場面と会話を足していきます。"
    ev.add_child(eb)
    var btn_free := Button.new()
    btn_free.text = "このまま町を見回す"
    btn_free.pressed.connect(func() -> void:
        _end_panel.visible = false
        Input.mouse_mode = Input.MOUSE_MODE_CAPTURED)
    ev.add_child(btn_free)
    var btn_back := Button.new()
    btn_back.text = "オープニング（第0章）に戻る"
    btn_back.pressed.connect(func() -> void:
        get_tree().change_scene_to_file("res://scenes/opening.tscn"))
    ev.add_child(btn_back)
    _end_panel.visible = false
    layer.add_child(_end_panel)

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED and not _in_dialogue:
        var mm := event as InputEventMouseMotion
        _yaw -= mm.relative.x * 0.0022
        _pitch = clamp(_pitch - mm.relative.y * 0.0022, -1.2, 1.2)
    if event is InputEventMouseButton and event.pressed:
        if _in_dialogue:
            _advance_dialogue()
        elif Input.mouse_mode != Input.MOUSE_MODE_CAPTURED and not _end_panel.visible:
            Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
    if event.is_action_pressed("ui_accept") and _in_dialogue:
        _advance_dialogue()
    if event is InputEventKey and event.pressed and not event.echo:
        var k := event as InputEventKey
        if k.physical_keycode == KEY_E:
            if _in_dialogue:
                _advance_dialogue()
            elif _near_target():
                _start_dialogue()

func _near_target() -> bool:
    if _target == null or _story_done:
        return false
    return global_position.distance_to(_target.global_position) < 3.0

func _start_dialogue() -> void:
    _in_dialogue = true
    _dialogue_idx = -1
    _dialogue_panel.visible = true
    _prompt_label.text = ""
    Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
    _advance_dialogue()

func _advance_dialogue() -> void:
    _dialogue_idx += 1
    if _dialogue_idx >= DIALOGUE.size():
        _in_dialogue = false
        _story_done = true
        _dialogue_panel.visible = false
        _end_panel.visible = true
        return
    var line: Dictionary = DIALOGUE[_dialogue_idx]
    _speaker_label.text = String(line["who"])
    _line_label.text = String(line["text"])

func _physics_process(delta: float) -> void:
    rotation.y = _yaw
    if _head:
        _head.rotation.x = _pitch
    if not is_on_floor():
        velocity.y -= GRAVITY * delta
    else:
        velocity.y = -0.5
    var input_dir := Vector2.ZERO
    if not _in_dialogue and not _end_panel.visible:
        if Input.is_physical_key_pressed(KEY_W) or Input.is_physical_key_pressed(KEY_UP):
            input_dir.y -= 1.0
        if Input.is_physical_key_pressed(KEY_S) or Input.is_physical_key_pressed(KEY_DOWN):
            input_dir.y += 1.0
        if Input.is_physical_key_pressed(KEY_A):
            input_dir.x -= 1.0
        if Input.is_physical_key_pressed(KEY_D):
            input_dir.x += 1.0
        if Input.is_physical_key_pressed(KEY_LEFT):
            _yaw += delta * 1.8
        if Input.is_physical_key_pressed(KEY_RIGHT):
            _yaw -= delta * 1.8
    input_dir = input_dir.normalized()
    var dir: Vector3 = (transform.basis * Vector3(input_dir.x, 0, input_dir.y))
    dir.y = 0.0
    dir = dir.normalized() if dir.length() > 0.01 else Vector3.ZERO
    velocity.x = dir.x * SPEED
    velocity.z = dir.z * SPEED
    move_and_slide()

    if _target:
        if _story_done:
            _objective_label.text = "目的：達成しました（このまま町を見回せます）"
            _prompt_label.text = ""
        else:
            var d: float = global_position.distance_to(_target.global_position)
            _objective_label.text = "目的：金のマーカーの下にいる「祖母」に近づいて、E で話しかける（あと約 %d m）" % int(d)
            _prompt_label.text = "E：祖母に話しかける" if _near_target() else ""
