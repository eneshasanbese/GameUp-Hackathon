using UnityEngine;

// Place on the supporting collider/tilemap, or its parent. Dryness remains on its layer.
[DisallowMultipleComponent]
public sealed class GroundSurface : MonoBehaviour
{
    public enum Material { Unspecified, Sand, Grass, BlackStone }

    [SerializeField] private Material surfaceMaterial;

    public Material SurfaceMaterial => surfaceMaterial;
}
