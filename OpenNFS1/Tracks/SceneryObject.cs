using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenNFS1.Loaders;
using GameEngine;
using OpenNFS1.Parsers;
using OpenNFS1;

namespace OpenNFS1.Parsers.Track
{

	abstract class SceneryItem
	{
		public int SegmentRef;
		public Vector3 Position;
		public float Orientation;
		public Vector2 Size;

		public abstract void Initialize();
		public virtual void Update() { }
		public abstract void Render(AlphaTestEffect effect);
	}

	class BillboardSceneryItem : SceneryItem
	{
		protected Matrix _matrix;
		protected Texture2D _texture;

		public BillboardSceneryItem() { }
		public BillboardSceneryItem(Texture2D texture)
		{
			_texture = texture;
		}

		public override void Initialize()
		{
			if (Size.X == 0)
			{
				float aspect = (float)_texture.Width / _texture.Height;
				Size.X = Size.Y * aspect * GameConfig.TerrainScale * 10000;
			}

			_matrix = Matrix.CreateScale(Size.X, Size.Y, 1) *
							Matrix.CreateRotationY(Orientation) *
							Matrix.CreateTranslation(Position);
		}

		public override void Render(AlphaTestEffect effect)
		{
			effect.World = _matrix;
			effect.Texture = _texture;
			effect.CurrentTechnique.Passes[0].Apply();
			Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}
	}

	class AnimatedBillboardSceneryItem : BillboardSceneryItem
	{
		List<Texture2D> _textures;
		int _currentTexture;
		double _textureChangeTime;

		public AnimatedBillboardSceneryItem(List<Texture2D> textures)
		{
			_textures = textures;
			_textureChangeTime = Engine.Instance.Random.NextDouble();
		}

		public override void Initialize()
		{
			if (Size.X == 0)
				Size.X = _textures[0].Width * GameConfig.TerrainScale * 10000;
			if (Size.Y == 0)
				Size.Y = _textures[0].Height * GameConfig.TerrainScale * 10000;

			_matrix = Matrix.CreateScale(Size.X, Size.Y, 1) *
							Matrix.CreateRotationY(Orientation) *
							Matrix.CreateTranslation(Position);
		}

		public override void Update()
		{
			_textureChangeTime -= Engine.Instance.FrameTime;
			if (_textureChangeTime < 0.0f)
			{
				_currentTexture++;
				_currentTexture %= _textures.Count;
				_texture = _textures[_currentTexture];
				_textureChangeTime = 0.2f;
			}
		}
	}

	class TwoSidedBillboardSceneryItem : SceneryItem
	{
		Texture2D _texture1, _texture2;
		Matrix _matrix1, _matrix2;

		public TwoSidedBillboardSceneryItem(Texture2D texture1, Texture2D texture2)
		{
			_texture1 = texture1;
			_texture2 = texture2;
		}

		public override void Initialize()
		{
			_matrix1 = Matrix.CreateScale(Size.X, Size.Y, 1) *
					Matrix.CreateRotationY(Orientation) *
					Matrix.CreateTranslation(Position);

			_matrix2 = Matrix.CreateScale(Size.X, Size.Y, 1) *
					Matrix.CreateTranslation(Size.X / 2, 0, Size.X / 2) *
					Matrix.CreateRotationY(Orientation + MathHelper.ToRadians(90)) *
					Matrix.CreateTranslation(Position);
		}

		public override void Render(AlphaTestEffect effect)
		{
			//side 1
			effect.World = _matrix1;
			effect.Texture = _texture1;
			effect.CurrentTechnique.Passes[0].Apply();
			Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

			//side 2
			effect.World = _matrix2;
			effect.Texture = _texture2;
			effect.CurrentTechnique.Passes[0].Apply();
			Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}
	}

	class ModelSceneryItem : SceneryItem
	{
		Mesh _mesh;
		Matrix _matrix;

		public ModelSceneryItem(Mesh mesh)
		{
			_mesh = mesh;
		}

		public override void Initialize()
		{
			_matrix = Matrix.CreateScale(7.5f) *
				   Matrix.CreateRotationY(Orientation) *
				   Matrix.CreateTranslation(Position);
		}

		public override void Render(AlphaTestEffect effect)
		{
			effect.World = _matrix;
			_mesh.Render(effect);
		}
	}
}
