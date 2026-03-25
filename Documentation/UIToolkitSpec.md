# Unity UI Toolkit Spec — Unity 6000.3.x (6000.3.2f1)

## UXML

### Rules

1. Root element must be `\<engine:UXML ...\>` (or an equivalent prefix bound to `UnityEngine.UIElements`).
2. Only use UI Toolkit UXML elements (no HTML tags like `\<div\>`, `\<span\>`).
3. Runtime elements are in `UnityEngine.UIElements`; `UnityEditor.UIElements` elements are Editor-only.
4. Common `VisualElement` attributes allowed:

   * `name`
   * `class` (space-separated USS classes)
   * `style` (inline USS)
   * `picking-mode` (`Position` | `Ignore`)
   * `focusable` (bool)
   * `tabindex` (int)
   * `tooltip` (Editor-only)
   * `view-data-key`
   * `usage-hints`
5. UXML file references:

   * Use `\<Style src="..."\>` or `\<Style path="..."\>` to reference `.uss` / `.tss`.
   * Use `\<Template src="..."\>` or `\<Template path="..."\>` plus `\<Instance template="..."\>` for templates.
6. Template rule: only one element per template should define `content-container`.

### Allowed UXML Elements (complete list, Unity 6000.3)

#### Base

* `BindableElement`
* `VisualElement`

#### Runtime Controls (`UnityEngine.UIElements`)

* `BoundsField`
* `BoundsIntField`
* `Box`
* `Button`
* `DoubleField`
* `DropdownField`
* `EnumField`
* `FloatField`
* `Foldout`
* `GroupBox`
* `Hash128Field`
* `HelpBox`
* `IMGUIContainer`
* `Image`
* `IntegerField`
* `Label`
* `ListView`
* `LongField`
* `MinMaxSlider`
* `MultiColumnListView`
* `MultiColumnTreeView`
* `PopupWindow`
* `ProgressBar`
* `RadioButton`
* `RadioButtonGroup`
* `RectField`
* `RectIntField`
* `RepeatButton`
* `ScrollView`
* `Scroller`
* `Slider`
* `SliderInt`
* `Tab`
* `TabView`
* `TemplateContainer`
* `TextElement`
* `TextField`
* `Toggle`
* `ToggleButtonGroup`
* `TreeView`
* `TwoPaneSplitView`
* `UnsignedIntegerField`
* `UnsignedLongField`
* `Vector2Field`
* `Vector2IntField`
* `Vector3Field`
* `Vector3IntField`
* `Vector4Field`

#### Editor-only Controls (`UnityEditor.UIElements`)

* `ColorField`
* `CurveField`
* `EnumFlagsField`
* `GradientField`
* `InspectorElement`
* `LayerField`
* `LayerMaskField`
* `Mask64Field`
* `MaskField`
* `ObjectField`
* `PropertyField`
* `RenderingLayerMaskField`
* `TagField`
* `Toolbar`
* `ToolbarBreadcrumbs`
* `ToolbarButton`
* `ToolbarMenu`
* `ToolbarPopupSearchField`
* `ToolbarSearchField`
* `ToolbarSpacer`
* `ToolbarToggle`

#### Template/Struct

* `Template`
* `Instance`
* `Columns`
* `Column`

#### C# only (no UXML)

* `GenericDropdownMenu`

---

## USS

### Rules

1. USS is CSS-like but **not** full CSS.
2. Allowed selector types:

   * Type
   * Name
   * Class
   * Universal (`*`)
   * Descendant (space)
   * Child (`>`)
   * Multiple/compound
   * Selector list (`,`)
3. Allowed pseudo-classes only:

   * `:hover`, `:active`, `:inactive`, `:focus`, `:disabled`, `:enabled`, `:checked`, `:root`
   * `:selected` is **not** supported; use `:checked`.
4. Allowed length units: `px`, `%`.

   * Unitless numbers for length are treated as `px`.
   * `0` may omit unit.
5. Allowed color formats:

   * `#RGB` / `#RRGGBB`
   * `rgb()` / `rgba()`
   * color keywords
6. Asset refs: `url("...")` or `resource("...")` only.
7. Use only supported USS properties from the official list (below). Unknown properties must not be emitted.

### Supported USS properties (complete list)

```text
align-content, align-items, align-self, all, aspect-ratio,
background-color, background-image, background-position, background-position-x, background-position-y,
background-repeat, background-size,
border-bottom-color, border-bottom-left-radius, border-bottom-right-radius, border-bottom-width,
border-color, border-left-color, border-left-width, border-radius, border-right-color, border-right-width,
border-top-color, border-top-left-radius, border-top-right-radius, border-top-width, border-width,
bottom, color, cursor, display, filter,
flex, flex-basis, flex-direction, flex-grow, flex-shrink, flex-wrap, font-size,
height, justify-content, left, letter-spacing,
margin, margin-bottom, margin-left, margin-right, margin-top,
max-height, max-width, min-height, min-width,
opacity, overflow,
padding, padding-bottom, padding-left, padding-right, padding-top,
position, right,
rotate, scale, text-overflow, text-shadow, top, transform-origin,
transition, transition-delay, transition-duration, transition-property, transition-timing-function,
translate,
-unity-background-image-tint-color, -unity-background-scale-mode, -unity-editor-text-rendering-mode,
-unity-font, -unity-font-definition, -unity-font-style, -unity-material, -unity-overflow-clip-box,
-unity-paragraph-spacing, -unity-slice-bottom, -unity-slice-left, -unity-slice-right, -unity-slice-scale,
-unity-slice-top, -unity-slice-type,
-unity-text-align, -unity-text-auto-size, -unity-text-generator,
-unity-text-outline, -unity-text-outline-color, -unity-text-outline-width, -unity-text-overflow-position,
visibility, white-space, width, word-spacing
```
