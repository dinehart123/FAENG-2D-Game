using UnityEngine;
using UnityEngine.UI;

public class FontCalculator : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string text = "FÆNG";

    [Header("Appearance")]
    [SerializeField] private float letterHeight = 150f;
    [SerializeField] private float spacing = 0f;

    private const string GLYPH_PATH =
        "Fonts & Materials/WildAlphabet_bitmap_pack/glyphs/";

    private void Start()
    {
        SetupLayout();
        RebuildText();
    }

    public void SetText(string newText)
    {
        text = newText;
        RebuildText();
    }

    public void RebuildText()
    {
        ClearLetters();

        foreach (char character in text)
        {
            if (character == ' ')
            {
                CreateSpace();
                continue;
            }

            string spriteName = GetSpriteName(character);

            Sprite sprite = Resources.Load<Sprite>(
                GLYPH_PATH + spriteName
            );

            if (sprite == null)
            {
                Debug.LogWarning(
                    "Could not find sprite: " +
                    GLYPH_PATH +
                    spriteName
                );

                continue;
            }

            CreateLetter(character, sprite);
        }
    }

    private string GetSpriteName(char character)
    {
        // Your Æ sprite is named AE.png
        if (character == 'Æ' || character == 'æ')
        {
            return "AE";
        }

        return character.ToString().ToUpper();
    }

    private void CreateLetter(char character, Sprite sprite)
    {
        GameObject letterObject = new GameObject(
            character.ToString(),
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(LayoutElement)
        );

        letterObject.transform.SetParent(transform, false);

        Image image = letterObject.GetComponent<Image>();

        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        float aspectRatio =
            sprite.rect.width / sprite.rect.height;

        float width = letterHeight * aspectRatio;

        LayoutElement layout =
            letterObject.GetComponent<LayoutElement>();

        layout.preferredWidth = width;
        layout.preferredHeight = letterHeight;

        RectTransform rect =
            letterObject.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(
            width,
            letterHeight
        );
    }

    private void CreateSpace()
    {
        GameObject spaceObject = new GameObject(
            "Space",
            typeof(RectTransform),
            typeof(LayoutElement)
        );

        spaceObject.transform.SetParent(
            transform,
            false
        );

        LayoutElement layout =
            spaceObject.GetComponent<LayoutElement>();

        layout.preferredWidth =
            letterHeight * 0.35f;

        layout.preferredHeight =
            letterHeight;
    }

    private void ClearLetters()
    {
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                transform.GetChild(i).gameObject
            );
        }
    }

    private void SetupLayout()
    {
        HorizontalLayoutGroup layout =
            GetComponent<HorizontalLayoutGroup>();

        if (layout == null)
        {
            layout =
                gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layout.spacing = spacing;

        layout.childAlignment =
            TextAnchor.MiddleCenter;

        layout.childControlWidth = true;
        layout.childControlHeight = true;

        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }
}