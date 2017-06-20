#pragma once

namespace AsiSupport
{
	public ref class DrawCall
	{
		public:
		property int ValidUntil;
		property int TextureId;
		property float RenderX;
		property float RenderY;
		property float Width;
		property float Height;
		property float Rotation;
		property float RotationCenterX;
		property float RotationCenterY;

		DrawCall(int id, int time, float x, float y, float width, float height, float rotation, float rotationCenterX, float rotationCenterY)
		{
			this->ValidUntil = Environment::TickCount + time;
			this->TextureId = id;
			this->RenderX = x;
			this->RenderY = y;
			this->Width = width;
			this->Height = height;
			this->Rotation = rotation;
			this->RotationCenterX = rotationCenterX;
			this->RotationCenterY = rotationCenterY;
		}
	};

	public ref class TexturesManager
	{
		public:
		static void Initialize();

		static int CreateTexture(String^ fileName);

		static void DrawTexture(int id, int time, float x, float y, float width, float height, float rotation, float rotationCenterX, float rotationCenterY);

		private:
		static int idCount = 0;
		static Dictionary<int, Rage::Texture^>^ textures = gcnew Dictionary<int, Rage::Texture^>();
		static List<DrawCall^>^ calls = gcnew List<DrawCall^>();

		static void Draw(Object^ sender, Rage::GraphicsEventArgs^ args);

		static Rage::Vector2 Scale(float x, float y);
	};
}