extends Node3D
## Chapter 0 — "The moment of birth" (opening cutscene).
## Design rule (story-driven): the player does almost nothing here.
## They can only move the view a little (mouse / arrow keys / drag).
## The story advances by itself through timed dialogue lines, then
## fades out and loads the town prototype (Chapters 1-2 stage).
##
## Facts used (docs/RESEARCH.md only): born 1956-02-29 in Rochester,
## Michigan, as Aileen Carol Pittman. The scene is kept gentle:
## a delivery room just after birth, a doctor, a nurse, the mother.
## No graphic or medical detail.

const LINES: Array = [
    {"who": "", "text": "（……おぎゃあ、おぎゃあ……）", "sec": 4.5},
    {"who": "医師", "text": "女の子です。元気な女の子ですよ。", "sec": 4.5},
    {"who": "看護師", "text": "1956年2月29日、ミシガン州ロチェスターで生まれました。", "sec": 5.0},
    {"who": "医師", "text": "名前は、アイリーン・キャロル・ピットマン。", "sec": 4.5},
    {"who": "母親", "text": "アイリーン……私の子……。", "sec": 4.5},
    {"who": "", "text": "こうして一人の女の子が生まれた。この子の人生を、これから一人称でたどっていく。", "sec": 6.0},
]

var _cam: Camera3D
var _yaw: float = 0.0
var _pitch: float = 0.0
var _line_idx: int = -1
var _line_timer: float = 0.0
var _age: float = 0.0
var _finished: bool = false

var _white: ColorRect
var _fade_black: ColorRect
var _speaker_label: Label
var _line_label: Label
var _chapter_label: Label
var _hint_label: Label

func _ready() -> void:
    _cam = get_node("BabyCamera") as Camera3D
    _build_ui()
    _next_line()

func _build_ui() -> void:
    var layer := CanvasLayer.new()
    add_child(layer)

    # Bright "hazy world" overlay: starts almost fully white and
    # slowly clears, standing in for a newborn's blurry first sight.
    _white = ColorRect.new()
    _white.color = Color(1, 0.99, 0.96, 1.0)
    _white.set_anchors_preset(Control.PRESET_FULL_RECT)
    _white.mouse_filter = Control.MOUSE_FILTER_IGNORE
    layer.add_child(_white)

    _fade_black = ColorRect.new()
    _fade_black.color = Color(0, 0, 0, 0)
    _fade_black.set_anchors_preset(Control.PRESET_FULL_RECT)
    _fade_black.mouse_filter = Control.MOUSE_FILTER_IGNORE
    layer.add_child(_fade_black)

    _chapter_label = _make_label(layer, 28, Vector2(40, 28))
    _chapter_label.text = "第0章　生まれる ― 1956年2月29日／ミシガン州ロチェスター"

    var panel := PanelContainer.new()
    panel.set_anchors_preset(Control.PRESET_BOTTOM_WIDE)
    panel.offset_left = 60
    panel.offset_right = -60
    panel.offset_top = -190
    panel.offset_bottom = -36
    layer.add_child(panel)
    var vbox := VBoxContainer.new()
    panel.add_child(vbox)
    _speaker_label = Label.new()
    _speaker_label.add_theme_font_size_override("font_size", 22)
    _speaker_label.add_theme_color_override("font_color", Color(1.0, 0.85, 0.45))
    vbox.add_child(_speaker_label)
    _line_label = Label.new()
    _line_label.add_theme_font_size_override("font_size", 26)
    _line_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    vbox.add_child(_line_label)

    _hint_label = _make_label(layer, 16, Vector2(40, 660))
    _hint_label.text = "マウス／矢印キーで少しだけ見回せます　｜　クリック or スペース：セリフを進める"
    _hint_label.modulate = Color(1, 1, 1, 0.65)

func _make_label(parent: Node, size: int, pos: Vector2) -> Label:
    var l := Label.new()
    l.add_theme_font_size_override("font_size", size)
    l.position = pos
    l.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.8))
    l.add_theme_constant_override("shadow_offset_x", 2)
    l.add_theme_constant_override("shadow_offset_y", 2)
    parent.add_child(l)
    return l

func _next_line() -> void:
    _line_idx += 1
    if _line_idx >= LINES.size():
        _finish()
        return
    var line: Dictionary = LINES[_line_idx]
    _speaker_label.text = String(line["who"])
    _line_label.text = String(line["text"])
    _line_timer = float(line["sec"])

func _finish() -> void:
    if _finished:
        return
    _finished = true
    _speaker_label.text = ""
    _line_label.text = "―― 第1章へ。舞台は、ミシガンの小さな町の住宅街。"
    var tw := create_tween()
    tw.tween_property(_fade_black, "color:a", 1.0, 2.0)
    tw.tween_callback(func() -> void:
        get_tree().change_scene_to_file("res://scenes/town.tscn"))

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
        var mm := event as InputEventMouseMotion
        _yaw = clamp(_yaw - mm.relative.x * 0.0012, -0.55, 0.55)
        _pitch = clamp(_pitch - mm.relative.y * 0.0012, -0.28, 0.28)
    if event.is_action_pressed("ui_accept") or (event is InputEventMouseButton and event.pressed):
        if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED and event is InputEventMouseButton:
            Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
            return
        if not _finished:
            _next_line()

func _process(delta: float) -> void:
    _age += delta
    # The hazy world clears over the first seconds.
    var clear: float = clamp(_age / 6.0, 0.0, 1.0)
    _white.color.a = lerpf(1.0, 0.0, clear)
    if _cam:
        _cam.fov = lerpf(82.0, 64.0, clear)
    # Very limited look-around only (this is a scene to watch).
    if Input.is_physical_key_pressed(KEY_LEFT):
        _yaw = clamp(_yaw + delta * 0.5, -0.55, 0.55)
    if Input.is_physical_key_pressed(KEY_RIGHT):
        _yaw = clamp(_yaw - delta * 0.5, -0.55, 0.55)
    if Input.is_physical_key_pressed(KEY_UP):
        _pitch = clamp(_pitch + delta * 0.35, -0.28, 0.28)
    if Input.is_physical_key_pressed(KEY_DOWN):
        _pitch = clamp(_pitch - delta * 0.35, -0.28, 0.28)
    if _cam:
        _cam.rotation = Vector3(_pitch, _yaw, sin(_age * 0.6) * 0.004)
    if _finished:
        return
    _line_timer -= delta
    if _line_timer <= 0.0:
        _next_line()
