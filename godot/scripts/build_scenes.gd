extends Node
## Scene builder: constructs scenes/opening.tscn (Chapter 0, hospital)
## and scenes/town.tscn (Michigan residential block, Chapters 1-2 stage)
## entirely from Godot built-in primitive meshes and StandardMaterial3D.
## No external assets. Run once via tools/builder.tscn:
##   godot --headless --path . res://tools/builder.tscn
## Re-running overwrites the generated .tscn files.

var _mats: Dictionary = {}

func _ready() -> void:
    _build_opening()
    _build_town()
    print("BUILD_OK: scenes/opening.tscn and scenes/town.tscn saved")
    get_tree().quit()

# ---------- material / primitive helpers ----------

func mat(color: Color, rough: float = 0.9, emission: Color = Color(0, 0, 0, 0), e_energy: float = 0.0) -> StandardMaterial3D:
    var key: String = str(color) + "|" + str(rough) + "|" + str(emission) + "|" + str(e_energy)
    if _mats.has(key):
        return _mats[key]
    var m := StandardMaterial3D.new()
    m.albedo_color = color
    m.roughness = rough
    if e_energy > 0.0:
        m.emission_enabled = true
        m.emission = emission
        m.emission_energy_multiplier = e_energy
    _mats[key] = m
    return m

func _finish(node: Node3D, parent: Node, pos: Vector3) -> Node3D:
    node.position = pos
    parent.add_child(node)
    return node

func box(parent: Node, nname: String, size: Vector3, pos: Vector3, material: Material, collide: bool = false, rot: Vector3 = Vector3.ZERO) -> Node3D:
    var mesh := BoxMesh.new()
    mesh.size = size
    mesh.material = material
    var mi := MeshInstance3D.new()
    mi.name = nname
    mi.mesh = mesh
    mi.rotation = rot
    if collide:
        var body := StaticBody3D.new()
        body.name = nname + "Body"
        var shape := CollisionShape3D.new()
        var bs := BoxShape3D.new()
        bs.size = size
        shape.shape = bs
        shape.rotation = rot
        body.add_child(shape)
        body.add_child(mi)
        return _finish(body, parent, pos)
    return _finish(mi, parent, pos)

func cyl(parent: Node, nname: String, r_top: float, r_bottom: float, h: float, pos: Vector3, material: Material, collide: bool = false, rot: Vector3 = Vector3.ZERO) -> Node3D:
    var mesh := CylinderMesh.new()
    mesh.top_radius = r_top
    mesh.bottom_radius = r_bottom
    mesh.height = h
    mesh.radial_segments = 14
    mesh.material = material
    var mi := MeshInstance3D.new()
    mi.name = nname
    mi.mesh = mesh
    mi.rotation = rot
    if collide:
        var body := StaticBody3D.new()
        body.name = nname + "Body"
        var shape := CollisionShape3D.new()
        var cs := CylinderShape3D.new()
        cs.radius = max(r_top, r_bottom)
        cs.height = h
        shape.shape = cs
        shape.rotation = rot
        body.add_child(shape)
        body.add_child(mi)
        return _finish(body, parent, pos)
    return _finish(mi, parent, pos)

func sphere(parent: Node, nname: String, r: float, pos: Vector3, material: Material, scale: Vector3 = Vector3.ONE) -> Node3D:
    var mesh := SphereMesh.new()
    mesh.radius = r
    mesh.height = r * 2.0
    mesh.radial_segments = 18
    mesh.rings = 12
    mesh.material = material
    var mi := MeshInstance3D.new()
    mi.name = nname
    mi.mesh = mesh
    mi.scale = scale
    return _finish(mi, parent, pos)

func label3d(parent: Node, nname: String, text: String, pos: Vector3, size: int = 64, billboard: bool = true, color: Color = Color.WHITE) -> Label3D:
    var l := Label3D.new()
    l.name = nname
    l.text = text
    l.font_size = size
    l.pixel_size = 0.0038
    l.modulate = color
    l.outline_size = 10
    l.outline_modulate = Color(0, 0, 0, 0.85)
    if billboard:
        l.billboard = BaseMaterial3D.BILLBOARD_ENABLED
    l.position = pos
    parent.add_child(l)
    return l

