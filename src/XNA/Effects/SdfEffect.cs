using Microsoft.Xna.Framework.Graphics;

namespace FontStashSharp.Effects
{
	public class SdfEffect : Effect
	{
		private EffectParameter _glyphEdgeParam;

		public float GlyphEdge { get; set; } = 0.5f;

		public SdfEffect(GraphicsDevice device) : base(device, Resources.GetSdfEffectSource())
		{
			_glyphEdgeParam = Parameters["GlyphEdge"];
		}

		protected override void OnApply()
		{
			base.OnApply();

			_glyphEdgeParam.SetValue(GlyphEdge);
		}
	}
}
