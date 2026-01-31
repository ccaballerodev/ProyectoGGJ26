using UnityEngine;

public class SetArtBool : MonoBehaviour
{
    /// <summary>
    /// Establece el bool como true
    /// </summary>
    public void SetArtTrue()
    {
        PlayerPrefs.SetInt("Art", 1); // 1 = true
        PlayerPrefs.Save(); // Asegura que se guarde en disco
    }

    /// <summary>
    /// Establece el bool como false
    /// </summary>
    public void SetArtFalse()
    {
        PlayerPrefs.SetInt("Art", 0); // 0 = false
        PlayerPrefs.Save();
    }
}