func person(parent: Node, nname: String, pos: Vector3, shirt: Color, pants: Color, skin: Color, hair: Color, label_text: String, coat: bool = false) -> Node3D:
    var root := Node3D.new()
    root.name = nname
    root.position = pos
    parent.add_child(root)
    var body := Node3D.new()
    body.name = "Body"
    root.add_child(body)
    var pants_m := mat(pants, 0.95)
    var shirt_m := mat(shirt, 0.95)
    var skin_m := mat(skin, 0.75)
    box(body, "Legs", Vector3(0.36, 0.78, 0.22), Vector3(0, 0.39, 0), pants_m)
    var torso_color: Color = Color(0.96, 0.96, 0.94) if coat else shirt
    box(body, "Torso", Vector3(0.52, 0.66, 0.28), Vector3(0, 1.1, 0), mat(torso_color, 0.95))
    if coat:
        box(body, "ShirtFront", Vector3(0.3, 0.5, 0.02), Vector3(0, 1.12, 0.15), shirt_m)
    box(body, "ArmL", Vector3(0.11, 0.6, 0.11), Vector3(-0.33, 1.08, 0), mat(torso_color, 0.95))
    box(body, "ArmR", Vector3(0.11, 0.6, 0.11), Vector3(0.33, 1.08, 0), mat(torso_color, 0.95))
    sphere(body, "Head", 0.165, Vector3(0, 1.62, 0), skin_m)
    sphere(body, "Hair", 0.17, Vector3(0, 1.68, -0.015), mat(hair, 1.0), Vector3(1, 0.72, 1))
    if label_text != "":
        label3d(root, "NameLabel", label_text, Vector3(0, 2.05, 0), 60)
    return root

func _own(node: Node, root: Node) -> void:
    for child in node.get_children():
        child.owner = root
        _own(child, root)

func _save_scene(root: Node, path: String) -> void:
    _own(root, root)
    var packed := PackedScene.new()
    var err: int = packed.pack(root)
    if err != OK:
        push_error("pack failed for " + path + ": " + str(err))
        return
    var serr: int = ResourceSaver.save(packed, path)
    if serr != OK:
        push_error("save failed for " + path + ": " + str(serr))

# ---------- Chapter 0: hospital room ----------

