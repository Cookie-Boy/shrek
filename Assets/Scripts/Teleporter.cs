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
    [SerializeField] private float scaleDownDuration = 0.5f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.15f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float teleportSoundVolume = 0.7f;

    private bool isTeleporting = false;

    void Start()
    {
        if (shrekController == null)
            shrekController = FindAnyObjectByType<ShrekController>();
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

        if (teleportSound != null && audioSource != null)
            audioSource.PlayOneShot(teleportSound, teleportSoundVolume);

        StartCoroutine(ScreenShake());
        yield return ScaleDownShrek();

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

    IEnumerator ScreenShake()
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector3 offset = Random.insideUnitSphere * shakeStrength;
            cameraTransform.localPosition = originalPos + offset;

            yield return null;
        }

        cameraTransform.localPosition = originalPos;
    }

}
