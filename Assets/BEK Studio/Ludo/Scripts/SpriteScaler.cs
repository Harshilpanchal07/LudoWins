using UnityEngine;

namespace BEKStudio {
    public class SpriteScaler : MonoBehaviour {
        void OnEnable() {
            ScaleSpriteToScreen();
        }

        void ScaleSpriteToScreen() {
            Camera mainCamera = Camera.main;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            float screenHeight = mainCamera.orthographicSize * 2.0f;
            float screenWidth = screenHeight * Screen.width / Screen.height;

            Vector2 scale = transform.localScale;
            scale.x = screenWidth / spriteSize.x;
            scale.y = scale.x;

            transform.localScale = scale;
        }
    }
}