func _build_opening() -> void:
    var root := Node3D.new()
    root.name = "Opening"
    root.set_script(load("res://scripts/opening.gd"))

    var wall_m := mat(Color(0.82, 0.88, 0.84), 0.95)
    var floor_m := mat(Color(0.78, 0.8, 0.78), 0.6)
    box(root, "Floor", Vector3(11, 0.2, 9), Vector3(0, -0.1, -0.5), floor_m, true)
    box(root, "Ceiling", Vector3(11, 0.2, 9), Vector3(0, 3.3, -0.5), mat(Color(0.95, 0.95, 0.93)))
    box(root, "WallBack", Vector3(11, 3.4, 0.2), Vector3(0, 1.6, -5), wall_m, true)
    box(root, "WallFront", Vector3(11, 3.4, 0.2), Vector3(0, 1.6, 4), wall_m, true)
    box(root, "WallLeft", Vector3(0.2, 3.4, 9), Vector3(-5.5, 1.6, -0.5), wall_m, true)
    box(root, "WallRight", Vector3(0.2, 3.4, 9), Vector3(5.5, 1.6, -0.5), wall_m, true)
    # Window with daylight on the back wall + frame
    box(root, "WindowGlow", Vector3(2.4, 1.5, 0.06), Vector3(-2.6, 1.9, -4.88), mat(Color(0.9, 0.97, 1.0), 0.3, Color(0.75, 0.88, 1.0), 1.6))
    box(root, "WindowFrameV", Vector3(0.08, 1.5, 0.08), Vector3(-2.6, 1.9, -4.86), mat(Color(0.6, 0.62, 0.6)))
    box(root, "WindowFrameH", Vector3(2.4, 0.08, 0.08), Vector3(-2.6, 1.9, -4.86), mat(Color(0.6, 0.62, 0.6)))
    # Door on the right wall + room sign
    box(root, "Door", Vector3(0.08, 2.2, 1.0), Vector3(5.38, 1.1, 1.5), mat(Color(0.55, 0.42, 0.3), 0.8))
    var sign := label3d(root, "RoomSign", "病室", Vector3(5.3, 2.55, 1.5), 72, false)
    sign.rotation_degrees = Vector3(0, -90, 0)
    # Wall clock on back wall
    cyl(root, "Clock", 0.28, 0.28, 0.05, Vector3(1.8, 2.5, -4.88), mat(Color(0.97, 0.97, 0.95), 0.4), false, Vector3(90, 0, 0))

    # Bed (mother) in the middle-back of the room
    var bed := Node3D.new()
    bed.name = "Bed"
    bed.position = Vector3(0.2, 0, -2.4)
    root.add_child(bed)
    box(bed, "BedFrame", Vector3(2.2, 0.3, 1.05), Vector3(0, 0.35, 0), mat(Color(0.75, 0.77, 0.78), 0.5), true)
    box(bed, "Mattress", Vector3(2.1, 0.18, 0.98), Vector3(0, 0.58, 0), mat(Color(0.97, 0.97, 0.95), 0.9))
    box(bed, "Pillow", Vector3(0.45, 0.12, 0.7), Vector3(-0.78, 0.72, 0), mat(Color(1, 1, 1), 0.95))
    box(bed, "Blanket", Vector3(1.25, 0.1, 1.0), Vector3(0.35, 0.68, 0), mat(Color(0.62, 0.75, 0.88), 1.0))
    # Mother lying on the bed (kept simple and gentle)
    var mother := Node3D.new()
    mother.name = "Mother"
    mother.position = Vector3(-0.25, 0.78, 0)
    bed.add_child(mother)
    box(mother, "Body", Vector3(0.85, 0.14, 0.42), Vector3(0.25, 0, 0), mat(Color(0.92, 0.85, 0.8), 1.0))
    sphere(mother, "Head", 0.15, Vector3(-0.5, 0.03, 0), mat(Color(0.96, 0.8, 0.68), 0.75))
    sphere(mother, "Hair", 0.155, Vector3(-0.53, 0.06, 0), mat(Color(0.35, 0.24, 0.16), 1.0), Vector3(1, 0.7, 1))
    label3d(bed, "MotherLabel", "母親", Vector3(-0.2, 1.35, 0), 56)

    # Bassinet (baby bed) — the camera sits just above it
    var bass := Node3D.new()
    bass.name = "Bassinet"
    bass.position = Vector3(0.6, 0, 0.9)
    root.add_child(bass)
    box(bass, "CribBase", Vector3(0.95, 0.4, 0.55), Vector3(0, 0.45, 0), mat(Color(0.95, 0.93, 0.88), 0.7), true)
    box(bass, "CribRim", Vector3(1.0, 0.07, 0.6), Vector3(0, 0.68, 0), mat(Color(0.85, 0.8, 0.72), 0.7))
    box(bass, "CribBlanket", Vector3(0.8, 0.06, 0.42), Vector3(0, 0.66, 0), mat(Color(0.98, 0.88, 0.9), 1.0))

    # Doctor and nurse standing by the bed, facing the baby
    var doctor := person(root, "Doctor", Vector3(-1.5, 0, -1.7), Color(0.25, 0.55, 0.55), Color(0.4, 0.42, 0.45), Color(0.92, 0.76, 0.62), Color(0.2, 0.18, 0.16), "医師", true)
    doctor.rotation.y = 2.6
    var nurse := person(root, "Nurse", Vector3(2.4, 0, -1.8), Color(0.65, 0.78, 0.9), Color(0.5, 0.55, 0.62), Color(0.97, 0.82, 0.7), Color(0.45, 0.3, 0.18), "看護師")
    nurse.rotation.y = -2.7
    box(nurse.get_node("Body"), "NurseCap", Vector3(0.26, 0.07, 0.26), Vector3(0, 1.76, 0), mat(Color(0.97, 0.97, 0.97), 0.9))

    # Cabinet, side table, lamp, ceiling light
    box(root, "Cabinet", Vector3(1.4, 1.9, 0.5), Vector3(-4.6, 0.95, -4.4), mat(Color(0.7, 0.72, 0.7), 0.6), true)
    box(root, "SideTable", Vector3(0.6, 0.55, 0.45), Vector3(1.6, 0.28, -3.2), mat(Color(0.65, 0.55, 0.42), 0.7), true)
    cyl(root, "LampPole", 0.03, 0.03, 1.5, Vector3(-3.4, 0.75, -1.2), mat(Color(0.4, 0.4, 0.42), 0.4))
    sphere(root, "LampShade", 0.16, Vector3(-3.4, 1.6, -1.2), mat(Color(1.0, 0.9, 0.7), 0.6, Color(1.0, 0.85, 0.55), 1.2))
    var lamp_light := OmniLight3D.new()
    lamp_light.name = "LampLight"
    lamp_light.position = Vector3(-3.4, 1.7, -1.2)
    lamp_light.light_color = Color(1.0, 0.88, 0.7)
    lamp_light.light_energy = 0.7
    lamp_light.omni_range = 6.0
    root.add_child(lamp_light)
    var ceil_light := OmniLight3D.new()
    ceil_light.name = "CeilingLight"
    ceil_light.position = Vector3(0, 2.9, -0.5)
    ceil_light.light_color = Color(1.0, 0.98, 0.94)
    ceil_light.light_energy = 0.9
    ceil_light.omni_range = 11.0
    root.add_child(ceil_light)

    var env := Environment.new()
    env.background_mode = Environment.BG_COLOR
    env.background_color = Color(0.85, 0.88, 0.86)
    env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
    env.ambient_light_color = Color(0.9, 0.92, 0.9)
    env.ambient_light_energy = 0.55
    var we := WorldEnvironment.new()
    we.name = "WorldEnvironment"
    we.environment = env
    root.add_child(we)

    var cam := Camera3D.new()
    cam.name = "BabyCamera"
    cam.position = Vector3(0.6, 1.02, 0.9)
    cam.fov = 82.0
    root.add_child(cam)

    _save_scene(root, "res://scenes/opening.tscn")

