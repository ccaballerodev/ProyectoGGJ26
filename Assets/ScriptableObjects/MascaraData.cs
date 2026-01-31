using UnityEngine;

[CreateAssetMenu(fileName = "NuevaMascara", menuName = "ScriptableObjects/MascaraData")]
public class MascaraData : ScriptableObject
{
    public MaskType tipo;      // El enum (Mascara1, Mascara2, etc.)
    public string nombreMascara; // El nombre que aparecerá en txtRol
    public Sprite iconoMascara;  // El sprite para la transición grande
}