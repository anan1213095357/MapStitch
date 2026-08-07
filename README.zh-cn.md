# MapStitch 🗺️

[![Godot Engine](https://img.shields.io/badge/Godot-4.x%20Mono-478cbf?logo=godotengine&logoColor=white)](https://godotengine.org/)
[![HTML5](https://img.shields.io/badge/Platform-Web%20Browser-e34f26?logo=html5&logoColor=white)]()
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

一个专为 2D 游戏（特别是像素风/仙侠世界）设计的 4K 大地图无缝拼接与资产编辑器。

<img width="3008" height="1958" alt="MapStitch Preview 1" src="https://github.com/user-attachments/assets/15ff1ae3-aba1-405f-b40a-4ae2e6e38233" />
<img width="3021" height="1965" alt="MapStitch Preview 2" src="https://github.com/user-attachments/assets/888d3f1f-41ac-49da-addc-48f032484238" />

彻底告别繁琐的 PS 切图与手动对齐！MapStitch 运行在纯 Web 端，支持智能底色识别（不限纯黑）、多边形建筑掏空/缝合、物理掩码绘制，并可一键导出适配 Godot 4.x 的绝对坐标 JSON 与独立透明 PNG 切片，为您打通从 AI 绘图到游戏引擎的最后一步。

## 📖 核心工作流 (Workflow)

为了最大化发挥本工具的效能，推荐使用以下标准 AI 辅助工作流：

1. **底图生成**：利用 AI 生成一张 `4096 x 4096` 分辨率的 2D 俯视像素大图，并上传至 MapStitch 作为初始区块。
2. **建筑分离**：将原图重新发给 AI（或使用图像编辑软件手动抠图），移除所有地板材质，将背景替换为**任意纯色**，保留完整的建筑/物品主体。
3. **空间编辑**：将处理好的纯色底图片重新上传。MapStitch 会自动剔除底色并提取建筑实体。你可以利用多边形工具掏空、缝合建筑，并绘制精准的遮挡与碰撞掩码。
4. **无缝扩建**：点击地图侧边的 `➕` 号，工具将提取完美对齐的边缘参考图。将参考图发给 AI 进行局部重绘，再将新图上传，如此往复即可拼接出无限广阔的超级大地图。

## ✨ 核心特性

- 🧩 **4K 无缝矩阵拼接**：自动裁切相邻图像的重叠边缘，保证 4K 大地图区块间 100% 像素级对齐，无限延伸大世界画布。
- 🧠 **智能背景分离（任意纯色）**：无需强制要求纯黑底！内置动态底色识别算法，精准提取建筑实体，自动完成边缘形态愈合与透空处理。
- ✂️ **多边形拓扑编辑**：提供拉线与框选工具。闭合后可一键「掏空」建筑内部多余部分，或将多个细碎部件「缝合」为单一渲染层。
- 🎨 **多层掩码绘制**：内置可视化画笔，自由绘制 **禁足区(红)**、**遮挡区(蓝)** 与 **前景层(黄)**。
- 📦 **Godot 原生对接**：一键导出 ZIP 数据包，内含所有独立透明 PNG 切片及全局绝对坐标 `map_data.json`。
- 💾 **本地离线缓存**：基于纯前端架构，数据毫秒级自动保存至浏览器 IndexedDB，断网可用，极致流畅。

## 🚀 快速开始

本项目为纯前端单文件（Single-file）应用，无需安装任何依赖或构建工具。

1. 克隆或下载本仓库至本地。
2. 在浏览器中直接双击打开 `index.html`。
3. 点击 **「上传 4K 背景」** 开始你的地图构建。
4. **Godot 引擎集成**：将 ZIP 导出包解压至项目目录，在主场景中挂载 `MapChunkManager.cs` 脚本，并将 `Map Data Path` 指向 `map_data.json` 即可自动生成地图与碰撞。

## 🕹️ 基本操作指南

- **漫游与缩放**：按住 `鼠标右键` 拖动画面，`鼠标滚轮` 无极缩放。
- **区块提取**：点击空白网格上的 `➕` 号，下载用于 AI 重绘的边缘参考图。
- **建筑上传**：选中已有区块后，点击「上传纯色底建筑」，系统将自动分离建筑本体。
- **切割与合并**：切换至对应模式，拉线或框选建筑，闭合图形（多边形需点回起点）即刻生效。
- **撤销操作**：使用 `Ctrl + Z` 撤销刚刚画错的掩码或选区。

## 📂 导出数据结构 (Godot JSON)

导出的 ZIP 包中包含 `map_data.json`，在 Godot 中可直接反序列化解析：

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
    "col": [[[x,y], ...]], // 禁足碰撞区 / Collision
    "occ": [[[x,y], ...]], // 遮挡区 / Occlusion
    "fg":  [[[x,y], ...]]  // 前景层 / Foreground
  }
}
