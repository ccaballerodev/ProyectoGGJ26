using UnityEngine;
using System.Collections; 

public class FanCreator : MonoBehaviour
{
    [Header("Configuración de Prefab")]
    public GameObject objectToBlowPrefab;
    public Transform centralSpawnPoint;
    public float spawnInterval = 0.15f;

    [Header("Configuración de Ráfaga")]
    public int objectsPerBurst = 3;
    public float spawnRadius = 1.5f;

    [Header("Fuerza Lineal Pura")]
    public float blowForce = 20f;

    [Header("Rotación y Límites")]
    public float torqueAmount = 10f;
    public float maxDistance = 30f;
    public float maxLifeTime = 5f;

    private void Start()
    {
        if (centralSpawnPoint == null) centralSpawnPoint = this.transform;
        StartCoroutine(BurstRoutine());
    }

    IEnumerator BurstRoutine()
    {
        while (true)
        {
            for (int i = 0; i < objectsPerBurst; i++)
            {
                SpawnObjectOnPerimeter();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObjectOnPerimeter()
    {
        if (!objectToBlowPrefab) return;

        // --- LA SOLUCIÓN AL SPREAD ---
        // En lugar de "insideUnitCircle", usamos un ángulo aleatorio para estar solo en el borde
        float angle = Random.Range(0f, Mathf.PI * 2f);

        // Calculamos la posición exacta en el borde del radio
        float x = Mathf.Cos(angle) * spawnRadius;
        float y = Mathf.Sin(angle) * spawnRadius;

        Vector3 offset = (centralSpawnPoint.right * x) + (centralSpawnPoint.up * y);
        Vector3 spawnPos = centralSpawnPoint.position + offset;

        // Instanciar
        GameObject cube = Instantiate(objectToBlowPrefab, spawnPos, centralSpawnPoint.rotation);

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Forzamos que la velocidad sea un vector limpio hacia adelante
            // Usamos Velocity directamente para que no haya acumulación errática de fuerzas
            rb.linearVelocity = centralSpawnPoint.forward * blowForce;

            // Torque para que gire pero no se desvíe
            rb.AddTorque(Random.insideUnitSphere * torqueAmount, ForceMode.Impulse);
        }

        StartCoroutine(DestructionTimer(cube, spawnPos));
    }

    IEnumerator DestructionTimer(GameObject obj, Vector3 startPos)
    {
        float t = 0;
        while (obj != null && t < maxLifeTime)
        {
            if (Vector3.Distance(startPos, obj.transform.position) > maxDistance) break;
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }
}