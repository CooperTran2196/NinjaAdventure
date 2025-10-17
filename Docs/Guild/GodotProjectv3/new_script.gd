tool
extends EditorScript

func _run():
	var root = get_editor_interface().get_edited_scene_root()
	if not root:
		printerr("No scene open")
		return
	_dump(root, "")

func _dump(node, indent):
	print(indent + node.name + " : " + node.get_class())
	for child in node.get_children():
		_dump(child, indent + "    ")
