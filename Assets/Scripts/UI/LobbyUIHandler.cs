using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIHandler : NetworkBehaviour
{
    [System.Serializable]
    public class PlayerUISlot
    {
        public Image maskImage;
        public TextMeshProUGUI nameText;
        public GameObject container;
    }

    [Header("Base de Datos de Máscaras")]
    public List<MascaraData> baseDeDatosMascaras;
    public Sprite spriteVacio;

    [Header("UI Jugador Local")]
    public TextMeshProUGUI txtRol;
    public Image mascaraPlayerImage;

    [Header("Slots del Lobby (UI)")]
    public PlayerUISlot[] playerSlots;

    [Header("Control del Host")]
    [SerializeField] private Button btnIniciarJuego;

    private GameObject objPreGame;
    public static bool objPreGameisActive;

    [Header("Navegación")]
    [SerializeField] private SceneFade sceneFade;

    private void Start()
    {
        Time.timeScale = 0f;
    }

    public override void OnNetworkSpawn()
    {
        // Solo el Host activa el botón en su pantalla
        if (IsHost && btnIniciarJuego != null)
        {
            btnIniciarJuego.gameObject.SetActive(true);
            btnIniciarJuego.interactable = false;

            // Asignamos la función al botón por código por seguridad
            btnIniciarJuego.onClick.AddListener(PresionarBotonIniciar);
        }
    }

    public void AbandonarPartida()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        Time.timeScale = 1f;

        if (sceneFade != null)
        {
            sceneFade.LoadScene("MainMenu"); 
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    void Update()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            ActualizarInterfazLobby();
        }

        
    }

    private void ActualizarInterfazLobby()
    {
        // Limpieza de slots
        foreach (var slot in playerSlots)
        {
            slot.maskImage.sprite = spriteVacio;
            slot.nameText.text = "Esperando...";
            slot.maskImage.color = new Color(1, 1, 1, 0.2f);
        }

        PlayerMask[] todosLosJugadores = GameObject.FindObjectsOfType<PlayerMask>();

        // Lógica de validación del Host
        if (IsHost && btnIniciarJuego != null)
        {
            // Se habilita solo cuando hay 4 jugadores exactamente (o más)
            btnIniciarJuego.interactable = (todosLosJugadores.Length >= 1);
        }

        foreach (PlayerMask jugador in todosLosJugadores)
        {
            if (jugador == null || !jugador.IsSpawned) continue;

            MascaraData datos = BuscarDatosMascara(jugador.assignedMask.Value);
            int id = (int)jugador.OwnerClientId;

            if (id >= 0 && id < playerSlots.Length)
            {
                playerSlots[id].nameText.text = "Player " + (id + 1);
                playerSlots[id].maskImage.sprite = (datos != null) ? datos.iconoMascara : spriteVacio;
                playerSlots[id].maskImage.color = (datos != null) ? Color.white : new Color(1, 1, 1, 0.2f);
            }

            if (jugador.IsOwner)
            {
                if (datos != null)
                {
                    txtRol.text = datos.nombreMascara;
                    mascaraPlayerImage.sprite = datos.iconoMascara;
                    mascaraPlayerImage.color = Color.white;
                }
                else
                {
                    txtRol.text = "Sin Rol";
                    mascaraPlayerImage.sprite = spriteVacio;
                }
            }
        }
    }

    private void PresionarBotonIniciar()
    {
        // Solo el servidor/host tiene autoridad para llamar al RPC
        if (IsHost)
        {
            IniciarJuegoClientRpc();
        }
    }

    [ClientRpc]
    private void IniciarJuegoClientRpc()
    {
        // Esta línea se ejecuta en TODOS los jugadores conectados
        Time.timeScale = 1f;

        // Aquí puedes poner tu lógica de "quitar la UI de lobby"
        Debug.Log("Sincronización de inicio recibida por todos.");

        // Si tienes el Canvas del lobby en una variable, podrías hacer:
        // lobbyCanvas.SetActive(false);
    }

    private MascaraData BuscarDatosMascara(MaskType tipo)
    {
        if (tipo == MaskType.Ninguna) return null;
        return baseDeDatosMascaras.Find(m => m.tipo == tipo);
    }
}