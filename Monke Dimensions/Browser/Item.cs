#if EDITOR

#else

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Capu_Dimensions.Browser;

public class Item : MonoBehaviour
{
    public string MapName { get; set; }
    public string MapDownload { get; set; }
    public string MapImageUrl { get; set; }

    public RawImage mapImage;
    public float scaleFactor = 1.1f;
    public float duration = 0.2f;

    private Vector3 originalScale;

    public static Item selectedItem;

    private void Start()
    {
        transform.GetChild(0).GetComponent<Text>().text = MapName;
        gameObject.layer = 18;
        originalScale = transform.localScale;
    }

    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        targetScale = transferOrigScale;
        duration = transferDuration;
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
    Vector3 transferOrigScale;
    float transferDuration;
    private void OnTriggerEnter(Collider collider)
    {
        if (0.25f >= Time.time) return;

        //var hand = collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
        if (collider.gameObject.name == "RFingerTip" || collider.gameObject.name == "LFingerTip")
        {
            //GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(211, hand.isLeftHand, 0.12f);
            //GorillaTagger.Instance.StartVibration(hand.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            if (selectedItem != null && selectedItem != this)
            {
                transferOrigScale = selectedItem.originalScale;
                transferDuration = duration;
                selectedItem.StartCoroutine("ScaleOverTime");
            }

            if (selectedItem == this)
            {
                transferOrigScale = originalScale;
                transferDuration = duration;
                StartCoroutine("ScaleOverTime");
                selectedItem = null;
            }
            else
            {
                Vector3 targetScaleVector = originalScale * scaleFactor;
                transferOrigScale = targetScaleVector;
                transferDuration = duration;
                StartCoroutine("ScaleOverTime");
                selectedItem = this;
            }
        }
    }

    [System.Serializable]
    public class DimensionItemData
    {
        public string Name;
        public string Download;
        public string Image;
    }

    [System.Serializable]
    public class DimensionItemDataWrapper
    {
        public List<DimensionItemData> Dimensions;
    }
}
#endif