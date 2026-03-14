using Game.Data;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    ///     Yolcu veya otobüs gibi objelere renk ataması yapmaya yarayan metotları içerir
    /// </summary>
    public static class LevelColorHandler
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static void ApplyColor(GameObject target, PassengerColorType colorType)
        {
            if (target == null)
            {
                return;
            }

            var renderers = target.GetComponentsInChildren<Renderer>(true);
            var color = GetColor(colorType);

            for (int i = 0; i < renderers.Length; i++)
            {
                MaterialPropertyBlock block = new();
                renderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(ColorId, color);
                renderers[i].SetPropertyBlock(block);
            }
        }

        public static Color GetColor(PassengerColorType colorType)
        {
            return colorType switch
            {
                PassengerColorType.Red => new Color(0.9f, 0.25f, 0.25f, 1f),
                PassengerColorType.Green => new Color(0.25f, 0.75f, 0.3f, 1f),
                PassengerColorType.Blue => new Color(0.25f, 0.45f, 0.9f, 1f),
                PassengerColorType.Yellow => new Color(0.95f, 0.8f, 0.2f, 1f),
                PassengerColorType.Purple => new Color(0.65f, 0.35f, 0.85f, 1f),
                _ => Color.white
            };
        }
    }
}