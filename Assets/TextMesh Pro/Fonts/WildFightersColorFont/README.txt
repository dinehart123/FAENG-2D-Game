WILD FIGHTERS COLOR FONT — Unity Pack

This is a full-color sprite font made from the supplied animal/nature alphabet artwork.
It preserves the original colors and textures, which a normal TTF/OTF cannot do reliably in TextMeshPro.

Included glyphs:
A-Z, 0-9, Æ (file AE.png). Lowercase input is automatically converted to uppercase.

UNITY SETUP
1. Copy the WildFightersColorFont folder into your Unity project's Assets folder.
2. Select all PNGs in Glyphs. In the Inspector set Texture Type = Sprite (2D and UI), then Apply.
3. Put WildColorText.cs on an empty UI GameObject under a Canvas.
4. Create a child GameObject named GlyphTemplate with an Image component, disable it, and assign it to the script's Glyph Template field.
5. Assign each sprite A-Z, 0-9 and AE in the inspector.
6. Enter text in the Text field. The component lays out the full-color glyphs automatically.

For the character Æ, type Æ directly in the Text field.

TIP: This is best for titles, menus, fighter names, victory text, and logos. Use a normal TMP font for paragraphs/body text.
