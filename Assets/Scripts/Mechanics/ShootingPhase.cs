using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShootingPhase : Phase {

  private int _move_counter;

  private Templar _selected_unit;
  private Vector2 _selected_square;

  private List<Vector2> templar_positions;

  public const int ENEMY_MOVE = 4;
  public const int FOG_DISTANCE = 8;

  public ShootingPhase(Map map) : base(map) {
    _move_counter = 0;
    Debug.Log("Shooting Phase");
    templar_positions = new List<Vector2>();
    foreach (var templar in map.templars()) {
      templar_positions.Add(map.actor_location(templar.transform));
    }
  }

  public override void click(Vector2 grid_location) {
    var target = map.get_actor_at(grid_location);
    if (target != null) {
      var templar = target.GetComponent<Templar>();
      if (templar != null) {
        _selected_unit = templar;
        _selected_square = grid_location;
        map.clean_tiles();
        highlight_targets();
      } else {
        var alien = target.GetComponent<Alien>();
        if (alien != null && _selected_unit != null && _selected_unit.can_shoot() && _selected_unit.within_arc(_selected_square, grid_location)) {
          shoot(grid_location, _selected_unit, alien);
        } else {
          Debug.Log("Invalid target");
          Debug.Log("Direction: " + _selected_unit.direction);
        }
      }
    }
  }

  public override void keypress(string key) {
    if (key == "n") {
      if (_move_counter < 2) move_aliens();
    }
  }

  public override bool can_progress() {
    return _move_counter >= 2;
  }

  public override Phase next_phase() {
    map.clean_tiles();
    foreach (var templar in map.templars()) {
      templar.reset_turn();
    }
    spawn_aliens();
    return new MovementPhase(map);
  }
  
  private void spawn_aliens() {
    var dir = new Director(map);
    var player_positions = new List<Vector2>();
    foreach (var templar in map.templars()) {
      player_positions.Add(map.actor_location(templar.transform));
    }
    dir.spawn_aliens(player_positions);
    foreach (var alien in map.aliens()) {
      if (in_fog(map.actor_location(alien.transform))) {
        alien.hide();
      } else {
        alien.show();
      }
    }
  }

  private void shoot(Vector2 grid_location, Templar shooter, Alien target) {
    shooter.shoot();

    var damage = new Damage(shooter.damage, target.armour, shooter.accuracy).calculate();
    target.hurt(damage);
  }

  private void move_aliens() {
    _move_counter++;

    foreach (var alien in map.aliens()) {
      var position = map.actor_location(alien.transform);
      var path_finder = new PathFinder(map, position, templar_positions);
      var path = path_finder.find_path();
      List<Vector2> slice = null;
      if (ENEMY_MOVE + 1 > path.Count) {
        slice = path;
      } else {
        slice = path.GetRange(0, ENEMY_MOVE + 1);
      }
      slice.Reverse();

      var new_position = position;

      foreach (var slice_pos in slice) {
        if (map.get_actor_at(slice_pos) == null) {
          alien.move(position, slice_pos);
          new_position = slice_pos;
          if (in_fog(slice_pos)) {
            alien.hide();
          } else {
            alien.show();
          }
          break;
        }
      }
      alien.attack(new_position);
    }
  }

  private bool in_fog(Vector2 location) {
    bool result = true;
    foreach (var player_location in templar_positions) {
      var dist = Mathf.Abs(location.x - player_location.x) + Mathf.Abs(location.y - player_location.y);
      if (dist < FOG_DISTANCE) result = false;
    }
    return result;
  }

  private void highlight_targets() {
    foreach (var alien in map.aliens()) {
      var pos = map.actor_location(alien.transform);
      if (!in_fog(pos) && _selected_unit.within_arc(_selected_square, pos)) {
        map.highlight_tile_at(pos);
      }
    }
  }
}
