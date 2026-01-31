using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityTesterPC_Generic : MonoBehaviour
{
    [Header("PC Key (New Input System)")]
    [SerializeField] private Key testKey = Key.E;

    [Header("Opcional")]
    [SerializeField] private bool includeInactive = false; // si quieres incluir objetos desactivados

    private InputAction testAction;

    private void Awake()
    {
        testAction = new InputAction(
            name: "TestAllAbilities",
            type: InputActionType.Button,
            binding: $"<Keyboard>/{testKey.ToString().ToLower()}"
        );
    }

    private void OnEnable()
    {
        testAction.performed += OnTestPerformed;
        testAction.Enable();
    }

    private void OnDisable()
    {
        testAction.performed -= OnTestPerformed;
        testAction.Disable();
    }

    private void OnDestroy()
    {
        testAction?.Dispose();
    }

    private void OnTestPerformed(InputAction.CallbackContext ctx)
    {
        int count = 0;

        // Activos por defecto (más rápido y suficiente casi siempre)
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var b in behaviours)
        {
            if (b == null) continue;

            // Si quieres incluir inactivos, esto solo funcionará si el objeto está activo en jerarquía.
            // Para verdaderamente incluir inactivos, mira el bloque alternativo debajo.
            if (!includeInactive && !b.isActiveAndEnabled) continue;

            if (b is IAbilityUsable usable)
            {
                count++;
                Debug.Log($"[AbilityTester] UseAbility() -> {b.gameObject.name} ({b.GetType().Name})");
                usable.UseAbility();
            }
        }

        Debug.Log($"[AbilityTester] Total UseAbility ejecutados: {count}");
    }
}