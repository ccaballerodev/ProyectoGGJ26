using Unity.Netcode;
using UnityEngine;

public enum MaskType { Ninguna = -1, Mascara1, Mascara2, Mascara3, Mascara4 }

public class PlayerMask : NetworkBehaviour
{
    public NetworkVariable<MaskType> assignedMask = new NetworkVariable<MaskType>(
        MaskType.Ninguna,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Modelos Visuales")]
    [SerializeField] private GameObject model1;
    [SerializeField] private GameObject model2;
    [SerializeField] private GameObject model3;
    [SerializeField] private GameObject model4;

    public override void OnNetworkSpawn()
    {
        assignedMask.OnValueChanged += (oldVal, newVal) => ActualizarModelo(newVal);

        if (assignedMask.Value != MaskType.Ninguna) ActualizarModelo(assignedMask.Value);
        else DesactivarTodosLosModelos();
    }

    private void ActualizarModelo(MaskType tipo)
    {
        DesactivarTodosLosModelos();

        switch (tipo)
        {
            case MaskType.Mascara1: if (model1) model1.SetActive(true); break;
            case MaskType.Mascara2: if (model2) model2.SetActive(true); break;
            case MaskType.Mascara3: if (model3) model3.SetActive(true); break;
            case MaskType.Mascara4: if (model4) model4.SetActive(true); break;
        }

        // IMPORTANTE: Avisar al movimiento que el Animator ha cambiado
        // Lo hacemos con un pequeño retraso o frame para que SetActive surta efecto
        Invoke(nameof(NotificarCambioAnimator), 0.05f);
    }

    private void NotificarCambioAnimator()
    {
        if (TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.RecargarAnimator();
        }
    }

    private void DesactivarTodosLosModelos()
    {
        if (model1) model1.SetActive(false);
        if (model2) model2.SetActive(false);
        if (model3) model3.SetActive(false);
        if (model4) model4.SetActive(false);
    }
}