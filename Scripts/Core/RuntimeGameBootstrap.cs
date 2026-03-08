using UnityEngine;
using UnityEngine.SceneManagement;

public static class RuntimeGameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGameBootstrap()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu")
        {
            return;
        }

        GameManager manager = Object.FindFirstObjectByType<GameManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("GameManager_Auto");
            manager = managerObject.AddComponent<GameManager>();
            Debug.Log("RuntimeGameBootstrap: Auto-created GameManager.");
        }

        EnsureCamera();

        // В полноценной UI-сцене не создаем старый debug-overlay.
        bool hasStructuredUi = Object.FindFirstObjectByType<UIManager>() != null
            || Object.FindFirstObjectByType<GameUIController>() != null;

        if (!hasStructuredUi)
        {
            EnsureDebugTableOnly();
            EnsureSimpleDebugOverlay(manager);
        }
    }

    private static void EnsureSimpleDebugOverlay(GameManager manager)
    {
        DebugGameUI debugUi = Object.FindFirstObjectByType<DebugGameUI>();
        if (debugUi == null)
        {
            GameObject uiObject = new GameObject("DebugGameUI_Auto");
            debugUi = uiObject.AddComponent<DebugGameUI>();
            Debug.Log("RuntimeGameBootstrap: Auto-created DebugGameUI.");
        }
    }

    private static void EnsureDebugTableOnly()
    {
        SpriteRenderer table = GameObject.Find("Table_Auto")?.GetComponent<SpriteRenderer>();
        if (table == null)
        {
            GameObject tableObject = new GameObject("Table_Auto");
            table = tableObject.AddComponent<SpriteRenderer>();
            table.sortingOrder = -100;
            table.transform.position = new Vector3(0f, 0f, 0f);
            table.transform.localScale = new Vector3(40f, 24f, 1f);
            Debug.Log("RuntimeGameBootstrap: Auto-created full-screen table.");
        }

        Sprite tableSprite = Resources.Load<Sprite>("Environment/table");
        table.sprite = tableSprite != null
            ? tableSprite
            : CreateSolidSprite(new Color(0.12f, 0.08f, 0.06f, 1f));

        FitTableToCamera(table, 1.08f);

        GameObject oldBackground = GameObject.Find("Background_Auto");
        if (oldBackground != null)
        {
            Object.Destroy(oldBackground);
        }
    }


    private static void FitTableToCamera(SpriteRenderer table, float fillMultiplier)
    {
        if (table == null)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic)
        {
            return;
        }

        Sprite sprite = table.sprite;
        if (sprite == null)
        {
            return;
        }

        float worldHeight = camera.orthographicSize * 2f * fillMultiplier;
        float worldWidth = worldHeight * camera.aspect * fillMultiplier;

        Vector2 spriteSize = sprite.bounds.size;
        if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f)
        {
            return;
        }

        table.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
        table.transform.position = new Vector3(0f, 0f, 0f);
    }

    private static void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            Debug.Log("RuntimeGameBootstrap: Auto-created main camera.");
        }
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
