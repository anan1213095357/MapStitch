# MapStitch 🗺️

[![Godot Engine](https://img.shields.io/badge/Godot-4.x%20Mono-478cbf?logo=godotengine&logoColor=white)](https://godotengine.org/)
[![HTML5](https://img.shields.io/badge/Platform-Web%20Browser-e34f26?logo=html5&logoColor=white)]()
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

*Read this in other languages: [简体中文](README.zh-cn.md)*

A 4K seamless map splicing and asset editor explicitly designed for 2D games, especially pixel art and fantasy RPG worlds.

<img width="3008" height="1958" alt="MapStitch Preview 1" src="https://github.com/user-attachments/assets/15ff1ae3-aba1-405f-b40a-4ae2e6e38233" />
<img width="3021" height="1965" alt="MapStitch Preview 2" src="https://github.com/user-attachments/assets/888d3f1f-41ac-49da-addc-48f032484238" />

Say goodbye to tedious manual alignment in Photoshop! MapStitch is a pure web-based tool featuring intelligent background color recognition (not limited to pure black), polygon-based building hollowing/merging, and physical mask painting. It exports Godot 4.x ready absolute-coordinate JSONs and individual transparent PNG slices with a single click—bridging the final gap between AI image generation and game engines.

## 📖 Core Workflow

To maximize the potential of this tool, we recommend the following AI-assisted workflow:

1. **Base Map Generation**: Use AI to generate a top-down `4096 x 4096` 2D map and upload it to MapStitch as your starting chunk.
2. **Asset Extraction**: Feed the original image back to the AI (or use image editing software) to remove all ground textures/paths and replace the background with **any pure color**, leaving only the buildings and objects intact.
3. **Spatial Editing**: Upload the solid-background image. MapStitch will automatically key out the background and extract the building entities. You can then use polygon tools to hollow out or merge structures and paint precise occlusion/collision masks.
4. **Seamless Expansion**: Click the `➕` icon on the edges of the grid. The tool will generate perfectly aligned edge reference images. Feed these to the AI for outpainting, upload the new generated chunks, and repeat the process to build a massive, infinite map.

## ✨ Features

- 🧩 **4K Seamless Matrix Splicing**: Automatically crops overlapping edges of adjacent images to ensure 100% pixel-perfect alignment between chunks.
- 🧠 **Smart Background Separation (Any Solid Color)**: Dynamically analyzes and keys out any solid background color to precisely extract buildings while automatically healing edges and applying transparency.
- ✂️ **Polygon Topology Editing**: Draw or box-select areas to "hollow out" unwanted internal parts of a building, or "stitch" fragmented pieces into a single render layer.
- 🎨 **Multi-Layer Mask Painting**: Built-in visual brushes allow you to freely draw **Collision zones (Red)**, **Occlusion zones (Blue)**, and **Foreground layers (Yellow)**.
- 📦 **Native Godot Pipeline**: Export a ZIP package containing all transparent PNG slices and a `map_data.json` with global absolute coordinates.
- 💾 **Local Offline Caching**: Pure frontend architecture utilizes IndexedDB. Assets are saved locally in milliseconds, ensuring a smooth, privacy-safe, offline-capable experience.

## 🚀 Quick Start

This project is a single-file application requiring no Node.js dependencies or build tools.

1. Clone or download this repository.
2. Double-click `index.html` to open it in your browser.
3. Click **"Upload 4K Background" (上传 4K 背景)** to start building your map.
4. **Godot Integration**: Extract the exported ZIP package into your Godot project. Attach the `MapChunkManager.cs` script to your main scene and set the `Map Data Path` to the extracted `map_data.json`.

## 🕹️ Controls

- **Pan & Zoom**: `Right-click + Drag` to pan the camera; `Mouse Wheel` for infinite zoom.
- **Extract Chunk Edges**: Click the `➕` on empty grids to download edge reference images for AI outpainting.
- **Upload Buildings**: Select an existing chunk, click "Upload Building (上传纯色底建筑)" to auto-extract assets from a solid background image.
- **Cut & Merge**: Switch to the corresponding mode, draw or select over buildings, and close the shape (click the starting point for polygons) to apply instantly.
- **Undo**: Press `Ctrl + Z` to undo the last drawn mask or selection.

## 📂 Exported Data Structure (Godot JSON)

The exported ZIP includes a `map_data.json` formatted for easy deserialization in Godot:

```json
{
  "map_version": "FINAL_PERFECT",
  "chunks": [
    {
      "chunk_id": "0_0",
      "grid_x": 0,
      "grid_y": 0,
      "background_path": "images/chunk_0_0_bg.jpg",
      "global_position": { "x": 0, "y": 0 },
      "buildings": [
        {
          "image_path": "images/chunk_0_0_building_0.png",
          "local_x": 120,
          "local_y": 340,
          "global_x": 120,
          "global_y": 340,
          "width": 512,
          "height": 512
        }
      ]
    }
  ],
  "global_polygons": {
    "col": [[[x,y], ...]], // Collision mask
    "occ": [[[x,y], ...]], // Occlusion mask
    "fg":  [[[x,y], ...]]  // Foreground layer
  }
}