# ---------- Town: Michigan residential block (Chapters 1-2) ----------

func _make_house(parent: Node, pos: Vector3, rot_y: float, wall_color: Color, roof_color: Color) -> Node3D:
    var g := Node3D.new()
    g.name = "House"
    g.position = pos
    g.rotation.y = rot_y
    parent.add_child(g)
    box(g, "Base", Vector3(7, 3.2, 6), Vector3(0, 1.6, 0), mat(wall_color, 0.95), true)
    var roof_m := mat(roof_color, 1.0)
    box(g, "RoofL", Vector3(4.5, 0.16, 6.7), Vector3(-1.72, 4.05, 0), roof_m, false, Vector3(0, 0, 0.62))
    box(g, "RoofR", Vector3(4.5, 0.16, 6.7), Vector3(1.72, 4.05, 0), roof_m, false, Vector3(0, 0, -0.62))
    box(g, "Chimney", Vector3(0.6, 1.2, 0.6), Vector3(1.8, 4.6, 1.2), mat(Color(0.55, 0.35, 0.3), 1.0))
    box(g, "Door", Vector3(0.08, 2.0, 0.95), Vector3(3.53, 1.0, 0.6), mat(Color(0.4, 0.27, 0.18), 0.8))
    var win_m := mat(Color(0.95, 0.85, 0.6), 0.35, Color(1.0, 0.8, 0.45), 0.7)
    box(g, "Win1", Vector3(0.06, 1.0, 1.2), Vector3(3.53, 1.8, -1.6), win_m)
    box(g, "Win2", Vector3(0.06, 1.0, 1.2), Vector3(3.53, 1.8, 2.2), win_m)
    box(g, "WinBack", Vector3(1.4, 1.0, 0.06), Vector3(-1.2, 1.8, -3.03), win_m)
    box(g, "Porch", Vector3(1.4, 0.18, 2.2), Vector3(4.1, 0.09, 0.6), mat(Color(0.6, 0.58, 0.55), 0.9), true)
    box(g, "Mailbox", Vector3(0.5, 0.35, 0.35), Vector3(5.6, 0.75, 2.6), mat(Color(0.3, 0.35, 0.5), 0.6))
    cyl(g, "MailboxPost", 0.04, 0.04, 0.75, Vector3(5.6, 0.37, 2.6), mat(Color(0.4, 0.32, 0.24), 0.9))
    return g

