using UnityEngine;

[CreateAssetMenu(fileName = "BagData", menuName = "Security/BagData")]
public class BagData : ScriptableObject
{
    [Header("Contenido")]
    public string[] items;          // nombres de los objetos que trae
    public bool hasDangerousItem;   // true = hay que reportar, false = dejar pasar

    [Header("Descripcion opcional")]
    [TextArea] public string notes; // ej: "Trae una navaja escondida"
}
