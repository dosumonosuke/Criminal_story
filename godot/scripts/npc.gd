extends Node3D
## Simple sidewalk walker: ping-pongs between point_a and point_b,
## faces the walking direction and bobs gently. Used for background
## townspeople in the Michigan residential prototype.

@export var point_a: Vector3 = Vector3.ZERO
@export var point_b: Vector3 = Vector3.ZERO
@export var walk_speed: float = 1.1
@export var start_offset: float = 0.0

var _target: Vector3
var _t: float = 0.0
var _body: Node3D

func _ready() -> void:
    global_position = point_a
    _target = point_b
    _body = get_node_or_null("Body") as Node3D

func _process(delta: float) -> void:
    _t += delta
    var to_target: Vector3 = _target - global_position
    to_target.y = 0.0
    var dist: float = to_target.length()
    if dist < 0.25:
        _target = point_a if _target == point_b else point_b
        return
    var dir: Vector3 = to_target / dist
    global_position += dir * walk_speed * delta
    var target_yaw: float = atan2(-dir.x, -dir.z) + PI
    rotation.y = lerp_angle(rotation.y, target_yaw, delta * 6.0)
    if _body:
        _body.position.y = abs(sin(_t * 6.0 + start_offset)) * 0.05
