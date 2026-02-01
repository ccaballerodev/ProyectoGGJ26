using UnityEngine;

[System.Serializable]
public struct PlayerAudioClip
{
    public PlayerSound sound;      // Tipo de sonido
    public AudioClip clip;         // Clip a reproducir
    public bool isReplicable;      // Si debe replicarse a otros jugadores
}

public enum PlayerSound
{
    Jump,
    Run,
    PowerStop,
    PowerMin,
    PowerPlus,
    PowerBack
}

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioManager : MonoBehaviour
{
    public static PlayerAudioManager Instance { get; private set; }

    [Header("Clips del jugador")]
    [SerializeField] private PlayerAudioClip[] audioClips;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Reproduce un sonido del jugador desde su AudioSource
    /// </summary>
    /// <param name="sound">Tipo de sonido</param>
    /// <returns>True si el sonido es replicable</returns>
    public bool PlaySound(PlayerSound sound)
    {
        PlayerAudioClip clipData = GetClipData(sound);
        if (clipData.clip == null)
        {
            Debug.LogWarning($"No se encontró el clip para {sound}");
            return false;
        }

        audioSource.PlayOneShot(clipData.clip);
        return clipData.isReplicable;
    }

    /// <summary>
    /// Obtiene la información del clip
    /// </summary>
    private PlayerAudioClip GetClipData(PlayerSound sound)
    {
        foreach (var entry in audioClips)
        {
            if (entry.sound == sound)
                return entry;
        }
        return default;
    }
}