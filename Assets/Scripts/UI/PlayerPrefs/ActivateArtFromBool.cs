using UnityEngine;

public class ActivateArtFromBool : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;

    private void Start()
    {
        // Lee el PlayerPrefs. Por defecto 0 (false) si no existe
        bool isActive = PlayerPrefs.GetInt("Art", 0) == 1;

        if (objectToActivate != null)
            objectToActivate.SetActive(isActive);
    }
}
