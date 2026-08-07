using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class MapExportData
{
	public string map_version { get; set; }
	public List<ChunkData> chunks { get; set; }
	public Dictionary<string, List<List<float[]>>> global_polygons { get; set; }
}

public class ChunkData
{
	public string chunk_id { get; set; }
	public int grid_x { get; set; }
	public int grid_y { get; set; }
	public string background_path { get; set; }
	public Vector2Data global_position { get; set; }
	public List<BuildingData> buildings { get; set; }
}

public class BuildingData
{
	public string image_path { get; set; }
	public float local_x { get; set; }
	public float local_y { get; set; }
	public float global_x { get; set; }
	public float global_y { get; set; }
	public float width { get; set; }
	public float height { get; set; }
}

public class Vector2Data
{
	public float x { get; set; }
	public float y { get; set; }
}

public partial class MapChunkManager : Node2D
{
	[Export]
	public string MapDataPath = "res://MapExport/map_data.json";

	[Export]
	public int LoadRadius = 1;

	private Node2D _trackTarget;
	private float _chunkStride = 3584f;
	private string _baseDir;

	private MapExportData _mapData;
	private Dictionary<string, ChunkData> _chunkDataDict = new Dictionary<string, ChunkData>();
	private Dictionary<string, Node2D> _activeChunkNodes = new Dictionary<string, Node2D>();
	private Node2D _chunksRoot;

	private Vector2I _lastPlayerChunkPos = new Vector2I(-9999, -9999);

	public override void _Ready()
	{
		_chunksRoot = new Node2D { Name = "Chunks" };
		AddChild(_chunksRoot);
		InitMapData();
	}

	public void SetTrackTarget(Node2D target)
	{
		_trackTarget = target;
		GD.Print($"[MapChunkManager] 已绑定追踪目标: {target.Name}，开始动态加载地图区块...");
		_lastPlayerChunkPos = new Vector2I(-9999, -9999);
	}

	public override void _Process(double delta)
	{
		if (_trackTarget == null || _mapData == null) return;

		int currentGridX = Mathf.FloorToInt(_trackTarget.GlobalPosition.X / _chunkStride);
		int currentGridY = Mathf.FloorToInt(_trackTarget.GlobalPosition.Y / _chunkStride);
		Vector2I currentChunkPos = new Vector2I(currentGridX, currentGridY);

		if (currentChunkPos != _lastPlayerChunkPos)
		{
			UpdateChunks(currentChunkPos);
			_lastPlayerChunkPos = currentChunkPos;
		}
	}

