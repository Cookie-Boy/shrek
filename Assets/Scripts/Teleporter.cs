using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("Common Settings")]
    [SerializeField] private string sceneName = "Forest";
    [SerializeField] private ShrekController shrekController;

    [Header("Teleport Effects")]
    [SerializeField] private Transform shrekRoot;
    [SerializeField] private float scaleDownDuration = 2f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationDuration = 0.8f;
    [SerializeField] private float spinIntensity = 720f; // градусов в секунду
    [SerializeField] private float vortexIntensity = 360f; // для закручивания в воронку

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip teleportSound; // Это звук смыва унитаза
    [SerializeField] private float teleportSoundVolume = 0.7f;

    private bool isTeleporting = false;
    private Transform rotationTarget; // Объект, который будет вращаться

    void Start()
    {
        if (shrekController == null)
            shrekController = FindAnyObjectByType<ShrekController>();
        
        // Определяем целевой объект для вращения
        rotationTarget = shrekRoot.transform.root;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTeleporting)
            return;

        if (!other.GetComponentInParent<ShrekController>())
            return;

        if (shrekController != null && shrekController.IsTeleportAvailable)
        {
            shrekController.IsKeyboardBlocked = true;
            StartCoroutine(TeleportRoutine());
        }
    }

    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        // Запускаем вращение отдельно от уменьшения
        StartCoroutine(ContinuousRotation());
        yield return ScaleDownShrek();

        // Проигрываем звук смыва (teleportSound)
        if (teleportSound != null && audioSource != null)
            audioSource.PlayOneShot(teleportSound, teleportSoundVolume);

        // Ждем пока закончится звук смыва
        float delay = teleportSound != null ? teleportSound.length : 0f;
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ScaleDownShrek()
    {
        Vector3 startScale = shrekRoot.localScale;
        Vector3 endScale = Vector3.one * 0.05f;
        float t = 0f;

        while (t < scaleDownDuration)
        {
            t += Time.deltaTime;
            float k = t / scaleDownDuration;

            shrekRoot.localScale = Vector3.Lerp(startScale, endScale, k);
            
            yield return null;
        }

        shrekRoot.localScale = Vector3.zero;
    }

    IEnumerator ContinuousRotation()
    {
        // Вращение будет продолжаться даже после уменьшения модели
        float totalRotationTime = scaleDownDuration + (teleportSound != null ? teleportSound.length : 0f);
        float elapsedTime = 0f;

        while (elapsedTime < totalRotationTime)
        {
            elapsedTime += Time.deltaTime;
            
            // Вращаем корневой объект
            if (rotationTarget != null)
            {
                rotationTarget.Rotate(0f, spinIntensity * Time.deltaTime, 0f, Space.World);
            }
            
            yield return null;
        }
    }
}