func _make_tree(parent: Node, pos: Vector3, s: float = 1.0) -> void:
    cyl(parent, "Trunk", 0.14 * s, 0.2 * s, 1.6 * s, pos + Vector3(0, 0.8 * s, 0), mat(Color(0.4, 0.3, 0.2), 1.0), true)
    var leaf := mat(Color(0.25 + randf() * 0.08, 0.5 + randf() * 0.1, 0.25), 1.0)
    sphere(parent, "Leaves1", 1.25 * s, pos + Vector3(0, 2.5 * s, 0), leaf)
    sphere(parent, "Leaves2", 0.9 * s, pos + Vector3(0.6 * s, 3.2 * s, 0.2), leaf)

func _make_car(parent: Node, pos: Vector3, rot_y: float, color: Color) -> void:
    var g := Node3D.new()
    g.name = "Car"
    g.position = pos
    g.rotation.y = rot_y
    parent.add_child(g)
    box(g, "CarBody", Vector3(1.8, 0.55, 4.3), Vector3(0, 0.58, 0), mat(color, 0.35), true)
    box(g, "CarCabin", Vector3(1.6, 0.5, 2.1), Vector3(0, 1.08, -0.25), mat(Color(0.16, 0.2, 0.26), 0.15))
    var wheel_m := mat(Color(0.08, 0.08, 0.09), 0.9)
    for wx in [-0.85, 0.85]:
        for wz in [-1.4, 1.4]:
            cyl(g, "Wheel", 0.34, 0.34, 0.22, Vector3(wx, 0.34, wz), wheel_m, false, Vector3(0, 0, 90))
    var light_m := mat(Color(1.0, 0.95, 0.75), 0.3, Color(1.0, 0.95, 0.7), 0.8)
    box(g, "HeadL", Vector3(0.3, 0.14, 0.05), Vector3(-0.55, 0.62, -2.16), light_m)
    box(g, "HeadR", Vector3(0.3, 0.14, 0.05), Vector3(0.55, 0.62, -2.16), light_m)

func _make_streetlight(parent: Node, pos: Vector3, with_light: bool) -> void:
    var g := Node3D.new()
    g.name = "Streetlight"
    g.position = pos
    parent.add_child(g)
    cyl(g, "Pole", 0.06, 0.08, 4.6, Vector3(0, 2.3, 0), mat(Color(0.3, 0.32, 0.35), 0.5), true)
    sphere(g, "Lamp", 0.14, Vector3(0, 4.65, 0), mat(Color(1.0, 0.92, 0.7), 0.4, Color(1.0, 0.88, 0.6), 1.0))
    if with_light:
        var l := OmniLight3D.new()
        l.position = Vector3(0, 4.5, 0)
        l.light_color = Color(1.0, 0.9, 0.7)
        l.light_energy = 0.5
        l.omni_range = 10.0
        g.add_child(l)