	private void InitMapData()
	{
		string absolutePath = ProjectSettings.GlobalizePath(MapDataPath);
		if (!File.Exists(absolutePath))
		{
			GD.PrintErr("找不到地图数据文件: " + absolutePath);
			return;
		}

		_baseDir = absolutePath.Substring(0, absolutePath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
		string jsonString = File.ReadAllText(absolutePath);
		_mapData = JsonSerializer.Deserialize<MapExportData>(jsonString);

		foreach (var chunk in _mapData.chunks)
		{
			_chunkDataDict[chunk.chunk_id] = chunk;
		}

		GenerateGlobalPolygons(_mapData.global_polygons);
	}

	private void UpdateChunks(Vector2I centerChunk)
	{
		List<string> chunksToKeep = new List<string>();

		for (int x = -LoadRadius; x <= LoadRadius; x++)
		{
			for (int y = -LoadRadius; y <= LoadRadius; y++)
			{
				string targetChunkId = $"{centerChunk.X + x}_{centerChunk.Y + y}";
				chunksToKeep.Add(targetChunkId);

				if (_chunkDataDict.ContainsKey(targetChunkId) && !_activeChunkNodes.ContainsKey(targetChunkId))
				{
					LoadChunkVisuals(_chunkDataDict[targetChunkId]);
				}
			}
		}

		List<string> chunksToRemove = new List<string>();
		foreach (var activeChunkId in _activeChunkNodes.Keys)
		{
			if (!chunksToKeep.Contains(activeChunkId))
			{
				chunksToRemove.Add(activeChunkId);
			}
		}

		foreach (var chunkId in chunksToRemove)
		{
			UnloadChunkVisuals(chunkId);
		}
	}

	private void LoadChunkVisuals(ChunkData chunk)
	{
		Node2D chunkContainer = new Node2D { Name = $"ChunkContainer_{chunk.chunk_id}" };

		Sprite2D chunkBg = new Sprite2D();
		chunkBg.Texture = LoadTextureAbsolute(_baseDir + chunk.background_path);
		chunkBg.Position = new Vector2(chunk.global_position.x, chunk.global_position.y);
		chunkBg.Centered = false;

		// 地表沉底
		chunkBg.ZIndex = -10;
		chunkContainer.AddChild(chunkBg);

		if (chunk.buildings != null)
		{
			foreach (var b in chunk.buildings)
			{
				Sprite2D buildingSprite = new Sprite2D();
				buildingSprite.Texture = LoadTextureAbsolute(_baseDir + b.image_path);
				buildingSprite.Position = new Vector2(b.local_x, b.local_y);
				buildingSprite.Centered = false;

				// 独立设置为 0，不再继承底图的 -10
				buildingSprite.ZIndex = 0;
				// 【关键修改！】把建筑直接挂在 container 上，而不是 chunkBg 上
				chunkContainer.AddChild(buildingSprite);
			}
		}

		_chunksRoot.AddChild(chunkContainer);
		_activeChunkNodes[chunk.chunk_id] = chunkContainer;
	}

	private void UnloadChunkVisuals(string chunkId)
	{
		if (_activeChunkNodes.TryGetValue(chunkId, out Node2D container))
		{
			container.QueueFree();
			_activeChunkNodes.Remove(chunkId);
		}
	}

	private Texture2D LoadTextureAbsolute(string path)
	{
		Image img = Image.LoadFromFile(path);
		return ImageTexture.CreateFromImage(img);
	}

	private void GenerateGlobalPolygons(Dictionary<string, List<List<float[]>>> globalPolygons)
	{
		if (globalPolygons == null) return;

		// 1. 禁足区 (红) - 实体碰撞墙
		if (globalPolygons.TryGetValue("col", out var colPolys))
		{
			StaticBody2D staticBody = new StaticBody2D { Name = "ForbiddenZones" };
			staticBody.CollisionLayer = 1;
			AddChild(staticBody);
			foreach (var pts in colPolys) { staticBody.AddChild(CreateCollisionPoly(pts)); }
		}

		// 2. 遮挡区 (蓝) - 玩家变半透明 (不改层级，只改透明度)
		if (globalPolygons.TryGetValue("occ", out var occPolys))
		{
			Node2D occRoot = new Node2D { Name = "OcclusionZones" };
			AddChild(occRoot);
			foreach (var pts in occPolys)
			{
				Area2D area = new Area2D();
				area.CollisionMask = 0xFFFFFFFF; // 扫描所有层
				area.AddChild(CreateCollisionPoly(pts));

				area.BodyEntered += (Node2D body) => {
					if (body.IsInGroup("Player") || body.Name.ToString().IsValidInt())
					{
						// 变成半透明剪影
						body.Modulate = new Color(1, 1, 1, 0.4f);
					}
				};
				area.BodyExited += (Node2D body) => {
					if (body.IsInGroup("Player") || body.Name.ToString().IsValidInt())
					{
						// 恢复不透明
						body.Modulate = new Color(1, 1, 1, 1f);
					}
				};
				occRoot.AddChild(area);
			}
		}

		// 3. 前景区 (黄) - 强行把玩家压到树冠/前景下面
		if (globalPolygons.TryGetValue("fg", out var fgPolys))
		{
			Node2D fgRoot = new Node2D { Name = "ForegroundZones" };
			AddChild(fgRoot);
			foreach (var pts in fgPolys)
			{
				Area2D area = new Area2D();
				area.CollisionMask = 0xFFFFFFFF; // 扫描所有层
				area.AddChild(CreateCollisionPoly(pts));

				area.BodyEntered += (Node2D body) => {
					if (body.IsInGroup("Player") || body.Name.ToString().IsValidInt())
					{
						// 取消相对层级，强行沉底被前景盖住
						body.ZAsRelative = false;
						body.ZIndex = -1;
					}
				};
				area.BodyExited += (Node2D body) => {
					if (body.IsInGroup("Player") || body.Name.ToString().IsValidInt())
					{
						// 出来后恢复
						body.ZIndex = 1;
					}
				};
				fgRoot.AddChild(area);
			}
		}
	}
	private CollisionPolygon2D CreateCollisionPoly(List<float[]> points)
	{
		Vector2[] godotPts = new Vector2[points.Count];
		for (int i = 0; i < points.Count; i++)
		{
			godotPts[i] = new Vector2(points[i][0], points[i][1]);
		}
		return new CollisionPolygon2D { Polygon = godotPts };
	}
}
