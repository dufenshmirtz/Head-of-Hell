using UnityEngine;

public class IndicatorRotationHandler : MonoBehaviour
{
    private float defaultScaleX;

    void Awake()
    {
        // Store the default X scale from the indicator itself
        defaultScaleX = transform.localScale.x;
    }

    void LateUpdate()
    {
        // Get the parent's X scale sign (P1's scale.x)
        float parentScaleX = transform.parent.localScale.x;

        // Flip the indicator's X scale to cancel parent's flip
        Vector3 scale = transform.localScale;
        scale.x = defaultScaleX * Mathf.Sign(1f / parentScaleX); // If parent flips, we unflip
        transform.localScale = scale;

        // Optional: lock rotation upright just in case
        transform.localRotation = Quaternion.identity;
    }
}
