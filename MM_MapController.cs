using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MM_MapController : MonoBehaviour {

	private enum TileCornerType	{
		NONE = 0b_0000_0000,
		TL = 0b_0000_0001,
		TR = 0b_0000_0010,
		BL = 0b_0000_0100,
		BR = 0b_0000_1000
	}

	public Tile tile_sky;
	public Tile tile_ocean;
	public Tile tile_save;
	public Tile tile_saveHealing;
	public Tile tile_boss;
	public Tile tile_pub;
	public Tile tile_chestRed;
	public Tile tile_shopItems;
	public Tile tile_closed;
	public Tile tile_h1;
	public Tile tile_h2;
	public Tile tile_h3;
	public Tile tile_v1;
	public Tile tile_v2;
	public Tile tile_v3;
	public Tile tile_tl;
	public Tile tile_tr;
	public Tile tile_bl;
	public Tile tile_br;
	public Tile tile_t;
	public Tile tile_b;
	public Tile tile_l;
	public Tile tile_r;
	public Tile tile_pl;
	public Tile tile_pr;
	public Tile tile_plr;
	public Tile tile_pt;
	public Tile tile_pb;
	public Tile tile_ptb;
	public Tile tile_ptl;
	public Tile tile_ptr;
	public Tile tile_pbl;
	public Tile tile_pbr;
	public Tile tile_ptlr;
	public Tile tile_ptbl;
	public Tile tile_ptbr;
	public Tile tile_pblr;
	public Tile tile_ptblr;
	public Tile tile_ctl;
	public Tile tile_ctr;
	public Tile tile_cbl;
	public Tile tile_cbr;
	public Tile tile_ctltr;
	public Tile tile_ctlbl;
	public Tile tile_ctlbr;
	public Tile tile_ctrbl;
	public Tile tile_ctrbr;
	public Tile tile_cblbr;
	public Tile tile_ctltrbl;
	public Tile tile_ctltrbr;
	public Tile tile_ctlblbr;
	public Tile tile_ctrblbr;
	public Tile tile_ctltrblbr;

	private Tilemap _mapBaseLayer;
	private Tilemap _mapIconsLayer;
	private Tilemap _mapPathsLayer;
	private Tilemap _mapWallsLayer;
	private Tilemap _mapBordersLayer;
	private Tilemap _mapCornersLayer;
	private RectTransform _playerIcon;

	private bool _mapRepopulated;
	private List<string> _mapNewCoordinates = new List<string>();
	private List<string> _mapRenderedCoordinates = new List<string>();
	private Dictionary<string, TileCornerType> _mapRenderedCorners = new Dictionary<string, TileCornerType>();
	private Vector2 _MAP_OFFSET = new Vector2(63f, 57.5f);

	public void SetReferences(Transform mapScreen) {
		Transform grid = mapScreen.transform.Find("Grid");
		_mapBaseLayer = grid.GetChild(0).GetComponent<Tilemap>();
		_mapIconsLayer = grid.GetChild(1).GetComponent<Tilemap>();
		_mapPathsLayer = grid.GetChild(2).GetComponent<Tilemap>();
		_mapWallsLayer = grid.GetChild(3).GetComponent<Tilemap>();
		_mapBordersLayer = grid.GetChild(4).GetComponent<Tilemap>();
		_mapCornersLayer = grid.GetChild(5).GetComponent<Tilemap>();
		_playerIcon = grid.GetChild(6).GetComponent<RectTransform>();
	}

	public void ClearExploredMap() {
		_clearExploredMap();
	}

	public void PrepareAllExploredCoordinatesForRendering() {
		_repopulateMap();
	}

	public void AddExploredMapCoordinate(string position) {
		if (!_mapRenderedCoordinates.Contains(position)) {
			_mapRenderedCoordinates.Add(position);
			_mapNewCoordinates.Add(position);
		}
	}

	public void PopulateNewMapCoordinates() {
		if (_mapNewCoordinates.Count == _mapRenderedCoordinates.Count) {
			// If the count of new coordinates matches the rendered count then this is the first rendering, so clear all tilemaps from any previous session
			_mapBaseLayer.ClearAllTiles();
			_mapIconsLayer.ClearAllTiles();
			_mapPathsLayer.ClearAllTiles();
			_mapWallsLayer.ClearAllTiles();
			_mapBordersLayer.ClearAllTiles();
			_mapCornersLayer.ClearAllTiles();
		}
		// Populate newly explored tiles on the map
		for (int i = 0; i < _mapNewCoordinates.Count; i++) {
			_paintMap(_mapNewCoordinates[i]);
		}
		_mapNewCoordinates.Clear();
	}

	public void SetPlayerIconPosition(string position, MM_Objects_MapUpdateCollider mapUpdateCollider) {
		// Determine the integer portion of the position
		string[] coordinates = position.Split(',');
		float x = float.Parse(coordinates[0]);
		float y = float.Parse(coordinates[1]);
		// Adjust the position based on the location within the map update collider
		if (mapUpdateCollider != null) {
			Vector2 extents = mapUpdateCollider.bounds.bounds.extents;
			float left = mapUpdateCollider.transform.position.x - extents.x;
			float bottom = mapUpdateCollider.transform.position.y - extents.y;
			float percentageX = (MM.Player.transform.position.x - left) / (extents.x * 2);
			float percentageY = (MM.Player.transform.position.y - bottom) / (extents.y * 2);
			if (mapUpdateCollider.limitLeftPosition && percentageX < 0.5f || mapUpdateCollider.limitRightPosition && percentageX > 0.5f) {
				percentageX = 0.5f;
			}
			if (mapUpdateCollider.limitBottomPosition && percentageY < 0.5f || mapUpdateCollider.limitTopPosition && percentageY > 0.5f) {
				percentageY = 0.5f;
			}
			x = x - 0.5f + percentageX;
			y = y - 0.5f + percentageY;
		}
		_playerIcon.anchoredPosition = new Vector3((x - _MAP_OFFSET.x) * 2f, (y - _MAP_OFFSET.y) * 2f, _playerIcon.localPosition.z);
	}

	private void _clearExploredMap() {
		// Clear the explored map
		_mapRenderedCoordinates.Clear();
		_mapNewCoordinates.Clear();
		_mapRenderedCorners.Clear();
		_mapRepopulated = false;
	}

	private void _repopulateMap() {
		if (!_mapRepopulated) {
			// Repopulate the map based on the player explored data
			List<String> exploredPositions = MM.player.ExploredMapCoordinates;
			for (int i = 0; i < exploredPositions.Count; i++) {
				if (!_mapRenderedCoordinates.Contains(exploredPositions[i])) {
					_mapRenderedCoordinates.Add(exploredPositions[i]);
					_mapNewCoordinates.Add(exploredPositions[i]);
				}
			}
			_mapRepopulated = true;
		}
	}

	private void _paintMap(string position) {
		string[] coordinates = position.Split(',');
		int x = int.Parse(coordinates[0]);
		int y = int.Parse(coordinates[1]);
		bool borderTop = false;
		bool borderBottom = false;
		bool borderLeft = false;
		bool borderRight = false;

		// Determine the tile to paint at the specified position
		Tile baseTile = null;
		Tile iconsTile = null;
		Tile pathsTile = null;
		Tile wallsTile = null;
		TileType icon = TileType.NOTHING;
		if (MM_Constants_Map.BaseLayerTiles.ContainsKey(position)) {
			switch (MM_Constants_Map.BaseLayerTiles[position]) {
				case TileType.SKY:
					baseTile = tile_sky;
					break;
				case TileType.OCEAN:
					baseTile = tile_ocean;
					break;
			}
		}
		if (MM.player.ExploredMapIconCoordinates.ContainsKey(position)) {
			icon = MM.player.ExploredMapIconCoordinates[position];
		} else if (MM_Constants_Map.IconsLayerTiles.ContainsKey(position)) {
			icon = MM_Constants_Map.IconsLayerTiles[position];
		}
		if (icon != TileType.NOTHING) {
			switch (icon) {
				case TileType.SAVE:
					iconsTile = tile_save;
					break;
				case TileType.SAVE_HEALING:
					iconsTile = tile_saveHealing;
					break;
				case TileType.BOSS:
					iconsTile = tile_boss;
					break;
				case TileType.PUB:
					iconsTile = tile_pub;
					break;
				case TileType.RED_CHEST:
					iconsTile = tile_chestRed;
					break;
				case TileType.SHOP_ITEMS:
					iconsTile = tile_shopItems;
					break;
			}
		}
		if (MM_Constants_Map.PathsLayerTiles.ContainsKey(position)) {
			switch (MM_Constants_Map.PathsLayerTiles[position]) {
				case TileType.PL:
					pathsTile = tile_pl;
					break;
				case TileType.PR:
					pathsTile = tile_pr;
					break;
				case TileType.PLR:
					pathsTile = tile_plr;
					break;
				case TileType.PT:
					pathsTile = tile_pt;
					break;
				case TileType.PB:
					pathsTile = tile_pb;
					break;
				case TileType.PTB:
					pathsTile = tile_ptb;
					break;
				case TileType.PTL:
					pathsTile = tile_ptl;
					break;
				case TileType.PTR:
					pathsTile = tile_ptr;
					break;
				case TileType.PBL:
					pathsTile = tile_pbl;
					break;
				case TileType.PBR:
					pathsTile = tile_pbr;
					break;
				case TileType.PTLR:
					pathsTile = tile_ptlr;
					break;
				case TileType.PTBL:
					pathsTile = tile_ptbl;
					break;
				case TileType.PTBR:
					pathsTile = tile_ptbr;
					break;
				case TileType.PBLR:
					pathsTile = tile_pblr;
					break;
				case TileType.PTBLR:
					pathsTile = tile_ptblr;
					break;
			}
		}
		if (MM_Constants_Map.WallsLayerTiles.ContainsKey(position)) {
			switch (MM_Constants_Map.WallsLayerTiles[position]) {
				case TileType.CLOSED:
					wallsTile = tile_closed;
					borderTop = true;
					borderBottom = true;
					borderLeft = true;
					borderRight = true;
					break;
				case TileType.H1:
					wallsTile = tile_h1;
					borderTop = true;
					borderLeft = true;
					borderBottom = true;
					break;
				case TileType.H2:
					wallsTile = tile_h2;
					borderTop = true;
					borderBottom = true;
					break;
				case TileType.H3:
					wallsTile = tile_h3;
					borderTop = true;
					borderBottom = true;
					borderRight = true;
					break;
				case TileType.V1:
					wallsTile = tile_v1;
					borderTop = true;
					borderLeft = true;
					borderRight = true;
					break;
				case TileType.V2:
					wallsTile = tile_v2;
					borderLeft = true;
					borderRight = true;
					break;
				case TileType.V3:
					wallsTile = tile_v3;
					borderBottom = true;
					borderLeft = true;
					borderRight = true;
					break;
				case TileType.TL:
					wallsTile = tile_tl;
					borderTop = true;
					borderLeft = true;
					break;
				case TileType.TR:
					wallsTile = tile_tr;
					borderTop = true;
					borderRight = true;
					break;
				case TileType.BL:
					wallsTile = tile_bl;
					borderBottom = true;
					borderLeft = true;
					break;
				case TileType.BR:
					wallsTile = tile_br;
					borderBottom = true;
					borderRight = true;
					break;
				case TileType.T:
					wallsTile = tile_t;
					borderTop = true;
					break;
				case TileType.B:
					wallsTile = tile_b;
					borderBottom = true;
					break;
				case TileType.L:
					wallsTile = tile_l;
					borderLeft = true;
					break;
				case TileType.R:
					wallsTile = tile_r;
					borderRight = true;
					break;
			}
		}

		// Paint the tile at the specified position
		if (baseTile != null) {
			_mapBaseLayer.SetTile(new Vector3Int(x, y, 0), baseTile);
		}
		if (iconsTile != null) {
			_mapIconsLayer.SetTile(new Vector3Int(x, y, 0), iconsTile);
		}
		if (pathsTile != null) {
			_mapPathsLayer.SetTile(new Vector3Int(x, y, 0), pathsTile);
		}
		if (wallsTile != null) {
			_mapWallsLayer.SetTile(new Vector3Int(x, y, 0), wallsTile);
		}

		// Paint the borders of this position (only bordering walls will be painted)
		if (borderTop) {
			_paintMapBorder(x, y, x, y + 1);
		}
		if (borderBottom) {
			_paintMapBorder(x, y, x, y - 1);
		}
		if (borderLeft) {
			_paintMapBorder(x, y, x - 1, y);
		}
		if (borderRight) {
			_paintMapBorder(x, y, x + 1, y);
		}
		// Paint the corners of this position
		if (borderTop && borderLeft) {
			_paintMapCorner(TileCornerType.BR, x - 1, y + 1);
		}
		if (borderTop && borderRight) {
			_paintMapCorner(TileCornerType.BL, x + 1, y + 1);
		}
		if (borderBottom && borderLeft) {
			_paintMapCorner(TileCornerType.TR, x - 1, y - 1);
		}
		if (borderBottom && borderRight) {
			_paintMapCorner(TileCornerType.TL, x + 1, y - 1);
		}
	}

	private void _paintMapCorner(TileCornerType type, int x, int y) {
		string position = x.ToString() + "," + y.ToString();
		TileCornerType existingType = _mapRenderedCorners.ContainsKey(position) ? _mapRenderedCorners[position] : TileCornerType.NONE;
		Tile cornersTile = null;

		switch (type) {
			case TileCornerType.TL:
				cornersTile = tile_ctl;
				break;
			case TileCornerType.TR:
				cornersTile = tile_ctr;
				break;
			case TileCornerType.BL:
				cornersTile = tile_cbl;
				break;
			case TileCornerType.BR:
				cornersTile = tile_cbr;
				break;
		}
		if (_mapRenderedCorners.ContainsKey(position)) {
			_mapRenderedCorners[position] = type | existingType;
		} else {
			_mapRenderedCorners.Add(position, type);
		}
		if (!MM_Constants_Map.BaseLayerTiles.ContainsKey(x.ToString() + "," + y.ToString())) {
			// Only draw additional corners if the cell we're drawing a corner on is not explorable
			bool populateTopLeft = (_mapRenderedCorners[position] & TileCornerType.TL) == TileCornerType.TL;
			bool populateTopRight = (_mapRenderedCorners[position] & TileCornerType.TR) == TileCornerType.TR;
			bool populateBottomLeft = (_mapRenderedCorners[position] & TileCornerType.BL) == TileCornerType.BL;
			bool populateBottomRight = (_mapRenderedCorners[position] & TileCornerType.BR) == TileCornerType.BR;
			cornersTile = _augmentMapCornerTile(cornersTile, populateTopLeft, populateTopRight, populateBottomLeft, populateBottomRight);
		}
		if (cornersTile != null) {
			Debug.Log("MM_MapController:_paintMapCorner : Drawing " + cornersTile + " at " + x.ToString() + "," + y.ToString());
			_mapCornersLayer.SetTile(new Vector3Int(x, y, 0), cornersTile);
		}
	}

	private Tile _augmentMapCornerTile(Tile tile, bool populateTopLeft, bool populateTopRight, bool populateBottomLeft, bool populateBottomRight) {
		if (tile == tile_ctl) {
			populateTopLeft = true;
		} else if (tile == tile_ctr) {
			populateTopRight = true;
		} else if (tile == tile_cbl) {
			populateBottomLeft = true;
		} else if (tile == tile_cbr) {
			populateBottomRight = true;
		}
		if (populateTopLeft && !populateTopRight && !populateBottomLeft && !populateBottomRight) {
			tile = tile_ctl;
		} else if (!populateTopLeft && populateTopRight && !populateBottomLeft && !populateBottomRight) {
			tile = tile_ctr;
		} else if (!populateTopLeft && !populateTopRight && populateBottomLeft && !populateBottomRight) {
			tile = tile_cbl;
		} else if (!populateTopLeft && !populateTopRight && !populateBottomLeft && populateBottomRight) {
			tile = tile_cbr;
		} else if (populateTopLeft && populateTopRight && !populateBottomLeft && !populateBottomRight) {
			tile = tile_ctltr;
		} else if (populateTopLeft && !populateTopRight && populateBottomLeft && !populateBottomRight) {
			tile = tile_ctlbl;
		} else if (populateTopLeft && !populateTopRight && !populateBottomLeft && populateBottomRight) {
			tile = tile_ctlbr;
		} else if (!populateTopLeft && populateTopRight && populateBottomLeft && !populateBottomRight) {
			tile = tile_ctrbl;
		} else if (!populateTopLeft && populateTopRight && !populateBottomLeft && populateBottomRight) {
			tile = tile_ctrbr;
		} else if (!populateTopLeft && !populateTopRight && populateBottomLeft && populateBottomRight) {
			tile = tile_cblbr;
		} else if (populateTopLeft && populateTopRight && populateBottomLeft && !populateBottomRight) {
			tile = tile_ctltrbl;
		} else if (populateTopLeft && populateTopRight && !populateBottomLeft && populateBottomRight) {
			tile = tile_ctltrbr;
		} else if (populateTopLeft && !populateTopRight && populateBottomLeft && populateBottomRight) {
			tile = tile_ctlblbr;
		} else if (!populateTopLeft && populateTopRight && populateBottomLeft && populateBottomRight) {
			tile = tile_ctrblbr;
		} else if (populateTopLeft && populateTopRight && populateBottomLeft && populateBottomRight) {
			tile = tile_ctltrblbr;
		}
		return tile;
	}

	private void _paintMapBorder(int sourceX, int sourceY, int x, int y) {
		Tile bordersTile = null;
		if (sourceX < x && sourceY == y) {
			bordersTile = tile_l;
		} else if (sourceX > x && sourceY == y) {
			bordersTile = tile_r;
		} else if (sourceX == x && sourceY < y) {
			bordersTile = tile_b;
		} else if (sourceX == x && sourceY > y) {
			bordersTile = tile_t;
		}
		if (!MM.player.ExploredMapCoordinates.Contains(x.ToString() + "," + y.ToString())) {
			// Only draw additional borders if the cell we're drawing a border on is not populated
			// If it is populated then that map cell will make its own call to _paintMap will draw the correct borders
			bool populateTop = MM.player.ExploredMapCoordinates.Contains(x.ToString() + "," + (y + 1).ToString());
			bool populateBottom = MM.player.ExploredMapCoordinates.Contains(x.ToString() + "," + (y - 1).ToString());
			bool populateLeft = MM.player.ExploredMapCoordinates.Contains((x - 1).ToString() + "," + y.ToString());
			bool populateRight = MM.player.ExploredMapCoordinates.Contains((x + 1).ToString() + "," + y.ToString());
			bordersTile = _augmentMapBorderTile(bordersTile, populateTop, populateBottom, populateLeft, populateRight);
		}
		if (bordersTile != null) {
			Debug.Log("MM_MapController:_paintMapBorder : Drawing " + bordersTile + " at " + x.ToString() + "," + y.ToString());
			_mapBordersLayer.SetTile(new Vector3Int(x, y, 0), bordersTile);
		}
	}

	private Tile _augmentMapBorderTile(Tile tile, bool populateTop, bool populateBottom, bool populateLeft, bool populateRight) {
		if (tile == tile_t) {
			populateTop = true;
		} else if (tile == tile_b) {
			populateBottom = true;
		} else if (tile == tile_l) {
			populateLeft = true;
		} else if (tile == tile_r) {
			populateRight = true;
		}
		if (populateLeft && !populateRight && !populateTop && !populateBottom) {
			tile = tile_l;
		} else if (!populateLeft && populateRight && !populateTop && !populateBottom) {
			tile = tile_r;
		} else if (!populateLeft && !populateRight && populateTop && !populateBottom) {
			tile = tile_t;
		} else if (!populateLeft && !populateRight && !populateTop && populateBottom) {
			tile = tile_b;
		} else if (populateLeft && populateRight && !populateTop && !populateBottom) {
			tile = tile_v2;
		} else if (populateLeft && !populateRight && populateTop && !populateBottom) {
			tile = tile_tl;
		} else if (populateLeft && !populateRight && !populateTop && populateBottom) {
			tile = tile_bl;
		} else if (!populateLeft && populateRight && populateTop && !populateBottom) {
			tile = tile_tr;
		} else if (!populateLeft && populateRight && !populateTop && populateBottom) {
			tile = tile_br;
		} else if (!populateLeft && !populateRight && populateTop && populateBottom) {
			tile = tile_h2;
		} else if (populateLeft && populateRight && populateTop && !populateBottom) {
			tile = tile_v1;
		} else if (populateLeft && populateRight && !populateTop && populateBottom) {
			tile = tile_v3;
		} else if (populateLeft && !populateRight && populateTop && populateBottom) {
			tile = tile_h1;
		} else if (!populateLeft && populateRight && populateTop && populateBottom) {
			tile = tile_h3;
		} else if (populateLeft && populateRight && populateTop && populateBottom) {
			tile = tile_closed;
		}
		return tile;
	}
}
