extends Node3D
## Floating objective marker: gentle bob and spin so the single
## story objective is always easy to find (story-driven design:
## the player is never left wondering where to go).

var _t: float = 0.0
var _base_y: float = 0.0

func _ready() -> void:
    _base_y = position.y

func _process(delta: float) -> void:
    _t += delta
    position.y = _base_y + sin(_t * 2.0) * 0.18
    rotate_y(delta * 1.4)
