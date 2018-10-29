using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace AsiSupport.Managers
{
	public class TextureManager
	{
		private Dictionary<int, Texture> textures;
		private int idCount;
		private List<DrawCall> calls;

		public TextureManager()
		{
			this.textures = new Dictionary<int, Texture>();
			this.idCount = 0;
			this.calls = new List<DrawCall>();
			Game.RawFrameRender += this.Draw;
		}

		public int CreateTexture(string fileName)
		{
			this.textures.Add(this.idCount, Game.CreateTextureFromFile(fileName));
			this.idCount++;

			return this.idCount - 1;
		}

		public void DrawTexture(int id, int time, float x, float y, float width, float height, float rotation, float rotationCenterX, float rotationCenterY)
		{
			this.calls.Add(new DrawCall(id, time, x, y, width, height, rotation, rotationCenterX, rotationCenterY));
		}

		public void Draw(object sender, GraphicsEventArgs args)
		{
			for(int i = 0; i < this.calls.Count; i++)
			{
				DrawCall call = calls[i];

				if(Environment.TickCount <= call.ValidUntil)
				{
					Vector2 position = this.Scale(call.RenderX, call.RenderY);
					Vector2 size = this.Scale(call.Width, call.Height);
					Vector2 rotationCenter = new Vector2(position.X + (size.X * call.RotationCenterX), position.Y + (size.Y * call.RotationCenterY));
					args.Graphics.DrawTexture(this.textures[call.TextureId], position, size, 0.0f, 0.0f, 1.0f, 1.0f, call.Rotation, rotationCenter);
				}
				else
				{
					calls.Remove(call);
					i--;
				}
			}
		}

		private Vector2 Scale(float x, float y)
		{
			return new Vector2(x * Game.Resolution.Width, y * Game.Resolution.Height);
		}

		private class DrawCall
		{
			public int ValidUntil { get; private set; }
			public int TextureId { get; private set; }
			public float RenderX { get; private set; }
			public float RenderY { get; private set; }
			public float Width { get; private set; }
			public float Height { get; private set; }
			public float Rotation { get; private set; }
			public float RotationCenterX { get; private set; }
			public float RotationCenterY { get; private set; }

			public DrawCall(int id, int time, float x, float y, float width, float height, float rotation, float rotationCenterX, float rotationCenterY)
			{
				this.ValidUntil = Environment.TickCount + time;
				this.TextureId = id;
				this.RenderX = x;
				this.RenderY = y;
				this.Width = width;
				this.Height = height;
				this.Rotation = rotation;
				this.RotationCenterX = rotationCenterX;
				this.RotationCenterY = rotationCenterY;
			}
		}
	}
}
