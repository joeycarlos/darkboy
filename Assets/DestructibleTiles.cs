using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructibleTiles : MonoBehaviour
{
    private Tilemap tm;

    void Start() {
        LinkComponents();
    }

    void LinkComponents() {
        tm = GetComponent<Tilemap>();
    }

    public void DestroyTiles(Vector2 startLocation, Vector2 dimensions) {

        // bounds position starts at bottom left
        // BoundsInt bounds = new BoundsInt(new Vector3Int(3, -1, 0), new Vector3Int(3, 3, 1));

        // player can only hit floor, so expand bounds to include blocks above as well
        int defaultDestroyHeight = 5;

        Vector3Int boundsOrigin = new Vector3Int(Mathf.RoundToInt(startLocation.x), Mathf.RoundToInt(startLocation.y), 0);
        Vector3Int boundsSize = new Vector3Int(Mathf.RoundToInt(dimensions.x), defaultDestroyHeight, 1);

        BoundsInt bounds = new BoundsInt(boundsOrigin, boundsSize);

        foreach (Vector3Int position in bounds.allPositionsWithin) {
            TileBase t = tm.GetTile(position);
            tm.SetTile(position, null);
        }

    }

    public BoundsInt Bounds;
    public Vector3Int Vector;

    void OnDrawGizmosSelected() {
        var offset = new Vector3(.5f, .5f, 0f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Bounds.min + offset, Vector3Int.one);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Bounds.max + offset, Vector3Int.one);
        Gizmos.color = Bounds.Contains(Bounds.position + Vector) ? Color.green : Color.red;
        Gizmos.DrawWireCube(Bounds.position + Vector + offset, Vector3Int.one);
    }
}
