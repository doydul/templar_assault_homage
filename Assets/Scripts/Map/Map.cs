using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour, IPathable, ISpawnable {

  public TileGenerator tile_gen;

  private Dictionary<int, Dictionary<int, Transform>> _grid;
  private Dictionary<int, Dictionary<int, Transform>> _actors;

  public int width {
    get {
      return _grid.Values.Count;
    }
  }

  public int height {
    get {
      return _grid[0].Values.Count;
    }
  }

  void Awake() {
    _grid = new Dictionary<int, Dictionary<int, Transform>>();
    _actors = new Dictionary<int, Dictionary<int, Transform>>();
  }

  public void add_tile(Vector2 grid_location, Transform transform) {
    var grid_x = (int)grid_location.x;
    var grid_y = (int)grid_location.y;
    if (!_grid.ContainsKey(grid_x)) {
      _grid[grid_x] = new Dictionary<int, Transform>();
    }
    _grid[grid_x][grid_y] = transform;
  }

  public Transform get_tile_at(Vector2 grid_location) {
    var grid_x = (int)grid_location.x;
    var grid_y = (int)grid_location.y;
    if (_grid.ContainsKey(grid_x) && _grid[grid_x].ContainsKey(grid_y)) {
      return _grid[grid_x][grid_y];
    } else {
      return null;
    }
  }

  public List<Vector2> spawn_locations() {
    var result = new List<Vector2>();
    foreach (var tile in tiles()) {
      if (tile.is_spawner) result.Add(tile.grid_location);
    }
    return result;
  }

  public bool location_pathable(Vector2 grid_location) {
    var tile = get_tile_at(grid_location);
    return tile != null &&
           tile.GetComponent<Tile>().type == " ";
  }
  
  public bool location_spawnable(Vector2 grid_location) {
    return location_pathable(grid_location) && get_actor_at(grid_location) == null;
  }

  public void spawn(Vector2 grid_location) {
    tile_gen.spawn_alien(grid_location);
  }
  
  public float blockage(Vector2 grid_location) {
    if (get_actor_at(grid_location) != null) {
      return 0.35f;
    } else if (location_pathable(grid_location)) {
      return 0f;
    } else {
      return 1f;
    }
  }

  public void add_actor(Vector2 grid_location, Transform transform) {
    var grid_x = (int)grid_location.x;
    var grid_y = (int)grid_location.y;
    if (!_actors.ContainsKey(grid_x)) {
      _actors[grid_x] = new Dictionary<int, Transform>();
    }
    _actors[grid_x][grid_y] = transform;
  }

  public void remove_actor_at(Vector2 grid_location) {
    if (get_actor_at(grid_location) == null) return;
    var grid_x = (int)grid_location.x;
    var grid_y = (int)grid_location.y;
    _actors[grid_x].Remove(grid_y);
  }

  public Transform get_actor_at(Vector2 grid_location) {
    var grid_x = (int)grid_location.x;
    var grid_y = (int)grid_location.y;
    if (_actors.ContainsKey(grid_x) && _actors[grid_x].ContainsKey(grid_y)) {
      return _actors[grid_x][grid_y];
    } else {
      return null;
    }
  }

  public Vector2 actor_location(Transform actor) {
    for (int grid_x = 0; grid_x < width; grid_x++) {
      if (_actors.ContainsKey(grid_x)) {
        for (int grid_y = 0; grid_y < height; grid_y++) {
          if (_actors[grid_x].ContainsKey(grid_y)) {
            if (_actors[grid_x][grid_y] == actor) {
              return new Vector2(grid_x, grid_y);
            }
          }
        }
      }
    }
    return new Vector2(-1, -1);
  }

  public void move_actor(Vector2 grid_location, Vector2 new_grid_location) {
    var actor = get_actor_at(grid_location);
    remove_actor_at(grid_location);
    add_actor(new_grid_location, actor);
  }

  public List<Transform> actors() {
    var result = new List<Transform>();
    foreach (var sub_dict in _actors.Values) {
      foreach (var transform in sub_dict.Values) {
        result.Add(transform);
      }
    }
    return result;
  }

  public List<Alien> aliens() {
    var result = new List<Alien>();
    foreach (var transform in actors()) {
      if (transform.GetComponent<Alien>()) {
        result.Add(transform.GetComponent<Alien>());
      }
    }
    return result;
  }

  public List<Templar> templars() {
    var result = new List<Templar>();
    foreach (var transform in actors()) {
      if (transform.GetComponent<Templar>()) {
        result.Add(transform.GetComponent<Templar>());
      }
    }
    return result;
  }

  public List<Tile> tiles() {
    var result = new List<Tile>();
    foreach (var sub_dict in _grid.Values) {
      foreach (var transform in sub_dict.Values) {
        result.Add(transform.GetComponent<Tile>());
      }
    }
    return result;
  }

  public void clean_tiles() {
    foreach (var tile in tiles()) {
      tile.sprite_renderer.color = Color.white;
    }
  }

  public void highlight_tile_at(Vector2 location) {
    var tile_transform = get_tile_at(location);
    if (tile_transform != null) {
      tile_transform.GetComponent<Tile>().sprite_renderer.color = Color.red;
    }
  }
}