func _build_town() -> void:
    var root := Node3D.new()
    root.name = "Town"

    # Sky, sun with shadows, light haze
    var sky_mat := ProceduralSkyMaterial.new()
    sky_mat.sky_top_color = Color(0.28, 0.52, 0.92)
    sky_mat.sky_horizon_color = Color(0.74, 0.84, 0.95)
    sky_mat.ground_bottom_color = Color(0.35, 0.42, 0.3)
    sky_mat.ground_horizon_color = Color(0.74, 0.84, 0.95)
    sky_mat.sun_angle_max = 25.0
    var sky := Sky.new()
    sky.sky_material = sky_mat
    var env := Environment.new()
    env.background_mode = Environment.BG_SKY
    env.sky = sky
    env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
    env.ambient_light_energy = 0.45
    env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
    env.fog_enabled = true
    env.fog_light_color = Color(0.78, 0.86, 0.95)
    env.fog_density = 0.0035
    env.fog_sky_affect = 0.4
    var we := WorldEnvironment.new()
    we.name = "WorldEnvironment"
    we.environment = env
    root.add_child(we)
    var sun := DirectionalLight3D.new()
    sun.name = "Sun"
    sun.rotation_degrees = Vector3(-48, -35, 0)
    sun.light_color = Color(1.0, 0.96, 0.88)
    sun.light_energy = 1.0
    sun.shadow_enabled = true
    sun.directional_shadow_max_distance = 120.0
    root.add_child(sun)

    # Ground and roads
    box(root, "Grass", Vector3(140, 0.2, 140), Vector3(0, -0.1, 0), mat(Color(0.27, 0.46, 0.22), 1.0), true)
    var asphalt := mat(Color(0.16, 0.16, 0.18), 0.98)
    box(root, "RoadMain", Vector3(8, 0.06, 140), Vector3(0, 0.03, 0), asphalt)
    box(root, "RoadCross", Vector3(140, 0.06, 8), Vector3(0, 0.045, 0), asphalt)
    var walk_m := mat(Color(0.68, 0.68, 0.66), 0.95)
    box(root, "WalkL", Vector3(2, 0.1, 140), Vector3(-5.2, 0.05, 0), walk_m)
    box(root, "WalkR", Vector3(2, 0.1, 140), Vector3(5.2, 0.05, 0), walk_m)
    box(root, "WalkCrossN", Vector3(140, 0.1, 2), Vector3(0, 0.055, -5.2), walk_m)
    box(root, "WalkCrossS", Vector3(140, 0.1, 2), Vector3(0, 0.055, 5.2), walk_m)
    var dash_m := mat(Color(0.9, 0.85, 0.55), 0.8)
    for z in range(-66, 67, 12):
        if abs(z) > 7:
            box(root, "Dash", Vector3(0.25, 0.02, 3), Vector3(0, 0.075, z), dash_m)
    for x in range(-66, 67, 12):
        if abs(x) > 7:
            box(root, "DashX", Vector3(3, 0.02, 0.25), Vector3(x, 0.085, 0), dash_m)
    # Crosswalk stripes at the intersection
    var cw_m := mat(Color(0.88, 0.88, 0.86), 0.85)
    for i in range(-2, 3):
        box(root, "Crosswalk", Vector3(0.7, 0.02, 3.4), Vector3(i * 1.5, 0.08, -6.8), cw_m)
        box(root, "Crosswalk2", Vector3(3.4, 0.02, 0.7), Vector3(-6.8, 0.08, i * 1.5), cw_m)

    # Houses: two rows along the main road + two across the cross street
    var palette: Array = [
        [Color(0.92, 0.85, 0.72), Color(0.5, 0.3, 0.22)],
        [Color(0.78, 0.84, 0.9), Color(0.3, 0.33, 0.42)],
        [Color(0.9, 0.76, 0.7), Color(0.45, 0.28, 0.25)],
        [Color(0.82, 0.88, 0.76), Color(0.35, 0.42, 0.3)],
        [Color(0.94, 0.9, 0.82), Color(0.55, 0.4, 0.28)],
        [Color(0.85, 0.8, 0.88), Color(0.4, 0.35, 0.5)],
    ]
    var idx: int = 0
    for z in [-36.0, -20.0, 20.0, 36.0]:
        var p: Array = palette[idx % palette.size()]
        _make_house(root, Vector3(-13.5, 0, z), 0.0, p[0], p[1])
        idx += 1
        var p2: Array = palette[idx % palette.size()]
        _make_house(root, Vector3(13.5, 0, z), PI, p2[0], p2[1])
        idx += 1
    _make_house(root, Vector3(-26, 0, -13.5), -PI / 2.0, palette[1][0], palette[1][1])
    _make_house(root, Vector3(26, 0, 13.5), PI / 2.0, palette[3][0], palette[3][1])

    # Small corner store with a sign and awning
    var store := Node3D.new()
    store.name = "Store"
    store.position = Vector3(15, 0, -13)
    store.rotation.y = PI
    root.add_child(store)
    box(store, "StoreBase", Vector3(11, 4.4, 8), Vector3(0, 2.2, 0), mat(Color(0.88, 0.82, 0.7), 0.9), true)
    box(store, "StoreRoof", Vector3(11.6, 0.3, 8.6), Vector3(0, 4.55, 0), mat(Color(0.4, 0.36, 0.32), 1.0))
    box(store, "StoreGlass", Vector3(8.5, 1.9, 0.08), Vector3(0, 1.25, 4.02), mat(Color(0.55, 0.7, 0.8), 0.15))
    box(store, "Awning", Vector3(9, 0.1, 1.6), Vector3(0, 2.5, 4.6), mat(Color(0.6, 0.25, 0.22), 0.9), false, Vector3(0.35, 0, 0))
    var store_sign := label3d(store, "StoreSign", "食料品店", Vector3(0, 3.6, 4.1), 110, false, Color(0.85, 0.3, 0.25))
    store_sign.pixel_size = 0.006

    # Trees along the sidewalks and in yards
    for z in range(-44, 49, 11):
        if abs(z) > 8:
            _make_tree(root, Vector3(-6.9, 0, z + 2), 0.9 + float(z % 3) * 0.08)
            _make_tree(root, Vector3(6.9, 0, z - 3), 1.0)
    _make_tree(root, Vector3(-19, 0, 14), 1.25)
    _make_tree(root, Vector3(19.5, 0, 26), 1.15)

    # Parked cars
    _make_car(root, Vector3(3.3, 0, -26), 0.0, Color(0.55, 0.2, 0.18))
    _make_car(root, Vector3(-3.3, 0, -16), PI, Color(0.2, 0.32, 0.5))
    _make_car(root, Vector3(3.3, 0, 9), 0.0, Color(0.75, 0.72, 0.65))
    _make_car(root, Vector3(-3.3, 0, 44), PI, Color(0.25, 0.4, 0.3))
    _make_car(root, Vector3(24, 0, 3.2), PI / 2.0, Color(0.6, 0.5, 0.2))

    # Streetlights and a traffic light at the intersection
    var light_i: int = 0
    for z in [-30.0, -7.0, 18.0, 42.0]:
        _make_streetlight(root, Vector3(5.9, 0, z), light_i % 2 == 0)
        _make_streetlight(root, Vector3(-5.9, 0, z + 5.0), false)
        light_i += 1
    var tl := Node3D.new()
    tl.name = "TrafficLight"
    tl.position = Vector3(5.9, 0, 5.9)
    root.add_child(tl)
    cyl(tl, "TLPole", 0.05, 0.06, 3.6, Vector3(0, 1.8, 0), mat(Color(0.25, 0.26, 0.28), 0.5), true)
    box(tl, "TLHead", Vector3(0.36, 0.95, 0.36), Vector3(0, 3.9, 0), mat(Color(0.12, 0.12, 0.13), 0.6))
    sphere(tl, "TLRed", 0.09, Vector3(-0.19, 4.22, 0), mat(Color(0.9, 0.2, 0.15), 0.4, Color(1, 0.25, 0.2), 1.4))
    sphere(tl, "TLYellow", 0.09, Vector3(-0.19, 3.9, 0), mat(Color(0.95, 0.75, 0.2), 0.4, Color(1, 0.8, 0.25), 0.5))
    sphere(tl, "TLGreen", 0.09, Vector3(-0.19, 3.58, 0), mat(Color(0.2, 0.85, 0.35), 0.4, Color(0.3, 1, 0.4), 0.4))

    # White picket fence in front of the grandparents' house (z = 20, left)
    var fence_m := mat(Color(0.94, 0.94, 0.92), 0.9)
    for i in range(6):
        box(root, "Fence", Vector3(0.09, 0.85, 0.28), Vector3(-8.6, 0.43, 16.6 + i * 1.35), fence_m)
    box(root, "FenceRail", Vector3(0.05, 0.09, 7.2), Vector3(-8.6, 0.62, 20.0), fence_m)

    # --- People ---
    # Grandmother: the single story objective. Stands by the target house.
    var grandma := person(root, "Grandmother", Vector3(-9.3, 0, 20.0), Color(0.5, 0.42, 0.6), Color(0.35, 0.3, 0.38), Color(0.94, 0.78, 0.64), Color(0.85, 0.83, 0.8), "祖母")
    grandma.rotation.y = PI / 2.0
    grandma.add_to_group("interactable", true)
    # Objective marker floating above her
    var marker := MeshInstance3D.new()
    marker.name = "ObjectiveMarker"
    var mm := SphereMesh.new()
    mm.radius = 0.22
    mm.height = 0.44
    mm.material = mat(Color(1.0, 0.8, 0.3), 0.3, Color(1.0, 0.75, 0.2), 2.2)
    marker.mesh = mm
    marker.position = Vector3(-9.3, 2.9, 20.0)
    marker.set_script(load("res://scripts/marker.gd"))
    root.add_child(marker)
    # Grandfather and brother Keith near the same house
    var grandpa := person(root, "Grandfather", Vector3(-10.6, 0, 17.2), Color(0.4, 0.38, 0.35), Color(0.3, 0.28, 0.26), Color(0.93, 0.76, 0.62), Color(0.75, 0.73, 0.7), "祖父")
    grandpa.rotation.y = PI / 2.0
    var keith := person(root, "Keith", Vector3(-9.5, 0, 22.6), Color(0.7, 0.3, 0.25), Color(0.3, 0.35, 0.5), Color(0.95, 0.79, 0.65), Color(0.3, 0.22, 0.15), "兄キース")
    keith.scale = Vector3(0.82, 0.82, 0.82)
    keith.rotation.y = PI / 2.0

    # Walking townspeople on the sidewalks
    var npc_script: Script = load("res://scripts/npc.gd")
    var walkers: Array = [
        [Vector3(-5.2, 0, -38), Vector3(-5.2, 0, 38), Color(0.3, 0.45, 0.6), 1.1],
        [Vector3(5.2, 0, 32), Vector3(5.2, 0, -34), Color(0.6, 0.5, 0.3), 0.95],
        [Vector3(-32, 0, 5.2), Vector3(30, 0, 5.2), Color(0.45, 0.55, 0.4), 1.25],
    ]
    var wi: int = 0
    for w in walkers:
        var npc := person(root, "Walker" + str(wi), w[0], w[2], Color(0.35, 0.33, 0.36), Color(0.94, 0.78, 0.64), Color(0.25, 0.2, 0.15), "近所の人")
        npc.set_script(npc_script)
        npc.set("point_a", w[0])
        npc.set("point_b", w[1])
        npc.set("walk_speed", w[3])
        npc.set("start_offset", float(wi) * 1.7)
        wi += 1

    # Player (first person)
    var player := CharacterBody3D.new()
    player.name = "Player"
    player.position = Vector3(3.5, 0.6, 38)
    player.set_script(load("res://scripts/player.gd"))
    root.add_child(player)
    var pcol := CollisionShape3D.new()
    pcol.name = "Collision"
    var cap := CapsuleShape3D.new()
    cap.radius = 0.35
    cap.height = 1.7
    pcol.shape = cap
    pcol.position = Vector3(0, 0.85, 0)
    player.add_child(pcol)
    var head := Node3D.new()
    head.name = "Head"
    head.position = Vector3(0, 1.58, 0)
    player.add_child(head)
    var cam := Camera3D.new()
    cam.name = "Camera3D"
    cam.fov = 70.0
    head.add_child(cam)

    _save_scene(root, "res://scenes/town.tscn")
