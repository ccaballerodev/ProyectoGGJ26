using UnityEngine;
using System.IO;

public class SaveImageWindows : MonoBehaviour
{
    [SerializeField] private Texture2D textureToSave;
    [SerializeField] private string fileName = "MiImagen.png";

    public void SaveImageAs()
    {
        if (textureToSave == null)
        {
            Debug.LogError("No hay textura asignada.");
            return;
        }

        // Copiar la textura a una nueva Texture2D para asegurarse de que sea readable
        Texture2D tex = new Texture2D(textureToSave.width, textureToSave.height, TextureFormat.RGBA32, false);
        tex.SetPixels(textureToSave.GetPixels());
        tex.Apply();

        // Convertir a PNG
        byte[] bytes = tex.EncodeToPNG();

        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("No se pudo convertir la textura a PNG.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, bytes);

        Debug.Log("Imagen guardada en: " + path);
    }
}