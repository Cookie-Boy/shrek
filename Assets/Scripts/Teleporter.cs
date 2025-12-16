using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneName = "NextLevel";
    [SerializeField] private ShrekController shrekController;

    void Start()
    {
        if (shrekController == null)
        {
            Debug.Log("Shrek controller is null");
            shrekController = FindAnyObjectByType<ShrekController>();

            if (shrekController == null)
            {
                Debug.LogWarning("Teleporter: ShrekController not found!");
            }
        }
    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (shrekController != null && shrekController.IsTeleportAvailable)
        {
            shrekController.IsTeleportAvailable = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Сначала соберите всю еду!");
        }
    }
}