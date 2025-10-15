#!/usr/bin/env python3
"""
Unity Scene Hierarchy Printer
Parses Unity .unity scene files (YAML format) and prints the complete GameObject hierarchy
"""

import re
import sys
from pathlib import Path
from collections import defaultdict

def parse_unity_scene(scene_path):
    """Parse Unity scene file and extract GameObject hierarchy"""
    
    with open(scene_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Split into YAML documents (separated by ---)
    documents = content.split('--- !u!')
    
    gameobjects = {}
    transforms = {}
    
    # Parse GameObjects and Transforms
    for doc in documents:
        # Extract GameObject info
        if doc.startswith('1 &'):
            lines = doc.split('\n')
            obj_id = None
            obj_name = None
            obj_active = True
            
            for line in lines:
                # Get object ID
                if line.startswith('1 &'):
                    obj_id = line.split('&')[1].split()[0]
                # Get object name
                elif line.strip().startswith('m_Name:'):
                    obj_name = line.split('m_Name:')[1].strip()
                # Get active state
                elif line.strip().startswith('m_IsActive:'):
                    obj_active = '1' in line
            
            if obj_id and obj_name:
                gameobjects[obj_id] = {
                    'name': obj_name,
                    'active': obj_active,
                    'children': []
                }
        
        # Extract Transform info (hierarchy relationships)
        elif doc.startswith('4 &'):
            lines = doc.split('\n')
            transform_id = None
            gameobject_ref = None
            parent_ref = None
            children_refs = []
            
            for i, line in enumerate(lines):
                # Get transform ID
                if line.startswith('4 &'):
                    transform_id = line.split('&')[1].split()[0]
                # Get GameObject reference
                elif line.strip().startswith('m_GameObject:'):
                    match = re.search(r'fileID:\s*(\d+)', line)
                    if match:
                        gameobject_ref = match.group(1)
                # Get parent reference
                elif line.strip().startswith('m_Father:'):
                    match = re.search(r'fileID:\s*(\d+)', line)
                    if match:
                        parent_ref = match.group(1)
                # Get children references
                elif line.strip().startswith('m_Children:'):
                    # Read children list
                    j = i + 1
                    while j < len(lines) and lines[j].strip().startswith('-'):
                        match = re.search(r'fileID:\s*(\d+)', lines[j])
                        if match:
                            children_refs.append(match.group(1))
                        j += 1
            
            if transform_id and gameobject_ref:
                transforms[transform_id] = {
                    'gameobject': gameobject_ref,
                    'parent': parent_ref,
                    'children': children_refs
                }
    
    # Build hierarchy by connecting GameObjects via Transforms
    gameobject_to_transform = {}
    for transform_id, transform_data in transforms.items():
        gameobject_to_transform[transform_data['gameobject']] = transform_id
    
    # Find root objects (no parent or parent is 0)
    roots = []
    for obj_id, obj_data in gameobjects.items():
        transform_id = gameobject_to_transform.get(obj_id)
        if transform_id:
            parent_transform = transforms[transform_id].get('parent')
            if not parent_transform or parent_transform == '0':
                roots.append(obj_id)
    
    return gameobjects, transforms, gameobject_to_transform, roots

def print_hierarchy(gameobjects, transforms, gameobject_to_transform, obj_id, indent=0, prefix=""):
    """Recursively print GameObject hierarchy"""
    
    obj = gameobjects.get(obj_id)
    if not obj:
        return
    
    # Print current object
    active_marker = "✓" if obj['active'] else "✗"
    print(f"{prefix}{active_marker} {obj['name']}")
    
    # Get children through transform
    transform_id = gameobject_to_transform.get(obj_id)
    if transform_id:
        child_transforms = transforms[transform_id].get('children', [])
        
        for i, child_transform_id in enumerate(child_transforms):
            # Find GameObject for this transform (skip if transform not found)
            if child_transform_id not in transforms:
                continue
            child_obj_id = transforms[child_transform_id].get('gameobject')
            
            # Determine if this is the last child
            is_last = (i == len(child_transforms) - 1)
            
            # Create prefix for child
            if indent == 0:
                new_prefix = "├── " if not is_last else "└── "
                child_prefix = "│   " if not is_last else "    "
            else:
                new_prefix = prefix.replace("├── ", "│   ").replace("└── ", "    ")
                new_prefix += "├── " if not is_last else "└── "
                child_prefix = prefix.replace("├── ", "│   ").replace("└── ", "    ")
                child_prefix += "│   " if not is_last else "    "
            
            if child_obj_id:
                print_hierarchy(gameobjects, transforms, gameobject_to_transform, 
                              child_obj_id, indent + 1, new_prefix)

def main():
    if len(sys.argv) < 2:
        print("Usage: python print_scene_hierarchy.py <scene_file.unity>")
        print("\nAvailable scenes:")
        scenes_dir = Path(__file__).parent / "Assets" / "GAME" / "Scenes"
        if scenes_dir.exists():
            for scene in scenes_dir.glob("*.unity"):
                print(f"  - {scene.name}")
        sys.exit(1)
    
    scene_path = Path(sys.argv[1])
    
    if not scene_path.exists():
        print(f"Error: Scene file not found: {scene_path}")
        sys.exit(1)
    
    print(f"\n{'='*80}")
    print(f"Scene Hierarchy: {scene_path.name}")
    print(f"{'='*80}\n")
    
    gameobjects, transforms, gameobject_to_transform, roots = parse_unity_scene(scene_path)
    
    print(f"Total GameObjects: {len(gameobjects)}")
    print(f"Root Objects: {len(roots)}\n")
    print("Legend: ✓ = Active, ✗ = Inactive\n")
    
    # Print each root and its hierarchy
    for root_id in roots:
        print_hierarchy(gameobjects, transforms, gameobject_to_transform, root_id)
        print()  # Blank line between root objects

if __name__ == "__main__":
    main()
