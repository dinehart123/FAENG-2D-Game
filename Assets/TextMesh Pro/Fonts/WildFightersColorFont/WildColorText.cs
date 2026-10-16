using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class WildColorText : MonoBehaviour
{
    [TextArea] public string text = "FAENG";
    public Image glyphTemplate;
    public float height = 120f;
    public float spacing = 4f;
    public float spaceWidth = 45f;

    [Header("A-Z")]
    public Sprite[] letters = new Sprite[26];
    [Header("0-9")]
    public Sprite[] digits = new Sprite[10];
    [Header("Special")]
    public Sprite ae;

    readonly List<GameObject> generated = new List<GameObject>();

    void OnEnable() { Rebuild(); }
    void OnValidate() { Rebuild(); }

    [ContextMenu("Rebuild Color Text")]
    public void Rebuild()
    {
        if (glyphTemplate == null) return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.gameObject != glyphTemplate.gameObject)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(c.gameObject);
                else Destroy(c.gameObject);
#else
                Destroy(c.gameObject);
#endif
            }
        }

        float x = 0f;
        string s = (text ?? "").ToUpperInvariant();
        foreach (char ch in s)
        {
            if (ch == ' ') { x += spaceWidth; continue; }
            Sprite sp = GetSprite(ch);
            if (sp == null) continue;
            Image img = Instantiate(glyphTemplate, transform);
            img.gameObject.SetActive(true);
            img.sprite = sp;
            img.preserveAspect = true;
            RectTransform rt = img.rectTransform;
            float aspect = sp.rect.width / sp.rect.height;
            float w = height * aspect;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(w, height);
            x += w + spacing;
        }
        RectTransform self = transform as RectTransform;
        if (self != null) self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(0, x - spacing));
    }

    Sprite GetSprite(char ch)
    {
        if (ch == 'Æ') return ae;
        if (ch >= 'A' && ch <= 'Z') return letters[ch - 'A'];
        if (ch >= '0' && ch <= '9') return digits[ch - '0'];
        return null;
    }
}
